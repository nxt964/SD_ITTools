using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ToolInterface;

namespace TextToNATOAlphabet
{
    public class NATOAlphabetRequest
    {
        public string Text { get; set; } = "";
        public bool IncludeOriginalCharacters { get; set; } = true;
    }

    public class TextToNATOAlphabetTool : ITool
    {
        public string Name => "NATO Phonetic Alphabet";
        public string Description => "Convert text to NATO phonetic alphabet";
        public string Category => "Converter";

        // Sửa lại định nghĩa Dictionary không dùng StringComparer vì chúng ta dùng char làm key
        private static readonly Dictionary<char, string> NatoAlphabet = new Dictionary<char, string>
        {
            {'A', "Alpha"},
            {'a', "Alpha"},
            {'B', "Bravo"},
            {'b', "Bravo"},
            {'C', "Charlie"},
            {'c', "Charlie"},
            {'D', "Delta"},
            {'d', "Delta"},
            {'E', "Echo"},
            {'e', "Echo"},
            {'F', "Foxtrot"},
            {'f', "Foxtrot"},
            {'G', "Golf"},
            {'g', "Golf"},
            {'H', "Hotel"},
            {'h', "Hotel"},
            {'I', "India"},
            {'i', "India"},
            {'J', "Juliet"},
            {'j', "Juliet"},
            {'K', "Kilo"},
            {'k', "Kilo"},
            {'L', "Lima"},
            {'l', "Lima"},
            {'M', "Mike"},
            {'m', "Mike"},
            {'N', "November"},
            {'n', "November"},
            {'O', "Oscar"},
            {'o', "Oscar"},
            {'P', "Papa"},
            {'p', "Papa"},
            {'Q', "Quebec"},
            {'q', "Quebec"},
            {'R', "Romeo"},
            {'r', "Romeo"},
            {'S', "Sierra"},
            {'s', "Sierra"},
            {'T', "Tango"},
            {'t', "Tango"},
            {'U', "Uniform"},
            {'u', "Uniform"},
            {'V', "Victor"},
            {'v', "Victor"},
            {'W', "Whiskey"},
            {'w', "Whiskey"},
            {'X', "X-ray"},
            {'x', "X-ray"},
            {'Y', "Yankee"},
            {'y', "Yankee"},
            {'Z', "Zulu"},
            {'z', "Zulu"},
            {'0', "Zero"},
            {'1', "One"},
            {'2', "Two"},
            {'3', "Three"},
            {'4', "Four"},
            {'5', "Five"},
            {'6', "Six"},
            {'7', "Seven"},
            {'8', "Eight"},
            {'9', "Nine"},
            {'.', "Dot"},
            {',', "Comma"},
            {'?', "Question Mark"},
            {'!', "Exclamation Mark"},
            {'-', "Dash"},
            {'_', "Underscore"},
            {'@', "At Sign"},
            {'#', "Hash"},
            {'$', "Dollar Sign"},
            {'%', "Percent"},
            {'&', "Ampersand"},
            {'*', "Asterisk"},
            {'(', "Open Parenthesis"},
            {')', "Close Parenthesis"},
            {'+', "Plus"},
            {'=', "Equals"},
            {'/', "Slash"},
            {'\\', "Backslash"},
            {'|', "Vertical Bar"},
            {';', "Semicolon"},
            {':', "Colon"},
            {'\'', "Single Quote"},
            {'"', "Double Quote"},
            {'<', "Less Than"},
            {'>', "Greater Than"},
            {' ', "Space"}
        };

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json))
                    return new { Error = "Invalid request. Input is empty." };

                var request = JsonSerializer.Deserialize<NATOAlphabetRequest>(json);
                if (request == null || string.IsNullOrEmpty(request.Text))
                    return new { Error = "Invalid request. Text is required." };

                string natoText = ConvertToNATO(request.Text, request.IncludeOriginalCharacters);
                return new { Result = natoText };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        private string ConvertToNATO(string text, bool includeOriginalChar)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var result = new StringBuilder();
            bool isFirstWord = true;

            foreach (char c in text)
            {
                if (isFirstWord)
                    isFirstWord = false;
                else
                    result.Append(" ");

                if (NatoAlphabet.TryGetValue(c, out string natoWord))
                {
                    if (includeOriginalChar)
                        result.Append($"{natoWord}");
                    else
                        result.Append(natoWord);
                }
                else
                {
                    // For characters not in the NATO alphabet
                    if (includeOriginalChar)
                        result.Append($"{c}");
                    else
                        result.Append("Unknown");
                }
            }

            return result.ToString();
        }

        // Sửa lỗi trong JavaScript - cần sửa data.result thành data.Result
        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 800px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>NATO Phonetic Alphabet Converter</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Convert text to NATO phonetic alphabet representation.</p>
                    </div>
                    <div class='card shadow-lg p-4'>
                        <div class='mb-3'>
                            <label for='inputText' class='form-label fw-bold'>Enter Text:</label>
                            <textarea id='inputText' class='form-control' rows='3' placeholder='Type or paste text here...'></textarea>
                        </div>
                        
                        <div class='mb-3'>
                            <div class='form-check'>
                                <input class='form-check-input' type='checkbox' id='includeOriginalChars' checked>
                                <label class='form-check-label' for='includeOriginalChars'>
                                    Include original characters (e.g. 'A - Alpha' instead of just 'Alpha')
                                </label>
                            </div>
                        </div>
                        
                        <button id='convertBtn' class='btn btn-primary w-100 my-3'>Convert to NATO Alphabet</button>
                        
                        <div id='resultContainer' style='display: none;'>
                            <label class='form-label fw-bold'>Result:</label>
                            <div class='d-flex mb-3'>
                                <div id='resultOutput' class='form-control bg-light' style='min-height: 150px; overflow-y: auto; white-space: pre-wrap;'></div>
                                <button id='copyBtn' class='btn btn-outline-secondary ms-2' title='Copy to clipboard'>
                                    <i class='bi bi-clipboard'></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
                
                <script>
                    document.addEventListener('DOMContentLoaded', function() {
                        const convertBtn = document.getElementById('convertBtn');
                        const inputText = document.getElementById('inputText');
                        const includeOriginalChars = document.getElementById('includeOriginalChars');
                        const resultContainer = document.getElementById('resultContainer');
                        const resultOutput = document.getElementById('resultOutput');
                        const copyBtn = document.getElementById('copyBtn');
                        
                        inputText.addEventListener('input', function() {
                            const text = inputText.value.trim();
                            if (!text) {
                                alert('Please enter some text to convert.');
                                return;
                            }
                            
                            convertBtn.disabled = true;
                            convertBtn.innerHTML = '<span class=""spinner-border spinner-border-sm"" role=""status"" aria-hidden=""true""></span> Converting...';
                            
                            fetch(window.location.pathname + '/execute', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({
                                    Text: text,
                                    IncludeOriginalCharacters: includeOriginalChars.checked
                                })
                            })
                            .then(response => response.json())
                            .then(data => {
                                convertBtn.disabled = false;
                                convertBtn.textContent = 'Convert to NATO Alphabet';
                                
                                if (data.Error) {
                                    alert('Error: ' + data.Error);
                                    return;
                                }
                                console.log('Conversion result:', data);
                                resultOutput.textContent = data.result.result; // Sửa từ data.result thành data.Result
                                resultContainer.style.display = 'block';
                            })
                            .catch(error => {
                                convertBtn.disabled = false;
                                convertBtn.textContent = 'Convert to NATO Alphabet';
                                console.error('Error:', error);
                                alert('An error occurred. Please try again.');
                            });
                        });
                        
                        // Copy to clipboard functionality
                        copyBtn.addEventListener('click', function() {
                            const text = resultOutput.textContent;
                            navigator.clipboard.writeText(text)
                                .then(() => {
                                    const originalHTML = copyBtn.innerHTML;
                                    copyBtn.innerHTML = '<i class=""bi bi-check-lg""></i>';
                                    setTimeout(() => {
                                        copyBtn.innerHTML = originalHTML;
                                    }, 1500);
                                })
                                .catch(err => {
                                    console.error('Could not copy text: ', err);
                                    alert('Failed to copy to clipboard');
                                });
                        });
                        
                        // Allow pressing Enter in the input field to trigger conversion
                        inputText.addEventListener('keydown', function(event) {
                            if (event.key === 'Enter' && !event.shiftKey) {
                                event.preventDefault();
                                convertBtn.click();
                            }
                        });
                    });
                </script>
            ";
        }
        
        public void Stop()
        {
            // Không cần thực hiện thao tác dọn dẹp đặc biệt
            return;
        }
    }
}
