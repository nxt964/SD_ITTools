using System;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ToolInterface;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Diagnostics.Metrics;
using System.Net.Mime;
using System.Reflection.Emit;
using System.Xml.Linq;
using System.Text.Json;

namespace TokenGenerator
{
    public class TokenGeneratorRequest
    {
        public int Length { get; set; }
        public bool IncludeUppercase { get; set; }
        public bool IncludeLowercase { get; set; }
        public bool IncludeNumbers { get; set; }
        public bool IncludeSymbols { get; set; }
    }

    public class TokenGeneratorTool : ITool
    {
        private Task? _runningTask;
        private CancellationTokenSource _cts = new();
        private bool _isRunning = false;

        public string Name => "Token Generator";
        public string Description => "Generate random tokens with customizable characters.";

        public string Category => "Crypto";

        public object Execute(object? input)
        {
            _cts = new CancellationTokenSource();
            _isRunning = true;

            _runningTask = Task.Run(() => {}, _cts.Token);

            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json))
                {
                    return "Invalid Request.";
                }

                var request = JsonSerializer.Deserialize<TokenGeneratorRequest>(json);
                if (request == null)
                {
                    return "Invalid Request";
                }

                int length = request.Length;
                bool includeUppercase = request.IncludeUppercase;
                bool includeLowercase = request.IncludeLowercase;
                bool includeNumbers = request.IncludeNumbers;
                bool includeSymbols = request.IncludeSymbols;


                // Gọi hàm sinh token
                return GenerateToken(length, includeUppercase, includeLowercase, includeNumbers, includeSymbols);
            }
            catch (Exception ex)
            {
                return $"Invalid input: {ex.Message}";
            }
        }

        public string GetUI()
        {
            return @"
                <div class=""container py-5 mx-auto"" style=""max-width: 600px;"">
                    <div class=""header mb-4"">
                        <h1 class=""text-start m-0"">Token Generator</h1>
                        <div class=""separator my-2"" style=""width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1"" ></div>
                        <p class=""text-start text-muted mb-0"">Generate a random string with the chars you want.</p>
                    </div>
                    <div class=""card shadow-lg p-4"">
                        <div class=""mb-3"">
                            <label class=""form-label fw-bold"">Options:</label>
                            <div class=""row justify-content-center align-items-center"" style=""row-gap: 10px;"">
                                <div class=""col-6"">
                                    <div class=""form-check form-switch"">
                                        <input class=""form-check-input"" type=""checkbox"" id=""uppercase"" checked>
                                        <label class=""form-check-label"">Uppercase (ABC...)</label>
                                    </div>
                                </div>
                                <div class=""col-6"">
                                    <div class=""form-check form-switch"">
                                        <input class=""form-check-input"" type=""checkbox"" id=""lowercase"" checked>
                                        <label class=""form-check-label"">Lowercase (abc...)</label>
                                    </div>
                                </div>
                                <div class=""col-6"">
                                    <div class=""form-check form-switch"">
                                        <input class=""form-check-input"" type=""checkbox"" id=""numbers"" checked>
                                        <label class=""form-check-label"">Numbers (123...)</label>
                                    </div>
                                </div>
                                <div class=""col-6"">
                                    <div class=""form-check form-switch"">
                                        <input class=""form-check-input"" type=""checkbox"" id=""symbols"">
                                        <label class=""form-check-label"">Symbols (!@#...)</label>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class=""mb-3"">
                            <label class=""form-label fw-bold"">
                                Length: <span id=""lengthValue"">64</span>
                            </label>
                            <input type=""range"" class=""form-range"" id=""length"" min=""1"" max=""512"" value=""64"">
                        </div>

                        <div class=""mb-3"">
                            <textarea id=""token"" class=""form-control"" rows=""2"" readonly>The generated token will appear here...</textarea>
                        </div>

                        <div class=""d-flex justify-content-center gap-2"">
                            <button id=""copyToken"" class=""btn btn-secondary"">Copy</button>  
                            <button id=""generateToken"" class=""btn btn-primary"">Generate</button>
                        </div>
                    </div>
                </div>

                <script>
                    document.getElementById('length').addEventListener('input', function() {
                        document.getElementById('lengthValue').innerText = this.value;
                    });

                    document.getElementById('copyToken').addEventListener('click', function() {
                        let token = document.getElementById('token');
                        token.select();
                        document.execCommand('copy');
                    });

                    document.getElementById('generateToken').addEventListener('click', function() {
                        let length = parseInt(document.getElementById('length').value, 10);
                        let useUppercase = document.getElementById('uppercase').checked;
                        let useLowercase = document.getElementById('lowercase').checked;
                        let useNumbers = document.getElementById('numbers').checked;
                        let useSymbols = document.getElementById('symbols').checked;

                        let data = {
                            Length: length,
                            IncludeUppercase: useUppercase,
                            IncludeLowercase: useLowercase,
                            IncludeNumbers: useNumbers,
                            IncludeSymbols: useSymbols
                        };

                        fetch(window.location.pathname + '/execute', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify(data)
                        })
                        .then(response => response.json())
                        .then(data => {
                                let tokenArea = document.getElementById('token');
                                tokenArea.value = data.result;
                                tokenArea.style.height = 'auto';
                                tokenArea.style.height = tokenArea.scrollHeight + 'px';
                        })
                        .catch(error => console.error('Error:', error));
                    });
                </script>";
        }


        public void Stop()
        {
            if (!_isRunning) return;

            _cts.Cancel();

            try
            {
                _runningTask?.Wait(); // Đảm bảo task dừng hẳn
            }
            catch (Exception) { }

            _cts.Dispose();
            _isRunning = false;
        }

        private string GenerateToken(int length, bool includeUppercase, bool includeLowercase, bool includeNumbers, bool includeSymbols)
        {
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string symbols = "!@#$%^&*()-_=+[]{}|;:'\",.<>?/";

            StringBuilder charPool = new StringBuilder();
            if (includeUppercase) charPool.Append(uppercase);
            if (includeLowercase) charPool.Append(lowercase);
            if (includeNumbers) charPool.Append(numbers);
            if (includeSymbols) charPool.Append(symbols);

            if (charPool.Length == 0)
                return "Please select at least one character type.";

            Random rnd = new Random();
            return new string(Enumerable.Range(0, length)
                .Select(_ => charPool[rnd.Next(charPool.Length)])
                .ToArray());
        }
    }

}