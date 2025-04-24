using System;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ToolInterface;
using SHA3.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HashText
{
    public class HashTextRequest
    {
        public string InputText { get; set; } = "";
        public string OutputFormat { get; set; } = "Hex";
    }

    public class HashTextTool : ITool
    {
        public string Name => "Hash Text";
        public string Description => "Generate hash values using various algorithms.";
        public string Category => "Crypto";

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json)) return new { result = "Invalid Request." };

                var request = JsonSerializer.Deserialize<HashTextRequest>(json);
                if (request == null || string.IsNullOrEmpty(request.InputText))
                    return new { result = "Invalid Request" };

                var hashResults = new Dictionary<string, string>
                {
                    { "MD5", ComputeHash(request.InputText, MD5.Create(), request.OutputFormat) },
                    { "SHA1", ComputeHash(request.InputText, SHA1.Create(), request.OutputFormat) },
                    { "SHA256", ComputeHash(request.InputText, SHA256.Create(), request.OutputFormat) },
                    { "SHA512", ComputeHash(request.InputText, SHA512.Create(), request.OutputFormat) },
                    { "SHA3", ComputeHash(request.InputText, Sha3.Sha3512(), request.OutputFormat) },
                    { "SHA384", ComputeHash(request.InputText, Sha3.Sha3384(), request.OutputFormat) }
                };

                return new { result = hashResults }; 
            }
            catch (Exception ex)
            {
                return new { result = $"Error: {ex.Message}" };
            }
        }

        private string ComputeHash(string input, HashAlgorithm algorithm, string format)
        {
            byte[] hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            return format switch
            {
                "Hex" => BitConverter.ToString(hashBytes).Replace("-", "").ToLower(),
                "Binary" => string.Join("", hashBytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))),
                "Base64" => Convert.ToBase64String(hashBytes),
                _ => throw new ArgumentException("Unsupported format")
            };
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 600px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>Hash Text</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Generate hash values using various algorithms (MD5, SHA1, SHA256, SHA512, SHA3, SHA384).</p>
                    </div>
                    <div class='card shadow-lg p-4'>
                        <div class='mb-3'>
                            <label class='form-label fw-bold'>Input Text:</label>
                            <textarea id='inputText' class='form-control' rows='2' placeholder='Enter text to hash...'></textarea>
                        </div>
                        <div class='mb-3'>
                            <label class='form-label fw-bold'>Digest Encoding:</label>
                            <select id='outputFormat' class='form-select'>
                                <option value='Hex'>Hexadecimal (base 16)</option>
                                <option value='Binary'>Binary (base 2)</option>
                                <option value='Base64'>Base64 (base 64)</option>
                            </select>
                        </div>
                        <button id='generateHash' class='btn btn-primary w-100 my-4'>Generate Hash</button>
                        <div id=""result"" class=""mb-3"">
                            <label class='form-label fw-bold'>Hash result:</label>
                            <table class=""table table-bordered w-100"" style=""table-layout: fixed;"">
                                <tbody> 
                                    <tr>
                                        <td class=""w-25"" style=""background-color: #f1f5f9;"">MD5</td>
                                        <td id=""md5"" class=""text-truncate width-auto""></td>
                                        <td class=""text-center"" style=""width: 30px;"">
                                            <button class=""btn btn-sm copy-btn p-0"" data-target=""md5"">
                                                <i class=""bi bi-copy""></i>
                                            </button>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""w-25"" style=""background-color: #f1f5f9;"">SHA1</td>
                                        <td id=""sha1"" class=""text-truncate width-auto""></td>
                                        <td class=""text-center"" style=""width: 30px;"">
                                            <button class=""btn btn-sm copy-btn p-0"" data-target=""sha1"">
                                                <i class=""bi bi-copy""></i>
                                            </button>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""w-25"" style=""background-color: #f1f5f9;"">SHA256</td>
                                        <td id=""sha256"" class=""text-truncate width-auto""></td>
                                        <td class=""text-center"" style=""width: 30px;"">
                                            <button class=""btn btn-sm copy-btn p-0"" data-target=""sha256"">
                                                <i class=""bi bi-copy""></i>
                                            </button>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""w-25"" style=""background-color: #f1f5f9;"">SHA512</td>
                                        <td id=""sha512"" class=""text-truncate width-auto""></td>
                                        <td class=""text-center"" style=""width: 30px;"">
                                            <button class=""btn btn-sm copy-btn p-0"" data-target=""sha512"">
                                                <i class=""bi bi-copy""></i>
                                            </button>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""w-25"" style=""background-color: #f1f5f9;"">SHA3</td>
                                        <td id=""sha3"" class=""text-truncate width-auto""></td>
                                        <td class=""text-center"" style=""width: 30px;"">
                                            <button class=""btn btn-sm copy-btn p-0"" data-target=""sha3"">
                                                <i class=""bi bi-copy""></i>
                                            </button>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class=""w-25"" style=""background-color: #f1f5f9;"">SHA384</td>
                                        <td id=""sha384"" class=""text-truncate width-auto""></td>
                                        <td class=""text-center"" style=""width: 30px;"">
                                            <button class=""btn btn-sm copy-btn p-0"" data-target=""sha384"">
                                                <i class=""bi bi-copy""></i>
                                            </button>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                <script>
                    let inputTextArea = document.getElementById('inputText');

                    inputTextArea.addEventListener('input', function() {
                        inputTextArea.classList.remove('border', 'border-danger');
                        inputTextArea.placeholder = 'Enter text to hash...';
                    });

                    document.getElementById('generateHash').addEventListener('click', function() {
                        let inputText = inputTextArea.value.trim();
                        let outputFormat = document.getElementById('outputFormat').value;

                        if (inputText === '') {
                            inputTextArea.placeholder = 'Please enter text to hash.'; 
                            inputTextArea.classList.add('border', 'border-danger');
                            inputTextArea.focus();
                            return;
                        }

                        let data = { InputText: inputText, OutputFormat: outputFormat };

                        fetch(window.location.pathname + '/execute', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify(data)
                        })
                        .then(response => response.json())
                        .then(data => {
                            return data.result.result;
                        })
                        .then(hashResults => {
                            Object.keys(hashResults).forEach(algo => {
                                let hashElement = document.getElementById(algo.toLowerCase());
                                if (hashElement) {
                                    hashElement.textContent = hashResults[algo];
                                }
                            });
                        })
                        .catch(error => console.error('Error:', error));
                    });

                    // Copy hash result to clipboard
                    document.querySelectorAll('.copy-btn').forEach(button => {
                        button.addEventListener('click', function() {
                            let targetId = button.getAttribute('data-target');
                            let hashText = document.getElementById(targetId)?.textContent;

                            if (hashText) {
                                navigator.clipboard.writeText(hashText).then(() => {
                                    button.innerHTML = '<i class=""bi bi-check""></i>';
                                    setTimeout(() => button.innerHTML = '<i class=""bi bi-copy""></i>', 1000);
                                }).catch(err => console.error('Copy failed:', err));
                            }
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
