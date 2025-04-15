using System;
using System.Text.Json;
using ToolInterface;
using static System.Math;

namespace MathEvaluator
{
    public class MathEvaluatorRequest
    {
        public string Expression { get; set; } = "";
    }

    public class MathEvaluatorTool : ITool
    {
        public string Name => "Math Evaluator";
        public string Description => "Evaluate mathematical expressions including sqrt, sin, cos, abs, and more.";
        public string Category => "Math";

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json)) return "Invalid Request.";

                var request = JsonSerializer.Deserialize<MathEvaluatorRequest>(json);
                if (request == null || string.IsNullOrEmpty(request.Expression))
                    return "Invalid Request";

                var result = EvaluateExpression(request.Expression);

                return result.ToString("F6"); // Trả về kết quả số thực với 6 chữ số sau dấu phẩy
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private double EvaluateExpression(string expression)
        {
            try
            {
                // Chuẩn hóa biểu thức: loại bỏ khoảng trắng và chuyển đổi hàm
                expression = expression.Trim().ToLower()
                                     .Replace("pi", PI.ToString());

                // Thay thế các hàm toán học bằng giá trị
                double result = ParseAndEvaluate(expression);

                return result;
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid expression: {ex.Message}");
            }
        }

        private double ParseAndEvaluate(string expression)
        {
            // Xử lý các hàm toán học và toán tử
            if (double.TryParse(expression, out double value))
            {
                return value; // Nếu là số, trả về ngay
            }

            // Tìm kiếm các hàm toán học
            if (expression.StartsWith("sqrt("))
            {
                string inner = expression.Substring(5, expression.Length - 6); // Loại bỏ "sqrt()"
                return Sqrt(ParseAndEvaluate(inner));
            }
            else if (expression.StartsWith("sin("))
            {
                string inner = expression.Substring(4, expression.Length - 5); // Loại bỏ "sin()"
                return Sin(ParseAndEvaluate(inner) * PI / 180); // Chuyển độ sang radian
            }
            else if (expression.StartsWith("cos("))
            {
                string inner = expression.Substring(4, expression.Length - 5); // Loại bỏ "cos()"
                return Cos(ParseAndEvaluate(inner) * PI / 180); // Chuyển độ sang radian
            }
            else if (expression.StartsWith("tan("))
            {
                string inner = expression.Substring(4, expression.Length - 5); // Loại bỏ "tan()"
                return Tan(ParseAndEvaluate(inner) * PI / 180); // Chuyển độ sang radian
            }
            else if (expression.StartsWith("abs("))
            {
                string inner = expression.Substring(4, expression.Length - 5); // Loại bỏ "abs()"
                return Abs(ParseAndEvaluate(inner));
            }
            else if (expression.StartsWith("log10("))
            {
                string inner = expression.Substring(6, expression.Length - 7); // Loại bỏ "log10()"
                return Log10(ParseAndEvaluate(inner));
            }
            else if (expression.StartsWith("log("))
            {
                string inner = expression.Substring(4, expression.Length - 5); // Loại bỏ "log()"
                return Log(ParseAndEvaluate(inner));
            }
            else if (expression.StartsWith("exp("))
            {
                string inner = expression.Substring(4, expression.Length - 5); // Loại bỏ "exp()"
                return Exp(ParseAndEvaluate(inner));
            }

            // Xử lý các phép toán cơ bản (+, -, *, /)
            int opIndex = FindOperatorIndex(expression);
            if (opIndex > 0)
            {
                string left = expression.Substring(0, opIndex).Trim();
                string right = expression.Substring(opIndex + 1).Trim();
                double leftVal = ParseAndEvaluate(left);
                double rightVal = ParseAndEvaluate(right);

                char op = expression[opIndex];
                switch (op)
                {
                    case '+': return leftVal + rightVal;
                    case '-': return leftVal - rightVal;
                    case '*': return leftVal * rightVal;
                    case '/': return leftVal / rightVal;
                    default: throw new ArgumentException("Unsupported operator");
                }
            }

            throw new ArgumentException("Invalid expression");
        }

        private int FindOperatorIndex(string expression)
        {
            int parenCount = 0;
            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];
                if (c == '(') parenCount++;
                else if (c == ')') parenCount--;
                else if (parenCount == 0 && "+-*/".Contains(c))
                    return i;
            }
            return -1;
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 600px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>Math Evaluator</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Evaluate mathematical expressions (sqrt, sin, cos, abs, log, ln, exp, etc.). Results are returned as decimal numbers.</p>
                    </div>
                    <div class='card shadow-lg p-4'>
                        <div class='mb-3'>
                            <label class='form-label fw-bold'>Expression:</label>
                            <textarea id='expression' class='form-control' rows='2' placeholder='Enter math expression (e.g., sqrt(16), sin(0))'></textarea>
                        </div>
                        <button id='evaluate' class='btn btn-primary w-100 my-4'>Evaluate</button>
                        <div id=""result"" class=""mb-3"">
                            <label class='form-label fw-bold'>Result:</label>
                            <div class=""card p-3"">
                                <span id=""resultValue"" class=""text-muted"">Enter an expression and click 'Evaluate' to see the result.</span>
                            </div>
                            <button class=""btn btn-sm copy-btn mt-2"" data-target=""resultValue"">
                                <i class=""bi bi-copy""></i> Copy Result
                            </button>
                        </div>
                    </div>
                </div>

                <script>
                    let expressionArea = document.getElementById('expression');

                    expressionArea.addEventListener('input', function() {
                        expressionArea.classList.remove('border', 'border-danger');
                        expressionArea.placeholder = 'Enter math expression (e.g., sqrt(16), sin(0))';
                    });

                    document.getElementById('evaluate').addEventListener('click', function() {
                        let expression = expressionArea.value.trim();

                        if (expression === '') {
                            expressionArea.placeholder = 'Please enter a math expression.'; 
                            expressionArea.classList.add('border', 'border-danger');
                            expressionArea.focus();
                            return;
                        }

                        let data = { Expression: expression };

                        fetch(window.location.pathname + '/execute', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify(data)
                        })
                        .then(response => response.json())
                        .then(res => {
                            let resultElement = document.getElementById('resultValue');
                            if (resultElement) {
                                resultElement.textContent = res.result;
                            }
                        })
                        .catch(error => console.error('Error:', error));
                    });

                    // Copy result to clipboard
                    document.querySelector('.copy-btn').addEventListener('click', function() {
                        let targetId = this.getAttribute('data-target');
                        let resultText = document.getElementById(targetId)?.textContent;

                        if (resultText) {
                            navigator.clipboard.writeText(resultText).then(() => {
                                this.innerHTML = '<i class=""bi bi-check""></i> Copied!';
                                setTimeout(() => this.innerHTML = '<i class=""bi bi-copy""></i> Copy Result', 1000);
                            }).catch(err => console.error('Copy failed:', err));
                        }
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