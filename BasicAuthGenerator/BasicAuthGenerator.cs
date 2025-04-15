using System;
using System.Text;
using System.Text.Json;
using ToolInterface;

namespace BasicAuthGenerator
{
    public class BasicAuthRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class BasicAuthGeneratorTool : ITool
    {
        public string Name => "Basic Auth Generator";
        public string Description => "Generate HTTP Basic Authentication header value from username and password";
        public string Category => "Web";

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json))
                    return new { Error = "Invalid request. Input is empty." };

                var request = JsonSerializer.Deserialize<BasicAuthRequest>(json);
                if (request == null || string.IsNullOrEmpty(request.Username))
                    return new { Error = "Username is required." };

                string basicAuthValue = GenerateBasicAuth(request.Username, request.Password ?? "");
                return new { Result = basicAuthValue };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        private string GenerateBasicAuth(string username, string password)
        {
            string credentials = $"{username}:{password}";
            byte[] credentialsBytes = Encoding.UTF8.GetBytes(credentials);
            string encodedCredentials = Convert.ToBase64String(credentialsBytes);
            return $"Basic {encodedCredentials}";
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 600px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>Basic Auth Generator</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Generate HTTP Basic Authentication header value for API requests.</p>
                    </div>
                    <div class='card shadow-lg p-4'>
                        <div class='mb-3'>
                            <label for='username' class='form-label fw-bold'>Username:</label>
                            <input type='text' id='username' class='form-control' placeholder='Enter username'>
                        </div>
                        
                        <div class='mb-4'>
                            <label for='password' class='form-label fw-bold'>Password:</label>
                            <div class='input-group'>
                                <input type='password' id='password' class='form-control' placeholder='Enter password'>
                                <button class='btn btn-outline-secondary' type='button' id='togglePassword' title='Toggle password visibility'>
                                    <i class='bi bi-eye'></i>
                                </button>
                            </div>
                        </div>
                        
                        <button id='generateBtn' class='btn btn-primary w-100 mb-3'>Generate</button>
                        
                        <div id='resultContainer' style='display: none;'>
                            <div class='mb-2'>
                                <label class='form-label fw-bold'>Result:</label>
                                <div class='d-flex'>
                                    <input type='text' id='authResult' class='form-control' readonly>
                                    <button id='copyBtn' class='btn btn-outline-secondary ms-2' title='Copy to clipboard'>
                                        <i class='bi bi-clipboard'></i>
                                    </button>
                                </div>
                            </div>
                            
                            <div class='mt-3'>
                                <div class='accordion' id='usageAccordion'>
                                    <div class='accordion-item'>
                                        <h2 class='accordion-header' id='usageHeader'>
                                            <button class='accordion-button collapsed' type='button' data-bs-toggle='collapse' data-bs-target='#usageCollapse'>
                                                How to use this header
                                            </button>
                                        </h2>
                                        <div id='usageCollapse' class='accordion-collapse collapse' data-bs-parent='#usageAccordion'>
                                            <div class='accordion-body'>
                                                <p>Add this header to your HTTP requests:</p>
                                                <pre class='bg-light p-2 rounded'><code>Authorization: <span id='usageExample'></span></code></pre>
                                                <p class='mb-1'>Examples:</p>
                                                <p class='mb-1'><strong>cURL:</strong> <code>curl -H ""Authorization: <span class='auth-example'></span>"" https://example.com/api</code></p>
                                                <p class='mb-1'><strong>Fetch API:</strong> <code>fetch(url, { headers: { 'Authorization': '<span class='auth-example'></span>' } })</code></p>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <script>
                    document.addEventListener('DOMContentLoaded', function() {
                        const generateBtn = document.getElementById('generateBtn');
                        const usernameInput = document.getElementById('username');
                        const passwordInput = document.getElementById('password');
                        const togglePassword = document.getElementById('togglePassword');
                        const resultContainer = document.getElementById('resultContainer');
                        const authResult = document.getElementById('authResult');
                        const copyBtn = document.getElementById('copyBtn');
                        const usageExample = document.getElementById('usageExample');
                        const authExamples = document.querySelectorAll('.auth-example');
                        
                        // Toggle password visibility
                        togglePassword.addEventListener('click', function() {
                            const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
                            passwordInput.setAttribute('type', type);
                            
                            // Change the eye icon
                            const eyeIcon = togglePassword.querySelector('i');
                            if (type === 'text') {
                                eyeIcon.classList.remove('bi-eye');
                                eyeIcon.classList.add('bi-eye-slash');
                            } else {
                                eyeIcon.classList.remove('bi-eye-slash');
                                eyeIcon.classList.add('bi-eye');
                            }
                        });
                        
                        // Generate basic auth
                        generateBtn.addEventListener('click', function() {
                            const username = usernameInput.value.trim();
                            if (!username) {
                                alert('Username is required!');
                                usernameInput.focus();
                                return;
                            }
                            
                            const password = passwordInput.value;
                            
                            generateBtn.disabled = true;
                            generateBtn.innerHTML = '<span class=""spinner-border spinner-border-sm"" role=""status"" aria-hidden=""true""></span> Generating...';
                            
                            fetch(window.location.pathname + '/execute', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({
                                    Username: username,
                                    Password: password
                                })
                            })
                            .then(response => response.json())
                            .then(data => {
                                generateBtn.disabled = false;
                                generateBtn.textContent = 'Generate';
                                
                                if (data.Error) {
                                    alert('Error: ' + data.Error);
                                    return;
                                }
                                console.log(data);
                                authResult.value =  data.result.result;
                                usageExample.textContent =  data.result.result;
                                authExamples.forEach(el => el.textContent =  data.result.result);
                                resultContainer.style.display = 'block';
                            })
                            .catch(error => {
                                generateBtn.disabled = false;
                                generateBtn.textContent = 'Generate';
                                console.error('Error:', error);
                                alert('An error occurred. Please try again.');
                            });
                        });
                        
                        // Copy to clipboard
                        copyBtn.addEventListener('click', function() {
                            authResult.select();
                            document.execCommand('copy');
                            
                            const originalHTML = copyBtn.innerHTML;
                            copyBtn.innerHTML = '<i class=""bi bi-check-lg""></i>';
                            
                            setTimeout(() => {
                                copyBtn.innerHTML = originalHTML;
                            }, 1500);
                        });
                        
                        // Enter key to submit
                        [usernameInput, passwordInput].forEach(input => {
                            input.addEventListener('keydown', function(event) {
                                if (event.key === 'Enter') {
                                    generateBtn.click();
                                }
                            });
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
