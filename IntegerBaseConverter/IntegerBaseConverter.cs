using ToolInterface;
using System;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace IntegerBaseConverter
{
    public class IntegerBaseConverterRequest
    {
        public string InputValue { get; set; } = string.Empty;
        public int FromBase { get; set; }
        public int ToBase { get; set; }
        public bool ConvertToAllBases { get; set; } = false;
    }

    public class IntegerBaseConverterResponse
    {
        public string ConvertedValue { get; set; } = string.Empty;
        public Dictionary<string, string> AllConversions { get; set; } = new Dictionary<string, string>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
    
    public class IntegerBaseConverter : ITool
    {
        public string Name => "Integer Base Converter";
        public string Description => "Convert integers between different bases (binary, decimal, hexadecimal)";
        public string Category => "Converter";

        public object Execute(object? input)
        {
            if (input is not IntegerBaseConverterRequest request)
            {
                return new IntegerBaseConverterResponse { ErrorMessage = "Invalid input" };
            }

            try
            {
                // Validate request
                if (string.IsNullOrEmpty(request.InputValue))
                    return new IntegerBaseConverterResponse { ErrorMessage = "Input value cannot be empty" };

                if (request.FromBase < 2 || request.FromBase > 64)
                    return new IntegerBaseConverterResponse { ErrorMessage = "Source base must be between 2 and 64" };

                // Convert the input value to decimal
                int decimalValue = Convert.ToInt32(request.InputValue, request.FromBase);
                
                var response = new IntegerBaseConverterResponse();
                
                // Nếu yêu cầu chuyển đổi tới tất cả các cơ số phổ biến
                if (request.ConvertToAllBases)
                {
                    // Binary (base 2)
                    response.AllConversions["binary"] = Convert.ToString(decimalValue, 2);
                    
                    // Octal (base 8)
                    response.AllConversions["octal"] = Convert.ToString(decimalValue, 8);
                    
                    // Decimal (base 10)
                    response.AllConversions["decimal"] = decimalValue.ToString();
                    
                    // Hexadecimal (base 16)
                    response.AllConversions["hex"] = Convert.ToString(decimalValue, 16).ToUpper();
                    
                    // Base64
                    try {
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(decimalValue.ToString());
                        response.AllConversions["base64"] = Convert.ToBase64String(bytes);
                    }
                    catch {
                        response.AllConversions["base64"] = "Conversion error";
                    }
                    
                    // Nếu có cơ số tùy chỉnh (dùng ToBase làm cơ số tùy chỉnh)
                    if (request.ToBase >= 2 && request.ToBase <= 64)
                    {
                        response.AllConversions["custom"] = Convert.ToString(decimalValue, request.ToBase).ToUpper();
                    }
                }
                else
                {
                    // Chuyển đổi theo cách truyền thống
                    if (request.ToBase < 2 || request.ToBase > 64)
                        return new IntegerBaseConverterResponse { ErrorMessage = "Target base must be between 2 and 64" };
                        
                    // Convert the decimal value to the target base
                    string convertedValue = Convert.ToString(decimalValue, request.ToBase).ToUpper();
                    response.ConvertedValue = convertedValue;
                }

                return response;
            }
            catch (FormatException)
            {
                return new IntegerBaseConverterResponse { ErrorMessage = "Input value contains invalid characters for the selected base" };
            }
            catch (OverflowException)
            {
                return new IntegerBaseConverterResponse { ErrorMessage = "Input value is too large to convert" };
            }
            catch (Exception ex)
            {
                return new IntegerBaseConverterResponse { ErrorMessage = ex.Message };
            }
        }

        public string GetUI()
        {
            return @"
            <div class='container py-5 mx-auto' style='max-width: 600px;'>
                <div class='header mb-4'>
                    <h1 class='text-start m-0'>Base Converter</h1>
                    <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                    <p class='text-start text-muted mb-0'>Convert numbers between different number systems (binary, decimal, hexadecimal, etc).</p>
                </div>

                <div class='card shadow-lg p-4'>
                    <div class='row mb-3 align-items-center'>
                        <div class='col-md-7'>
                            <label class='form-label fw-bold'>Input Number:</label>
                            <input type='text' id='inputNumber' class='form-control' value='42'>
                        </div>
                        <div class='col-md-5'>
                            <label class='form-label fw-bold'>Input Base:</label>
                            <input type='text' id='inputBase' class='form-control' value='10'>
                        </div>
                    </div>

                    <button id='convertBtn' class='btn btn-primary w-100 my-4'>Convert</button>

                    <div class='mb-3'>
                        <label class='form-label fw-bold'>Conversion Results:</label>
                        <table class='table table-bordered w-100' style='table-layout: fixed;'>
                            <tbody>
                                <tr>
                                    <td class='w-25' style='background-color: #f1f5f9;'>Binary</td>
                                    <td id='binaryOutput' class='text-truncate width-auto'></td>
                                    <td class='text-center' style='width: 30px;'>
                                        <button class='btn btn-sm copy-btn p-0' data-target='binaryOutput'>
                                            <i class='bi bi-copy'></i>
                                        </button>
                                    </td>
                                </tr>
                                <tr>
                                    <td class='w-25' style='background-color: #f1f5f9;'>Octal</td>
                                    <td id='octalOutput' class='text-truncate width-auto'></td>
                                    <td class='text-center' style='width: 30px;'>
                                        <button class='btn btn-sm copy-btn p-0' data-target='octalOutput'>
                                            <i class='bi bi-copy'></i>
                                        </button>
                                    </td>
                                </tr>
                                <tr>
                                    <td class='w-25' style='background-color: #f1f5f9;'>Decimal</td>
                                    <td id='decimalOutput' class='text-truncate width-auto'></td>
                                    <td class='text-center' style='width: 30px;'>
                                        <button class='btn btn-sm copy-btn p-0' data-target='decimalOutput'>
                                            <i class='bi bi-copy'></i>
                                        </button>
                                    </td>
                                </tr>
                                <tr>
                                    <td class='w-25' style='background-color: #f1f5f9;'>Hexadecimal</td>
                                    <td id='hexOutput' class='text-truncate width-auto'></td>
                                    <td class='text-center' style='width: 30px;'>
                                        <button class='btn btn-sm copy-btn p-0' data-target='hexOutput'>
                                            <i class='bi bi-copy'></i>
                                        </button>
                                    </td>
                                </tr>
                                <tr>
                                    <td class='w-25' style='background-color: #f1f5f9;'>Base 64</td>
                                    <td id='base64Output' class='text-truncate width-auto'></td>
                                    <td class='text-center' style='width: 30px;'>
                                        <button class='btn btn-sm copy-btn p-0' data-target='base64Output'>
                                            <i class='bi bi-copy'></i>
                                        </button>
                                    </td>
                                </tr>
                                <tr>
                                    <td class='w-25' style='background-color: #f1f5f9;'>Custom:
                                    <input type='number' id='customBase' class='form-control mb-2' min='2' max='64'>

                                    </td>
                                    <td id='baseCustomOutput' class='text-truncate width-auto'></td>
                                    <td class='text-center' style='width: 30px;'>
                                        <button class='btn btn-sm copy-btn p-0' data-target='baseCustomOutput'>
                                            <i class='bi bi-copy'></i>
                                        </button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>

            <script>
            document.addEventListener('DOMContentLoaded', function() {
                // Elements
                const inputNumber = document.getElementById('inputNumber');
                const inputBase = document.getElementById('inputBase');
                const customBase = document.getElementById('customBase');
                const convertBtn = document.getElementById('convertBtn');
                const outputs = {
                    binary: document.getElementById('binaryOutput'),
                    octal: document.getElementById('octalOutput'),
                    decimal: document.getElementById('decimalOutput'),
                    hex: document.getElementById('hexOutput'),
                    base64: document.getElementById('base64Output'),
                    custom: document.getElementById('baseCustomOutput')
                };
                
                // Set initial custom base value
                customBase.value = 36;
                
                // Add event listeners
                inputNumber.addEventListener('input', validateInputNumber);
                inputBase.addEventListener('input', validateInputBase);
                customBase.addEventListener('input', validateCustomBase);
                convertBtn.addEventListener('click', convert);
                customBase.addEventListener('change', updateCustomConversion);
                
                // Initial validation and conversion
                validateInputNumber();
                validateInputBase();
                validateCustomBase();
                convert();
                
                // Setup copy buttons
                document.querySelectorAll('.copy-btn').forEach(button => {
                    button.addEventListener('click', function() {
                        const target = this.getAttribute('data-target');
                        const value = document.getElementById(target).textContent;
                        copyToClipboard(value, this);
                    });
                });
                
                // Functions
                function validateInputNumber() {
                    let value = inputNumber.value.trim();
                    let base = parseInt(inputBase.value);
                    
                    if (!value) {
                        setInvalid(inputNumber, 'Input cannot be empty');
                        return false;
                    }
                    
                    // Validate characters for the given base
                    if (!isValidForBase(value, base)) {
                        setInvalid(inputNumber, `Invalid characters for base ${base}`);
                        return false;
                    }
                    
                    setValid(inputNumber);
                    return true;
                }
                
                function validateInputBase() {
                    let value = parseInt(inputBase.value);
                    
                    if (isNaN(value)) {
                        setInvalid(inputBase, 'Base must be a number');
                        return false;
                    }
                    
                    if (value < 2 || value > 64) {
                        setInvalid(inputBase, 'Base must be between 2 and 64');
                        return false;
                    }
                    
                    setValid(inputBase);
                    validateInputNumber(); // Re-validate input when base changes
                    return true;
                }
                
                function validateCustomBase() {
                    let value = parseInt(customBase.value);
                    
                    if (isNaN(value)) {
                        setInvalid(customBase, 'Base must be a number');
                        return false;
                    }
                    
                    if (value < 2 || value > 64) {
                        setInvalid(customBase, 'Base must be between 2 and 64');
                        return false;
                    }
                    
                    setValid(customBase);
                    return true;
                }
                
                function isValidForBase(value, base) {
                    const validChars = getValidCharsForBase(base);
                    const regex = new RegExp(`^[${validChars}]+$`, 'i');
                    return regex.test(value);
                }
                
                function getValidCharsForBase(base) {
                    let chars = '0123456789';
                    if (base > 10) {
                        chars += 'ABCDEFGHIJKLMNOPQRSTUVWXYZ'.substring(0, base - 10);
                        chars += 'abcdefghijklmnopqrstuvwxyz'.substring(0, base - 10);
                    } else {
                        chars = chars.substring(0, base);
                    }
                    return chars;
                }
                
                function setInvalid(element, message) {
                    element.classList.add('is-invalid');
                    
                    // Create or update error message
                    let errorDiv = element.nextElementSibling;
                    if (!errorDiv || !errorDiv.classList.contains('invalid-feedback')) {
                        errorDiv = document.createElement('div');
                        errorDiv.className = 'invalid-feedback';
                        element.parentNode.insertBefore(errorDiv, element.nextSibling);
                    }
                    errorDiv.textContent = message;
                }
                
                function setValid(element) {
                    element.classList.remove('is-invalid');
                    element.classList.add('is-valid');
                    
                    // Remove error message if exists
                    let errorDiv = element.nextElementSibling;
                    if (errorDiv && errorDiv.classList.contains('invalid-feedback')) {
                        errorDiv.remove();
                    }
                }
                
                function convert() {
                    if (!validateInputNumber() || !validateInputBase() || !validateCustomBase()) {
                        clearOutputs();
                        return;
                    }
                    
                    try {
                        const num = inputNumber.value.trim();
                        const base = parseInt(inputBase.value);
                        
                        // Convert to decimal first
                        const decimalValue = convertToDecimal(num, base);
                        
                        if (isNaN(decimalValue)) {
                            throw new Error('Invalid conversion');
                        }
                        
                        // Convert decimal to other bases
                        outputs.binary.textContent = decimalValue.toString(2);
                        outputs.octal.textContent = decimalValue.toString(8);
                        outputs.decimal.textContent = decimalValue.toString(10);
                        outputs.hex.textContent = decimalValue.toString(16).toUpperCase();
                        outputs.base64.textContent = convertFromDecimal(num,64);
                        
                        updateCustomConversion();
                        
                    } catch (error) {
                        console.error('Conversion error:', error);
                        clearOutputs();
                    }
                }

                function convertToDecimal(num, base) {
                    if (base < 2 || base > 64) {
                        throw new Error(""Base must be between 2 and 64"");
                    }
    
                    const digits = ""0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/"";
                    let value = 0;
    
                    for (let i = 0; i < num.length; i++) {
                        let digit = digits.indexOf(num[i]);
                        if (digit === -1 || digit >= base) {
                            throw new Error(""Invalid character for base "" + base);
                        }
                        value = value * base + digit;
                    }
    
                    return value;
                }

                function convertFromDecimal(num, base) {
                    if (base < 2 || base > 64) {
                        throw new Error(""Base must be between 2 and 64"");
                    }
    
                    const digits = ""0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz+/"";
                    if (num === 0) return ""0"";
    
                    let result = """";
                    while (num > 0) {
                        result = digits[num % base] + result;
                        num = Math.floor(num / base);
                    }
    
                    return result;
                }
                
                function updateCustomConversion() {
                    if (!validateInputNumber() || !validateInputBase() || !validateCustomBase()) {
                        outputs.custom.textContent = '';
                        return;
                    }
                    
                    try {
                        const num = inputNumber.value.trim();
                        const inputBaseValue = parseInt(inputBase.value);
                        const customBaseValue = parseInt(customBase.value);
                        
                        // Convert from input base to decimal
                        const decimalValue = convertToDecimal(num, inputBaseValue);
                        
                        if (isNaN(decimalValue)) {
                            throw new Error('Invalid conversion');
                        }
                        
                        // Convert from decimal to custom base
                        outputs.custom.textContent = convertFromDecimal(decimalValue, customBaseValue);
                        
                    } catch (error) {
                        console.error('Custom conversion error:', error);
                        outputs.custom.textContent = 'Conversion error';
                    }
                }

                
                function clearOutputs() {
                    for (let key in outputs) {
                        outputs[key].textContent = '';
                    }
                }
                
                function copyToClipboard(text, button) {
                    navigator.clipboard.writeText(text)
                        .then(() => {
                            // Provide visual feedback
                            const originalIcon = button.innerHTML;
                            button.innerHTML = '<i class=\'bi bi-check2\'></i>';
                            
                            setTimeout(() => {
                                button.innerHTML = originalIcon;
                            }, 1500);
                        })
                        .catch(err => {
                            console.error('Copy failed:', err);
                            alert('Copy to clipboard failed');
                        });
                }

                // Thêm chức năng từ code mới (đã tích hợp vào code hiện có)
                function decimalToBase64(decimalValue) {
                    return btoa(decimalValue.toString());
                }
            });
            </script>
            ";
        }

        public void Stop()
        {
            // No resources to clean up
            return;
        }
    }
}
