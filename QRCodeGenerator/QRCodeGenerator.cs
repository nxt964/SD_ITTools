using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.Json;
using QRCoder;  // Import the QRCoder namespace
using ToolInterface;

// Change the namespace to avoid conflict
namespace QRCodeGeneratorTool
{
    public class QRCodeRequest
    {
        public string Text { get; set; } = "";
        public string ForegroundColor { get; set; } = "#000000"; // Black default
        public string BackgroundColor { get; set; } = "#FFFFFF"; // White default
        public string ErrorCorrectionLevel { get; set; } = "Medium"; // Low, Medium, Quartile, High
    }

    public class QRCodeGeneratorTool : ITool
    {
        public string Name => "QR Code Generator";
        public string Description => "Generate QR codes with custom colors and error correction";
        public string Category => "Images and videos";

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json))
                    return new { Error = "Invalid request. Input is empty." };

                var request = JsonSerializer.Deserialize<QRCodeRequest>(json);
                if (request == null || string.IsNullOrWhiteSpace(request.Text))
                    return new { Error = "QR code text content is required." };

                // Parse colors
                if (!TryParseHexColor(request.ForegroundColor, out Color foreColor))
                    return new { Error = "Invalid foreground color format. Use hex format: #RRGGBB" };

                if (!TryParseHexColor(request.BackgroundColor, out Color backColor))
                    return new { Error = "Invalid background color format. Use hex format: #RRGGBB" };

                // Parse error correction level
                QRCodeGenerator.ECCLevel eccLevel;  // This is correct - it's using the class name
                switch (request.ErrorCorrectionLevel?.ToLower())
                {
                    case "low":
                        eccLevel = QRCodeGenerator.ECCLevel.L;
                        break;
                    case "medium":
                        eccLevel = QRCodeGenerator.ECCLevel.M;
                        break;
                    case "quartile":
                        eccLevel = QRCodeGenerator.ECCLevel.Q;
                        break;
                    case "high":
                        eccLevel = QRCodeGenerator.ECCLevel.H;
                        break;
                    default:
                        eccLevel = QRCodeGenerator.ECCLevel.M;
                        break;
                }

                // Generate QR Code - QRCodeGenerator is from the QRCoder namespace
                using (var qrGenerator = new QRCodeGenerator())
                {
                    var qrData = qrGenerator.CreateQrCode(request.Text, eccLevel);
                    var qrCode = new PngByteQRCode(qrData);
                    
                    // Sử dụng PngByteQRCode với Color trực tiếp
                    var qrCodeBytes = qrCode.GetGraphic(20, foreColor, backColor, true);
                    
                    var base64String = Convert.ToBase64String(qrCodeBytes);
                    
                    return new { Result = base64String, ContentType = "image/png" };
                }

            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        private bool TryParseHexColor(string hexColor, out Color color)
        {
            color = Color.Black; // Default

            if (string.IsNullOrWhiteSpace(hexColor))
                return false;

            // Remove # if present
            if (hexColor.StartsWith("#"))
                hexColor = hexColor.Substring(1);

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

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 800px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>QR Code Generator</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Generate customized QR codes for URLs, text, or contact information.</p>
                    </div>
                    
                    <div class='row g-4'>
                        <div class='col-md-6'>
                            <div class='card shadow h-100'>
                                <div class='card-header bg-primary text-white'>
                                    <h5 class='card-title mb-0'>QR Code Settings</h5>
                                </div>
                                <div class='card-body'>
                                    <div class='mb-3'>
                                        <label for='qrContent' class='form-label fw-bold'>QR Code Content:</label>
                                        <textarea id='qrContent' class='form-control' rows='3' 
                                            placeholder='Enter URL or text content for QR code'></textarea>
                                        <div class='form-text'>URLs should start with http:// or https://</div>
                                    </div>
                                    
                                    <div class='row mb-3'>
                                        <div class='col-md-6'>
                                            <label for='foreColor' class='form-label fw-bold'>Foreground Color:</label>
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
                                    
                                    <div class='mb-3'>
                                        <label for='errorLevel' class='form-label fw-bold'>Error Correction Level:</label>
                                        <select id='errorLevel' class='form-select'>
                                            <option value='Low'>Low</option>
                                            <option value='Medium' selected>Medium</option>
                                            <option value='Quartile'>Quartile</option>
                                            <option value='High'>High</option>
                                        </select>
                                        <div class='form-text'>Higher levels add more redundancy but result in denser QR codes</div>
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
                                        <img id='qrImage' class='img-fluid' style='max-width: 300px;'>
                                        
                                        <div class='mt-3'>
                                            <button id='downloadBtn' class='btn btn-outline-primary me-2'>
                                                <i class='bi bi-download'></i> Download
                                            </button>
                                            <button id='copyBtn' class='btn btn-outline-secondary'>
                                                <i class='bi bi-clipboard'></i> Copy
                                            </button>
                                        </div>
                                    </div>
                                    
                                    <div id='initialState' class='py-5 text-muted'>
                                        <i class='bi bi-qr-code' style='font-size: 3rem;'></i>
                                        <p class='mt-2'>Enter content and click Generate to create a QR code</p>
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
                        const qrContent = document.getElementById('qrContent');
                        const foreColorPicker = document.getElementById('foreColorPicker');
                        const foreColor = document.getElementById('foreColor');
                        const backColorPicker = document.getElementById('backColorPicker');
                        const backColor = document.getElementById('backColor');
                        const errorLevel = document.getElementById('errorLevel');
                        const qrResult = document.getElementById('qrResult');
                        const qrImage = document.getElementById('qrImage');
                        const initialState = document.getElementById('initialState');
                        const loadingIndicator = document.getElementById('loadingIndicator');
                        const errorDisplay = document.getElementById('errorDisplay');
                        const downloadBtn = document.getElementById('downloadBtn');
                        const copyBtn = document.getElementById('copyBtn');
                        
                        // Debounce function to avoid too many API calls
                        const debounce = (func, delay) => {
                            let debounceTimer;
                            return function() {
                                const context = this;
                                const args = arguments;
                                clearTimeout(debounceTimer);
                                debounceTimer = setTimeout(() => func.apply(context, args), delay);
                            };
                        };
                        
                        // Sync color inputs
                        foreColorPicker.addEventListener('input', function() {
                            foreColor.value = this.value;
                            generateQRCode();
                        });
                        
                        foreColor.addEventListener('change', function() {
                            foreColorPicker.value = this.value;
                            generateQRCode();
                        });
                        
                        backColorPicker.addEventListener('input', function() {
                            backColor.value = this.value;
                            generateQRCode();
                        });
                        
                        backColor.addEventListener('change', function() {
                            backColorPicker.value = this.value;
                            generateQRCode();
                        });
                        
                        // Generate QR code for any change
                        qrContent.addEventListener('input', debounce(function() {
                            generateQRCode();
                        }, 500));
                        
                        errorLevel.addEventListener('change', function() {
                            generateQRCode();
                        });
                        
                        // Function to generate QR code
                        function generateQRCode() {
                            const content = qrContent.value.trim();
                            if (!content) {
                                showError('Please enter content for the QR code');
                                return;
                            }
                            
                            // Show loading state
                            initialState.style.display = 'none';
                            qrResult.style.display = 'none';
                            errorDisplay.style.display = 'none';
                            loadingIndicator.style.display = 'block';
                            
                            // Make API call
                            fetch(window.location.pathname + '/execute', {
                                method: 'POST',
                                headers: { 'Content-Type': 'application/json' },
                                body: JSON.stringify({
                                    Text: content,
                                    ForegroundColor: foreColor.value,
                                    BackgroundColor: backColor.value,
                                    ErrorCorrectionLevel: errorLevel.value
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
                                
                                // Set download button
                                downloadBtn.onclick = function() {
                                    downloadQR(data.result.result, 'qrcode.png');
                                };
                            })
                            .catch(error => {
                                loadingIndicator.style.display = 'none';
                                showError('Error generating QR code: ' + error.message);
                            });
                        }
                        
                        // Function to show errors
                        function showError(message) {
                            errorDisplay.textContent = message;
                            errorDisplay.style.display = 'block';
                            qrResult.style.display = 'none';
                            initialState.style.display = 'none';
                        }
                        
                        // Function to download QR code
                        function downloadQR(base64Data, filename) {
                            const link = document.createElement('a');
                            link.href = 'data:image/png;base64,' + base64Data;
                            link.download = filename;
                            document.body.appendChild(link);
                            link.click();
                            document.body.removeChild(link);
                        }
                        
                        // Copy QR code to clipboard
                        copyBtn.addEventListener('click', function() {
                            try {
                                qrImage.toBlob(function(blob) {
                                    const item = new ClipboardItem({ 'image/png': blob });
                                    navigator.clipboard.write([item])
                                        .then(() => {
                                            const originalHTML = copyBtn.innerHTML;
                                            copyBtn.innerHTML = '<i class=""bi bi-check-lg""></i> Copied!';
                                            setTimeout(() => {
                                                copyBtn.innerHTML = originalHTML;
                                            }, 1500);
                                        })
                                        .catch(err => {
                                            console.error('Error copying image: ', err);
                                            alert('Failed to copy image to clipboard');
                                        });
                                });
                            } catch (e) {
                                alert('Your browser doesn\'t support copying images. Please use the download button instead.');
                                console.error(e);
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
