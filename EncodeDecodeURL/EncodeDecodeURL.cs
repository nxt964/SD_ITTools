using System;
using System.Text;
using System.Web;
using System.Text.Json;
using ToolInterface;

namespace EncodeDecodeURL
{
    public class EncodeDecodeURLRequest
    {
        public string Text { get; set; } = "";
        public string Action { get; set; } = "encode"; // "encode" or "decode"
    }

    public class EncodeDecodeURLTool : ITool
    {
        public string Name => "URL Encoder Decoder";
        public string Description => "Encode or decode URLs and URL components";
        public string Category => "Web";

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json))
                    return new { Error = "Invalid request. Input is empty." };

                var request = JsonSerializer.Deserialize<EncodeDecodeURLRequest>(json);
                if (request == null)
                    return new { Error = "Invalid request format." };

                if (string.IsNullOrEmpty(request.Text))
                    return new { Result = "" };

                string result;
                if (request.Action.ToLower() == "encode")
                {
                    result = HttpUtility.UrlEncode(request.Text);
                }
                else if (request.Action.ToLower() == "decode")
                {
                    // Try to handle potential URL decode errors
                    try
                    {
                        result = HttpUtility.UrlDecode(request.Text);
                    }
                    catch (Exception ex)
                    {
                        return new { Error = $"Failed to decode: {ex.Message}" };
                    }
                }
                else
                {
                    return new { Error = "Invalid action. Use 'encode' or 'decode'." };
                }

                return new { Result = result };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 900px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>URL Encoder/Decoder</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Encode or decode URLs and URL components in real-time.</p>
                    </div>
                    
                    <div class='row g-4'>
                        <!-- Encoder Section -->
                        <div class='col-md-6'>
                            <div class='card shadow h-100'>
                                <div class='card-header bg-primary text-white'>
                                    <h5 class='card-title mb-0'>URL Encoder</h5>
                                </div>
                                <div class='card-body d-flex flex-column'>
                                    <label for='encodeInput' class='form-label'>Plain Text:</label>
                                    <textarea id='encodeInput' class='form-control flex-grow-1 mb-3' rows='8' 
                                        placeholder='Type or paste text to encode...'></textarea>
                                    
                                    <div class='d-flex justify-content-between align-items-center mb-2'>
                                        <label class='form-label mb-0'>URL Encoded Result:</label>
                                        <button id='copyEncoded' class='btn btn-sm btn-outline-secondary'>
                                            <i class='bi bi-clipboard'></i> Copy
                                        </button>
                                    </div>
                                    <textarea id='encodeOutput' class='form-control flex-grow-1 bg-light' rows='8' readonly></textarea>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Decoder Section -->
                        <div class='col-md-6'>
                            <div class='card shadow h-100'>
                                <div class='card-header bg-success text-white'>
                                    <h5 class='card-title mb-0'>URL Decoder</h5>
                                </div>
                                <div class='card-body d-flex flex-column'>
                                    <label for='decodeInput' class='form-label'>URL Encoded Text:</label>
                                    <textarea id='decodeInput' class='form-control flex-grow-1 mb-3' rows='8'
                                        placeholder='Type or paste URL encoded text to decode...'></textarea>
                                    
                                    <div class='d-flex justify-content-between align-items-center mb-2'>
                                        <label class='form-label mb-0'>Decoded Result:</label>
                                        <button id='copyDecoded' class='btn btn-sm btn-outline-secondary'>
                                            <i class='bi bi-clipboard'></i> Copy
                                        </button>
                                    </div>
                                    <textarea id='decodeOutput' class='form-control flex-grow-1 bg-light' rows='8' readonly></textarea>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class='row mt-4'>
                        <div class='col-12'>
                            <div class='card shadow-sm'>
                                <div class='card-body'>
                                    <h5>About URL Encoding</h5>
                                    <p>URL encoding replaces unsafe ASCII characters with a ""<code>%</code>"" followed by two hexadecimal digits. URLs cannot contain spaces and certain special characters.</p>
                                    <p class='mb-0'><strong>Common URL Encodings:</strong> Space = <code>%20</code>, / = <code>%2F</code>, ? = <code>%3F</code>, & = <code>%26</code>, = = <code>%3D</code>, # = <code>%23</code>, + = <code>%2B</code></p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <script>
                    document.addEventListener('DOMContentLoaded', function() {
                        // Elements for encoding
                        const encodeInput = document.getElementById('encodeInput');
                        const encodeOutput = document.getElementById('encodeOutput');
                        const copyEncoded = document.getElementById('copyEncoded');
                        
                        // Elements for decoding
                        const decodeInput = document.getElementById('decodeInput');
                        const decodeOutput = document.getElementById('decodeOutput');
                        const copyDecoded = document.getElementById('copyDecoded');
                        
                        // Debounce function to prevent too many API calls
                        function debounce(func, wait) {
                            let timeout;
                            return function executedFunction(...args) {
                                const later = () => {
                                    clearTimeout(timeout);
                                    func(...args);
                                };
                                clearTimeout(timeout);
                                timeout = setTimeout(later, wait);
                            };
                        }
                        
                        // Function to encode text
                        const encodeText = debounce(function(text) {
                            if (!text) {
                                encodeOutput.value = '';
                                return;
                            }
                            
                            fetch(window.location.pathname + '/execute', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({
                                    Text: text,
                                    Action: 'encode'
                                })
                            })
                            .then(response => response.json())
                            .then(data => {
                                console.log(data);
                                if (data.Error) {
                                    encodeOutput.value = `Error: ${data.Error}`;
                                } else {
                                    encodeOutput.value = data.result.result;
                                }
                            })
                            .catch(error => {
                                console.error('Error:', error);
                                encodeOutput.value = 'Error processing request';
                            });
                        }, 300);
                        
                        // Function to decode text
                        const decodeText = debounce(function(text) {
                            if (!text) {
                                decodeOutput.value = '';
                                return;
                            }
                            
                            fetch(window.location.pathname + '/execute', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({
                                    Text: text,
                                    Action: 'decode'
                                })
                            })
                            .then(response => response.json())
                            .then(data => {
                                console.log(data);
                                if (data.Error) {
                                    decodeOutput.value = `Error: ${data.Error}`;
                                } else {
                                    decodeOutput.value = data.result.result;
                                }
                            })
                            .catch(error => {
                                console.error('Error:', error);
                                decodeOutput.value = 'Error processing request';
                            });
                        }, 300);
                        
                        // Event listeners for real-time encoding/decoding
                        encodeInput.addEventListener('input', function() {
                            encodeText(this.value);
                        });
                        
                        decodeInput.addEventListener('input', function() {
                            decodeText(this.value);
                        });
                        
                        // Copy functionality for encode result
                        copyEncoded.addEventListener('click', function() {
                            encodeOutput.select();
                            document.execCommand('copy');
                            
                            const originalHTML = copyEncoded.innerHTML;
                            copyEncoded.innerHTML = '<i class=""bi bi-check-lg""></i> Copied!';
                            
                            setTimeout(() => {
                                copyEncoded.innerHTML = originalHTML;
                            }, 1500);
                        });
                        
                        // Copy functionality for decode result
                        copyDecoded.addEventListener('click', function() {
                            decodeOutput.select();
                            document.execCommand('copy');
                            
                            const originalHTML = copyDecoded.innerHTML;
                            copyDecoded.innerHTML = '<i class=""bi bi-check-lg""></i> Copied!';
                            
                            setTimeout(() => {
                                copyDecoded.innerHTML = originalHTML;
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
