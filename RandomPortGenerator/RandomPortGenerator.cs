using System;
using System.Text.Json;
using System.Xml.XPath;
using ToolInterface;

namespace RandomPortGenerator
{
    public class RandomPortRequest { }

    public class RandomPortGeneratorTool : ITool
    {
        public string Name => "Random Port Generator";
        public string Description => "Generate random port numbers outside of the range of 'known' ports (0-1023).";
        public string Category => "Networking";

        private static readonly Random random = new Random();

        public object Execute(object? input)
        {
            try
            {
                // Deserialize request - even if it's empty
                var request = input is string jsonString 
                    ? JsonSerializer.Deserialize<RandomPortRequest>(jsonString) 
                    : new RandomPortRequest();

                // Generate random port
                int randomPort = random.Next(1024, 65536);
                var result = new { Port = randomPort };
                Console.WriteLine($"Generated Port: {randomPort}");
                return result;
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 500px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>Random Port Generator</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Generate random port numbers outside of the range of 'known' ports (0-1023).</p>
                    </div>
                    <div class='card shadow-lg p-4'>
                        <button id='generateButton' class='btn btn-primary w-100'>Generate</button>
                        <div id='resultContainer' class='mt-3' style='display: none;'>
                            <label class='form-label fw-bold'>Generated Port:</label>
                            <div class='d-flex align-items-center'>
                                <textarea id='result' class='alert alert-success flex-grow-1 mb-0'></textarea>
                                <button id='copyButton' class='btn btn-outline-secondary ms-2' title='Copy to clipboard'>
                                    <i class='bi bi-clipboard'></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <script>
                    document.addEventListener('DOMContentLoaded', function() {
                        const generateButton = document.getElementById('generateButton');
                        const resultContainer = document.getElementById('resultContainer');
                        const copyButton = document.getElementById('copyButton');
                        let portArea = document.getElementById('result');
                        
                        // Status variables
                        let isGenerating = false;
                        
                        generateButton.addEventListener('click', function() {
                            if (isGenerating) return;
                            
                            // Set loading state
                            isGenerating = true;
                            generateButton.disabled = true;
                            generateButton.innerHTML = '<span class=""spinner-border spinner-border-sm"" role=""status"" aria-hidden=""true""></span> Generating...';
                            
                            // Make the API call to the backend
                            fetch(window.location.pathname + '/execute', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({}) // Gửi object rỗng thay vì biến data chưa được định nghĩa
                            })
                            .then(response => response.json())
                            .then(data => {
                                // Reset button state
                                generateButton.disabled = false;
                                generateButton.textContent = 'Generate';
                                isGenerating = false;
                                
                                // Display the result
                                if (data.result) {
                                    portArea.value = data.result.port; // Gán giá trị port vào textarea
                                    resultContainer.style.display = 'block';
                                } else if (data.Error) {
                                    portArea.value = 'Error: ' + data.Error;
                                    portArea.className = 'alert alert-danger flex-grow-1 mb-0';
                                    portArea.style.display = 'block';
                                }
                            })
                            .catch(error => {
                                // Reset button and show error
                                generateButton.disabled = false;
                                generateButton.textContent = 'Generate';
                                isGenerating = false;
                                
                                console.error('Error:', error);
                                portArea.value = 'Failed to generate port. Please try again.';
                                portArea.className = 'alert alert-danger flex-grow-1 mb-0';
                                resultContainer.style.display = 'block';
                            });
                        });
                        
                        // Add copy functionality
                        copyButton.addEventListener('click', function() {
                            const portValue = portArea.value;
                            navigator.clipboard.writeText(portValue)
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
                    });
                </script>
            ";
        }

        public void Stop() { }
    }
}
