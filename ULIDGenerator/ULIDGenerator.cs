using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using ToolInterface;

namespace ULIDGenerator
{
    /// <summary>
    /// Công cụ tạo ULID - Universally Unique Lexicographically Sortable Identifier
    /// </summary>
    public class ULIDGeneratorTool : ITool
    {
        public string Name => "ULID Generator";
        public string Description => "Generate ULIDs (Universally Unique Lexicographically Sortable Identifiers)";
        public string Category => "Crypto";

        private static readonly char[] Base32Chars = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToCharArray();
        private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

        public object Execute(object? input)
        {
            try
            {
                // Parse input parameters
                var jsonString = input?.ToString() ?? "{}";
                var parameters = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);
                
                // Extract parameters with defaults
                int count = 1;
                string format = "raw";
                
                if (parameters != null)
                {
                    // Xử lý count
                    if (parameters.TryGetValue("count", out var countElement))
                    {
                        if (countElement.ValueKind == JsonValueKind.Number)
                        {
                            count = countElement.GetInt32();
                        }
                        else if (countElement.ValueKind == JsonValueKind.String && 
                                int.TryParse(countElement.GetString(), out var parsedCount))
                        {
                            count = parsedCount;
                        }
                    }
                    
                    // Xử lý format
                    if (parameters.TryGetValue("format", out var formatElement) && 
                        formatElement.ValueKind == JsonValueKind.String)
                    {
                        format = formatElement.GetString()?.ToLower() ?? "raw";
                    }
                }
                
                // Validate count
                if (count < 1) count = 1;
                if (count > 100) count = 100; // Limit to 100 to prevent abuse
                
                // Generate ULIDs
                var ulids = new List<string>();
                var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                
                for (int i = 0; i < count; i++)
                {
                    ulids.Add(GenerateULID());
                }
                
                // Return result based on format
                if (format == "json")
                {
                    return new
                    {
                        Count = ulids.Count,
                        Timestamp = timestamp,
                        Ulids = ulids
                    };
                }
                else // Raw format
                {
                    return new
                    {
                        Count = ulids.Count,
                        Timestamp = timestamp,
                        Ulids = string.Join("\n", ulids)
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ULIDGenerator: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new { Error = ex.Message };
            }
        }

        /// <summary>
        /// Tạo một ULID mới
        /// </summary>
        /// <returns>Chuỗi ULID</returns>
        public string GenerateULID()
        {
            // Lấy timestamp hiện tại
            var timestamp = DateTimeOffset.UtcNow;
            
            // 6 byte đầu tiên là timestamp
            byte[] timestampBytes = BitConverter.GetBytes(timestamp.ToUnixTimeMilliseconds());
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }
            
            // Chỉ sử dụng 6 byte cuối của timestamp (48-bit)
            byte[] timestampPart = new byte[6];
            Array.Copy(timestampBytes, timestampBytes.Length - 6, timestampPart, 0, 6);
            
            // 10 byte tiếp theo là random
            byte[] randomBytes = new byte[10];
            Rng.GetBytes(randomBytes);
            
            // Kết hợp timestamp và phần ngẫu nhiên
            byte[] ulidBytes = new byte[16];
            Array.Copy(timestampPart, 0, ulidBytes, 0, 6);
            Array.Copy(randomBytes, 0, ulidBytes, 6, 10);
            
            // Mã hóa sang base32
            return ToBase32String(ulidBytes);
        }

        /// <summary>
        /// Chuyển đổi mảng byte thành chuỗi Base32
        /// </summary>
        private string ToBase32String(byte[] bytes)
        {
            StringBuilder result = new StringBuilder();
            int bits = 0;
            int bitsRemaining = 0;
            
            foreach (byte b in bytes)
            {
                bits = (bits << 8) | b;
                bitsRemaining += 8;
                
                while (bitsRemaining >= 5)
                {
                    bitsRemaining -= 5;
                    int index = (bits >> bitsRemaining) & 0x1F;
                    result.Append(Base32Chars[index]);
                }
            }
            
            // Xử lý bit còn lại (nếu có)
            if (bitsRemaining > 0)
            {
                bits = bits << (5 - bitsRemaining);
                int index = bits & 0x1F;
                result.Append(Base32Chars[index]);
            }
            
            return result.ToString();
        }

        /// <summary>
        /// Trả về UI của công cụ
        /// </summary>
        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 600px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>ULID Generator</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Generate ULIDs (Universally Unique Lexicographically Sortable Identifiers).</p>
                    </div>
                    <div class='card shadow-lg p-4'>
                        <div class='mb-3 row'>
                            <div class='col-md-6'>
                                <label for='countInput' class='form-label'>Number of ULIDs:</label>
                                <input type='number' class='form-control' id='countInput' min='1' max='100' value='1'>
                            </div>
                            <div class='col-md-6'>
                                <label for='formatSelect' class='form-label'>Output Format:</label>
                                <select class='form-select' id='formatSelect'>
                                    <option value='raw' selected>Raw (Plain Text)</option>
                                    <option value='json'>JSON</option>
                                </select>
                            </div>
                        </div>

                        <button id='generateButton' class='btn btn-primary w-100'>Generate ULID</button>
                        
                        <div id='resultContainer' class='mt-4' style='display: none;'>
                            <div class='mb-3'>
                                <div class='d-flex justify-content-between align-items-center mb-2'>
                                    <label class='form-label fw-bold mb-0'>Generated ULIDs:</label>
                                    <button id='copyUlidButton' class='btn btn-sm btn-outline-secondary'>
                                        <i class='bi bi-clipboard'></i> Copy
                                    </button>
                                </div>
                                <textarea id='ulidResult' class='form-control' rows='5' readonly style='font-family: monospace;'></textarea>
                            </div>
                            <div class='mb-0 d-flex justify-content-between'>
                                <div>
                                    <span class='fw-bold'>Timestamp:</span>
                                    <span id='timestampResult' class='text-muted ms-2'></span>
                                </div>
                                <div>
                                    <span class='fw-bold'>Count:</span>
                                    <span id='countResult' class='text-muted ms-2'></span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <script>
                    document.addEventListener('DOMContentLoaded', function() {
                        const generateButton = document.getElementById('generateButton');
                        const resultContainer = document.getElementById('resultContainer');
                        const ulidResult = document.getElementById('ulidResult');
                        const timestampResult = document.getElementById('timestampResult');
                        const countResult = document.getElementById('countResult');
                        const copyUlidButton = document.getElementById('copyUlidButton');
                        const countInput = document.getElementById('countInput');
                        const formatSelect = document.getElementById('formatSelect');
                        
                        // Validate count input
                        countInput.addEventListener('change', function() {
                            let value = parseInt(this.value);
                            if (isNaN(value) || value < 1) this.value = 1;
                            if (value > 100) this.value = 100;
                        });
                        
                        // Tạo ULID
                        generateButton.addEventListener('click', function() {
                            generateButton.disabled = true;
                            generateButton.innerHTML = '<span class=""spinner-border spinner-border-sm"" role=""status"" aria-hidden=""true""></span> Generating...';
                            
                            const count = parseInt(countInput.value) || 1;
                            const format = formatSelect.value;
                            
                            fetch(window.location.pathname + '/execute', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({
                                    count: count,
                                    format: format
                                })
                            })
                            .then(response => response.json())
                            .then(data => {
                                generateButton.disabled = false;
                                generateButton.textContent = 'Generate ULID';
                                
                                if (data.Error) {
                                    alert('Error: ' + data.Error);
                                    return;
                                }
                                console.log(data);
                                // Hiển thị kết quả
                                if (format === 'json') {
                                    ulidResult.value = JSON.stringify(data.result.ulids, null, 2);
                                } else {
                                    ulidResult.value = data.result.ulids;
                                }
                                
                                timestampResult.textContent = data.result.timestamp + ' UTC';
                                countResult.textContent = data.result.count;
                                resultContainer.style.display = 'block';
                            })
                            .catch(error => {
                                generateButton.disabled = false;
                                generateButton.textContent = 'Generate ULID';
                                console.error('Error:', error);
                                alert('Failed to generate ULID. Please try again.');
                            });
                        });
                        
                        // Sao chép vào clipboard
                        copyUlidButton.addEventListener('click', function() {
                            ulidResult.select();
                            document.execCommand('copy');
                            
                            // Cung cấp phản hồi trực quan
                            const originalContent = copyUlidButton.innerHTML;
                            copyUlidButton.innerHTML = '<i class=""bi bi-check2""></i> Copied!';
                            
                            setTimeout(() => {
                                copyUlidButton.innerHTML = originalContent;
                            }, 1500);
                        });
                    });
                </script>
            ";
        }

        public void Stop()
        {
            return; 
        }
    }
}
