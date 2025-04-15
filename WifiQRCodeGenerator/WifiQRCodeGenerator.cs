using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using QRCoder;
using ToolInterface;

namespace WifiQRCodeGenerator
{
    public class WifiQRCodeRequest
    {
        public string SSID { get; set; } = "";
        public string Password { get; set; } = "";
        public string EncryptionMethod { get; set; } = "WPA"; // "WPA", "WEP", "WPA2-EAP", "nopass"
        public string ForegroundColor { get; set; } = "#000000"; // Black default
        public string BackgroundColor { get; set; } = "#FFFFFF"; // White default
        public bool IsHidden { get; set; } = false; // Optional: For hidden networks
    }

    public class WifiQRCodeGeneratorTool : ITool
    {
        public string Name => "WiFi QR Code Generator";
        public string Description => "Generate QR codes for WiFi network connection";
        public string Category => "Images and videos";

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json))
                    return new { Error = "Invalid request. Input is empty." };

                var request = JsonSerializer.Deserialize<WifiQRCodeRequest>(json);
                if (request == null || string.IsNullOrWhiteSpace(request.SSID))
                    return new { Error = "Network SSID is required." };

                // Validate encryption method
                var encryptionMethod = MapEncryptionMethod(request.EncryptionMethod);
                if (encryptionMethod == "WPA" || encryptionMethod == "WEP")
                {
                    if (string.IsNullOrEmpty(request.Password))
                        return new { Error = $"Password is required for {encryptionMethod} encryption." };
                }

                // Parse colors
                if (!TryParseHexColor(request.ForegroundColor, out Color foreColor))
                    return new { Error = "Invalid foreground color format. Use hex format: #RRGGBB" };

                if (!TryParseHexColor(request.BackgroundColor, out Color backColor))
                    return new { Error = "Invalid background color format. Use hex format: #RRGGBB" };

                // Generate WiFi Authentication string in standard format
                // WIFI:T:<Authentication Type>;S:<SSID>;P:<Password>;H:<Hidden SSID>;
                string wifiAuthString = $"WIFI:T:{encryptionMethod};S:{EscapeInput(request.SSID)};";
                
                if (!string.IsNullOrEmpty(request.Password) && encryptionMethod != "nopass")
                    wifiAuthString += $"P:{EscapeInput(request.Password)};";
                
                if (request.IsHidden)
                    wifiAuthString += "H:true;";
                
                wifiAuthString += ";";

                // Generate QR Code
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrData = qrGenerator.CreateQrCode(wifiAuthString, QRCodeGenerator.ECCLevel.Q);
                    var qrCode = new PngByteQRCode(qrData);
                    var qrCodeBytes = qrCode.GetGraphic(20, foreColor, backColor, true);
                    
                    var base64String = Convert.ToBase64String(qrCodeBytes);
                    return new { 
                        Result = base64String, 
                        ContentType = "image/png",
                        WifiString = wifiAuthString
                    };
                }
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        private string MapEncryptionMethod(string method)
        {
            return method?.ToLower() switch
            {
                "wpa" or "wpa2" or "wpa/wpa2" => "WPA",
                "wep" => "WEP",
                "wpa2-eap" or "wpa2/eap" or "eap" => "WPA2-EAP",
                "nopass" or "none" or "no password" => "nopass",
                _ => "WPA" // Default to WPA
            };
        }

        private string EscapeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Escape special characters according to WiFi QR code specification
            return input
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace(";", "\\;")
                .Replace(",", "\\,")
                .Replace(":", "\\:");
        }

        private bool TryParseHexColor(string hexColor, out Color color)
        {
            color = Color.Black; // Default

            if (string.IsNullOrWhiteSpace(hexColor))
                return false;

            // Remove # if present
            if (hexColor.StartsWith("#"))
                hexColor = hexColor.Substring(1);

            try 
            {
                // Handle 3 and 6 digit hex
                if (hexColor.Length == 3)
                {
                    var r = Convert.ToByte(new string(hexColor[0], 2), 16);
                    var g = Convert.ToByte(new string(hexColor[1], 2), 16);
                    var b = Convert.ToByte(new string(hexColor[2], 2), 16);
                    color = Color.FromArgb(r, g, b);
                    return true;
                }
                else if (hexColor.Length == 6)
                {
                    var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                    var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                    var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                    color = Color.FromArgb(r, g, b);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 800px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>WiFi QR Code Generator</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Create QR codes for easy WiFi network connection on mobile devices.</p>
                    </div>
                    
                    <div class='row g-4'>
                        <div class='col-md-6'>
                            <div class='card shadow h-100'>
                                <div class='card-header bg-primary text-white'>
                                    <h5 class='card-title mb-0'>WiFi Settings</h5>
                                </div>
                                <div class='card-body'>
                                    <div class='mb-3'>
                                        <label for='ssid' class='form-label fw-bold'>Network Name (SSID):</label>
                                        <input type='text' id='ssid' class='form-control' 
                                            placeholder='Enter WiFi network name'>
                                    </div>
                                    
                                    <div class='mb-3'>
                                        <label for='encryptionMethod' class='form-label fw-bold'>Security Type:</label>
                                        <select id='encryptionMethod' class='form-select'>
                                            <option value='WPA'>WPA/WPA2</option>
                                            <option value='WEP'>WEP</option>
                                            <option value='WPA2-EAP'>WPA2/EAP (Enterprise)</option>
                                            <option value='nopass'>No Password</option>
                                        </select>
                                    </div>
                                    
                                    <div class='mb-3' id='passwordContainer'>
                                        <label for='password' class='form-label fw-bold'>Password:</label>
                                        <div class='input-group'>
                                            <input type='password' id='password' class='form-control' 
                                                placeholder='Enter WiFi password'>
                                            <button class='btn btn-outline-secondary' type='button' id='togglePassword' title='Show/Hide Password'>
                                                <i class='bi bi-eye'></i>
                                            </button>
                                        </div>
                                    </div>
                                    
                                    <div class='mb-3 form-check'>
                                        <input type='checkbox' class='form-check-input' id='isHidden'>
                                        <label class='form-check-label' for='isHidden'>Hidden Network</label>
                                    </div>
                                    
                                    <div class='row mb-3'>
                                        <div class='col-md-6'>
                                            <label for='foreColor' class='form-label fw-bold'>QR Code Color:</label>
                                            <div class='input-group'>
                                                <input type='color' id='foreColorPicker' class='form-control form-control-color' value='#000000'>
                                                <input type='text' id='foreColor' class='form-control' value='#000000'>
                                            </div>
                                        </div>
                                        <div class='col-md-6'>
                                            <label for='backColor' class='form-label fw-bold'>Background Color:</label>
                                            <div class='input-group'>
                                                <input type='color' id='backColorPicker' class='form-control form-control-color' value='#FFFFFF'>
                                                <input type='text' id='backColor' class='form-control' value='#FFFFFF'>
                                            </div>
                                        </div>
                                    </div>
                                    
                                    <div class='text-center'>
                                        <button id='generateBtn' class='btn btn-primary'>Generate WiFi QR Code</button>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <div class='col-md-6'>
                            <div class='card shadow h-100'>
                                <div class='card-header bg-success text-white'>
                                    <h5 class='card-title mb-0'>Generated QR Code</h5>
                                </div>
                                <div class='card-body text-center'>
                                    <div id='loadingIndicator' style='display: none;'>
                                        <div class='spinner-border' role='status'>
                                            <span class='visually-hidden'>Loading...</span>
                                        </div>
                                        <p class='mt-2'>Generating QR Code...</p>
                                    </div>
                                    
                                    <div id='qrResult' style='display: none;'>
                                        <img id='qrImage' class='img-fluid' style='max-width: 250px;'>
                                        
                                        <div class='mt-3'>
                                            <button id='downloadBtn' class='btn btn-outline-primary me-2'>
                                                <i class='bi bi-download'></i> Download
                                            </button>
                                        </div>
                                    </div>
                                    
                                    <div id='initialState' class='py-5 text-muted'>
                                        <i class='bi bi-wifi' style='font-size: 3rem;'></i>
                                        <p class='mt-2'>Enter WiFi details to generate a QR code</p>
                                        <p class='small text-muted'>Scan the QR code with your smartphone camera to connect automatically</p>
                                    </div>
                                    
                                    <div id='errorDisplay' class='alert alert-danger mt-3' style='display: none;'></div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <script>
                    document.addEventListener('DOMContentLoaded', function() {
                        // DOM elements
                        const ssid = document.getElementById('ssid');
                        const encryptionMethod = document.getElementById('encryptionMethod');
                        const passwordContainer = document.getElementById('passwordContainer');
                        const password = document.getElementById('password');
                        const togglePassword = document.getElementById('togglePassword');
                        const isHidden = document.getElementById('isHidden');
                        const foreColorPicker = document.getElementById('foreColorPicker');
                        const foreColor = document.getElementById('foreColor');
                        const backColorPicker = document.getElementById('backColorPicker');
                        const backColor = document.getElementById('backColor');
                        const generateBtn = document.getElementById('generateBtn');
                        const qrResult = document.getElementById('qrResult');
                        const qrImage = document.getElementById('qrImage');
                        const initialState = document.getElementById('initialState');
                        const loadingIndicator = document.getElementById('loadingIndicator');
                        const errorDisplay = document.getElementById('errorDisplay');
                        const downloadBtn = document.getElementById('downloadBtn');
                        
                        // Hide/show password based on encryption type
                        encryptionMethod.addEventListener('change', function() {
                            if (this.value === 'nopass') {
                                passwordContainer.style.display = 'none';
                            } else {
                                passwordContainer.style.display = 'block';
                            }
                        });
                        
                        // Toggle password visibility
                        togglePassword.addEventListener('click', function() {
                            const type = password.getAttribute('type') === 'password' ? 'text' : 'password';
                            password.setAttribute('type', type);
                            
                            const eyeIcon = togglePassword.querySelector('i');
                            eyeIcon.classList.toggle('bi-eye');
                            eyeIcon.classList.toggle('bi-eye-slash');
                        });
                        
                        // Color pickers sync
                        foreColorPicker.addEventListener('input', function() {
                            foreColor.value = this.value;
                        });
                        
                        foreColor.addEventListener('change', function() {
                            foreColorPicker.value = this.value;
                        });
                        
                        backColorPicker.addEventListener('input', function() {
                            backColor.value = this.value;
                        });
                        
                        backColor.addEventListener('change', function() {
                            backColorPicker.value = this.value;
                        });
                        
                        // Generate QR code
                        generateBtn.addEventListener('click', function() {
                            const ssidValue = ssid.value.trim();
                            if (!ssidValue) {
                                showError('Network name (SSID) is required');
                                ssid.focus();
                                return;
                            }
                            
                            const encMethod = encryptionMethod.value;
                            const passwordValue = password.value;
                            
                            if ((encMethod === 'WPA' || encMethod === 'WEP') && !passwordValue) {
                                showError('Password is required for ' + encMethod + ' encryption');
                                password.focus();
                                return;
                            }
                            
                            // Show loading
                            initialState.style.display = 'none';
                            qrResult.style.display = 'none';
                            errorDisplay.style.display = 'none';
                            loadingIndicator.style.display = 'block';
                            
                            // Make API call
                            fetch(window.location.pathname + '/execute', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({
                                    SSID: ssidValue,
                                    Password: passwordValue,
                                    EncryptionMethod: encMethod,
                                    IsHidden: isHidden.checked,
                                    ForegroundColor: foreColor.value,
                                    BackgroundColor: backColor.value
                                })
                            })
                            .then(response => response.json())
                            .then(data => {
                                console.log(data);
                                loadingIndicator.style.display = 'none';
                                
                                if (data.Error) {
                                    showError(data.Error);
                                    return;
                                }
                                
                                // Show QR code
                                qrImage.src = 'data:image/png;base64,' + data.result.result;
                                qrResult.style.display = 'block';
                                console.log('WiFi String:', data.WifiString);
                                
                                // Set download button
                                downloadBtn.onclick = function() {
                                    const link = document.createElement('a');
                                    link.href = 'data:image/png;base64,' + data.result.result;
                                    link.download = 'wifi_' + ssidValue.replace(/[^a-z0-9]/gi, '_').toLowerCase() + '.png';
                                    document.body.appendChild(link);
                                    link.click();
                                    document.body.removeChild(link);
                                };
                            })
                            .catch(error => {
                                loadingIndicator.style.display = 'none';
                                showError('An error occurred while generating the QR code: ' + error.message);
                            });
                        });
                        
                        // Show error message
                        function showError(message) {
                            errorDisplay.textContent = message;
                            errorDisplay.style.display = 'block';
                            qrResult.style.display = 'none';
                            initialState.style.display = 'none';
                        }
                        
                        // Allow pressing Enter to generate
                        [ssid, password].forEach(input => {
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
