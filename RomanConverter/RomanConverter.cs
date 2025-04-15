using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using ToolInterface;

namespace RomanConverter
{
    public class RomanConverterRequest
    {
        public string InputText { get; set; } = "";
    }

    public class RomanConverterTool : ITool
    {
        public string Name => "Roman Converter";
        public string Description => "Convert numbers to and from Roman numerals.";
        public string Category => "Converter";

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json)) return "Invalid Request.";

                var request = JsonSerializer.Deserialize<RomanConverterRequest>(json);
                if (request == null || string.IsNullOrEmpty(request.InputText))
                    return "Invalid Request";

                string result;
                // Kiểm tra xem input có phải là số hay không
                if (int.TryParse(request.InputText, out int number))
                {
                    // Chuyển từ số sang chữ số La Mã
                    result = ToRoman(number);
                }
                else
                {
                    // Kiểm tra chuỗi có phải là roman numeral hợp lệ không
                    if (!IsValidRoman(request.InputText))
                    {
                        return new Dictionary<string, string> { 
                            { "Result", "Error: Invalid Roman numeral" },
                            { "IsError", "true" }
                        };
                    }
                    // Chuyển từ chữ số La Mã sang số
                    result = FromRoman(request.InputText).ToString();
                }

                return new Dictionary<string, string> { { "Result", result } };
            }
            catch (Exception ex)
            {
                return new Dictionary<string, string> { 
                    { "Result", $"Error: {ex.Message}" },
                    { "IsError", "true" }
                };
            }
        }

        // Định nghĩa các cặp ký tự Roman và giá trị tương ứng
        private static readonly Dictionary<string, int> RomanMap = new()
        {
            { "M", 1000 }, { "CM", 900 }, { "D", 500 }, { "CD", 400 },
            { "C", 100 }, { "XC", 90 }, { "L", 50 }, { "XL", 40 },
            { "X", 10 }, { "IX", 9 }, { "V", 5 }, { "IV", 4 }, { "I", 1 }
        };

        // Kiểm tra xem chuỗi có phải là Roman numeral hợp lệ không
        private bool IsValidRoman(string input)
        {
            // Regex kiểm tra Roman numeral hợp lệ
            // Chỉ cho phép các ký tự: I, V, X, L, C, D, M và một số quy tắc cơ bản
            string pattern = @"^(?=[MDCLXVI])M*(C[MD]|D?C{0,3})(X[CL]|L?X{0,3})(I[XV]|V?I{0,3})$";
            return Regex.IsMatch(input.ToUpper(), pattern);
        }

        // Chuyển đổi từ số sang Roman numeral
        private string ToRoman(int num)
        {
            if (num <= 0 || num > 3999) 
                throw new ArgumentException("Number out of range (1-3999)");
            
            var result = "";
            foreach (var pair in RomanMap)
            {
                while (num >= pair.Value)
                {
                    result += pair.Key;
                    num -= pair.Value;
                }
            }
            return result;
        }

        // Chuyển đổi từ Roman numeral sang số
        private int FromRoman(string roman)
        {
            roman = roman.ToUpper();
            int result = 0;
            int i = 0;

            while (i < roman.Length)
            {
                // Kiểm tra ký tự kép trước
                if (i + 1 < roman.Length)
                {
                    string doubleSymbol = roman.Substring(i, 2);
                    if (RomanMap.ContainsKey(doubleSymbol))
                    {
                        result += RomanMap[doubleSymbol];
                        i += 2;
                        continue;
                    }
                }

                // Kiểm tra ký tự đơn
                string singleSymbol = roman[i].ToString();
                if (RomanMap.ContainsKey(singleSymbol))
                {
                    result += RomanMap[singleSymbol];
                    i++;
                }
                else
                {
                    throw new ArgumentException($"Invalid Roman numeral: {roman}");
                }
            }
            Console.WriteLine($"Converted Roman numeral: {roman} = {result}");
            return result;
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 600px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>Roman Converter</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Convert numbers to and from Roman numerals.</p>
                    </div>
                    <div class='card shadow-lg p-4'>
                        <div class='mb-3'>
                            <label class='form-label fw-bold'>Input:</label>
                            <input id='inputText' class='form-control' placeholder='Enter a number (1-3999) or Roman numeral (e.g. MCMLXXXIV)'>
                        </div>
                        <button id='convert' class='btn btn-primary w-100 my-4'>Convert</button>
                        <div id='result' class='d-flex align-items-center'>
                            <label class='form-label fw-bold me-2'>Result:</label>
                            <div id='convertedValue' class='p-2 m-2 bg-light border rounded flex-grow-1 text-center' 
                                style='min-height: 40px; display: flex; align-items: center; justify-content: center;'>
                            </div>
                                <button id='copyButton' class='btn btn-outline-secondary ms-2' title='Copy to clipboard'>
                                    <i class='bi bi-clipboard'></i>
                                </button>
                        </div>
                    </div>
                </div>
                <script>
                    document.getElementById('convert').addEventListener('click', function() {
                        let inputText = document.getElementById('inputText').value.trim();
                        let resultElement = document.getElementById('convertedValue');
                        const copyButton = document.getElementById('copyButton');
                        if (inputText === '') return;
                        
                        fetch(window.location.pathname + '/execute', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ InputText: inputText })
                        })
                        .then(response => response.json())
                        .then(data => {
                            resultElement.textContent = data.result.Result;
                            if (data.IsError === 'true') {
                                resultElement.classList.add('text-danger');
                            } else {
                                resultElement.classList.remove('text-danger');
                            }
                        })
                        .catch(error => {
                            console.error('Error:', error);
                            document.getElementById('convertedValue').textContent = 'An error occurred';
                            document.getElementById('convertedValue').classList.add('text-danger');
                        });
                    });
                    
                    // Enter key to convert
                    document.getElementById('inputText').addEventListener('keyup', function(event) {
                        if (event.key === 'Enter') {
                            document.getElementById('convert').click();
                        }
                    });

                    // Xử lý nút copy
                    copyButton.addEventListener('click', function() {
                            const numberValue = document.getElementById('convertedValue').textContent;
                            navigator.clipboard.writeText(numberValue)
                                .then(() => {
                                    const originalText = copyButton.innerHTML;
                                    copyButton.innerHTML = '<i class=""bi bi-check""></i>';
                                    
                                    setTimeout(() => {
                                        copyButton.innerHTML = originalText;
                                    }, 1500);
                                })
                                .catch(err => {
                                    console.error('Could not copy text: ', err);
                                });
                        });
                        
                        // Generate a port on page load (optional)
                        // generateButton.click();
                </script>
            ";
        }

        public void Stop()
        {
            return;
        }
    }
}
