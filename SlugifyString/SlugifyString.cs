using System;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using ToolInterface;

namespace SlugifyString
{
    public class SlugifyRequest
    {
        public string Text { get; set; } = "";
        public bool LowerCase { get; set; } = true;
        public string Separator { get; set; } = "-";
        public bool RemoveDiacritics { get; set; } = true;
    }

    public class SlugifyStringTool : ITool
    {
        public string Name => "Slugify String";
        public string Description => "Convert strings to URL-friendly slugs";
        public string Category => "Web";

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json))
                    return new { Error = "Invalid request. Input is empty." };

                var request = JsonSerializer.Deserialize<SlugifyRequest>(json);
                if (request == null)
                    return new { Error = "Invalid request format." };

                string result = SlugifyText(
                    request.Text, 
                    request.LowerCase, 
                    request.Separator, 
                    request.RemoveDiacritics
                );

                return new { Result = result };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        private string SlugifyText(string text, bool toLowerCase, string separator, bool removeDiacritics)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Remove diacritics (accents)
            if (removeDiacritics)
            {
                text = RemoveDiacritics(text);
            }

            // Convert to lowercase if specified
            if (toLowerCase)
            {
                text = text.ToLowerInvariant();
            }

            // Replace spaces and punctuation with the separator
            text = Regex.Replace(text, @"[^\w\s-]", ""); // Remove all non-word chars except spaces and hyphens
            text = Regex.Replace(text, @"[\s_-]+", " ").Trim(); // Replace all spaces, underscores, hyphens with a single space
            text = Regex.Replace(text, @"\s", separator); // Replace spaces with the separator

            // Remove consecutive separators
            text = Regex.Replace(text, $"{Regex.Escape(separator)}+", separator);

            return text;
        }

        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 800px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>Slugify String</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Convert text to URL-friendly slugs (e.g. ""Hello World"" → ""hello-world"").</p>
                    </div>
                    
                    <div class='card shadow-lg p-4'>
                        <div class='mb-3'>
                            <label for='inputText' class='form-label fw-bold'>Enter Text:</label>
                            <textarea id='inputText' class='form-control' rows='3' 
                                placeholder='Type or paste text to convert to a slug...'></textarea>
                        </div>
                        
                        <div class='row mb-4'>
                            <div class='col-md-6'>
                                <div class='form-check mb-2'>
                                    <input class='form-check-input' type='checkbox' id='lowerCaseOption' checked>
                                    <label class='form-check-label' for='lowerCaseOption'>
                                        Convert to lowercase
                                    </label>
                                </div>
                                <div class='form-check'>
                                    <input class='form-check-input' type='checkbox' id='removeDiacriticsOption' checked>
                                    <label class='form-check-label' for='removeDiacriticsOption'>
                                        Remove diacritics (accents)
                                    </label>
                                </div>
                            </div>
                            <div class='col-md-6'>
                                <label for='separatorOption' class='form-label'>Separator:</label>
                                <select id='separatorOption' class='form-select'>
                                    <option value='-' selected>Hyphen (-)</option>
                                    <option value='_'>Underscore (_)</option>
                                    <option value=''>None</option>
                                    <option value='.'>Period (.)</option>
                                </select>
                            </div>
                        </div>
                        
                        <div class='mb-3'>
                            <label class='form-label fw-bold'>Result:</label>
                            <div class='input-group'>
                                <input type='text' id='resultOutput' class='form-control bg-light' readonly>
                                <button id='copyBtn' class='btn btn-outline-secondary' title='Copy to clipboard'>
                                    <i class='bi bi-clipboard'></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
                
                <script>
                    document.addEventListener('DOMContentLoaded', function() {
                        const inputText = document.getElementById('inputText');
                        const lowerCaseOption = document.getElementById('lowerCaseOption');
                        const separatorOption = document.getElementById('separatorOption');
                        const removeDiacriticsOption = document.getElementById('removeDiacriticsOption');
                        const resultOutput = document.getElementById('resultOutput');
                        const copyBtn = document.getElementById('copyBtn');
                        
                        // Debounce function to limit API calls
                        function debounce(func, wait) {
                            let timeout;
                            return function(...args) {
                                clearTimeout(timeout);
                                timeout = setTimeout(() => func.apply(this, args), wait);
                            };
                        }
                        
                        // Generate slug when inputs change
                        const generateSlug = debounce(function() {
                            const text = inputText.value.trim();
                            if (!text) {
                                resultOutput.value = '';
                                return;
                            }
                            
                            fetch(window.location.pathname + '/execute', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({
                                    Text: text,
                                    LowerCase: lowerCaseOption.checked,
                                    Separator: separatorOption.value,
                                    RemoveDiacritics: removeDiacriticsOption.checked
                                })
                            })
                            .then(response => response.json())
                            .then(data => {
                                if (data.Error) {
                                    resultOutput.value = `Error: ${data.Error}`;
                                } else {
                                    resultOutput.value = data.result.result;
                                }
                            })
                            .catch(error => {
                                console.error('Error:', error);
                                resultOutput.value = 'Error generating slug';
                            });
                        }, 200);
                        
                        // Event listeners for real-time slug generation
                        inputText.addEventListener('input', generateSlug);
                        lowerCaseOption.addEventListener('change', generateSlug);
                        separatorOption.addEventListener('change', generateSlug);
                        removeDiacriticsOption.addEventListener('change', generateSlug);
                        
                        // Copy functionality
                        copyBtn.addEventListener('click', function() {
                            resultOutput.select();
                            document.execCommand('copy');
                            
                            const originalHTML = copyBtn.innerHTML;
                            copyBtn.innerHTML = '<i class=""bi bi-check-lg""></i>';
                            
                            setTimeout(() => {
                                copyBtn.innerHTML = originalHTML;
                            }, 1500);
                        });
                    });
                </script>
            ";
        }
        
        public void Stop()
        {
            // No resources to clean up
        }
    }
}
