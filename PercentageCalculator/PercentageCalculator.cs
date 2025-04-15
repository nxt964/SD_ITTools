using System;
using System.Text.Json;
using ToolInterface;

namespace PercentageCalculator
{
    public class PercentageCalculatorRequest
    {
        public string X { get; set; } = "";
        public string Y { get; set; } = "";
        public string From { get; set; } = "";
        public string To { get; set; } = "";
    }

    public class PercentageCalculatorTool : ITool
    {
        public string Name => "Percentage Calculator";
        public string Description => "Calculate percentages from one value to another or from a percentage to a value.";
        public string Category => "Math";

        public object Execute(object? input)
        {
            // Không cần xử lý server-side vì tính toán được thực hiện trên client
            return "This tool processes calculations client-side.";
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 600px; background-color: #f0f4f8;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>Percentage Calculator</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Easily calculate percentages from a value to another value, or from a percentage to a value.</p>
                    </div>
                    <div class='card shadow-lg p-4'>
                        <div class='mb-4 p-3 bg-white rounded' style='border: 1px solid #e0e0e0;'>
                            <label class='form-label fw-bold mb-2'>What is X % of Y</label>
                            <div class='input-group mb-2'>
                                <input type='number' id='x1' class='form-control' placeholder='X' onchange='calculatePercentage()'>
                                <span class='input-group-text'>%</span>
                                <span class='input-group-text'>of</span>
                                <input type='number' id='y1' class='form-control' placeholder='Y' onchange='calculatePercentage()'>
                                <span class='input-group-text'>=</span>
                                <input type='number' id='result1' class='form-control' placeholder='Result' readonly>
                            </div>
                        </div>
                        <div class='mb-4 p-3 bg-white rounded' style='border: 1px solid #e0e0e0;'>
                            <label class='form-label fw-bold mb-2'>X is what percent of Y</label>
                            <div class='input-group mb-2'>
                                <input type='number' id='x2' class='form-control' placeholder='X' onchange='calculatePercentage()'>
                                <span class='input-group-text'>is what percent of</span>
                                <input type='number' id='y2' class='form-control' placeholder='Y' onchange='calculatePercentage()'>
                                <span class='input-group-text'>=</span>
                                <input type='number' id='result2' class='form-control' placeholder='Result' readonly>
                            </div>
                        </div>
                        <div class='p-3 bg-white rounded' style='border: 1px solid #e0e0e0;'>
                            <label class='form-label fw-bold mb-2'>What is the percentage increase/decrease</label>
                            <div class='input-group mb-2'>
                                <span class='input-group-text'>From</span>
                                <input type='number' id='from' class='form-control' placeholder='From' onchange='calculatePercentage()'>
                                <span class='input-group-text'>To</span>
                                <input type='number' id='to' class='form-control' placeholder='To' onchange='calculatePercentage()'>
                                <span class='input-group-text'>=</span>
                                <input type='number' id='result3' class='form-control' placeholder='Result' readonly>
                            </div>
                        </div>
                    </div>
                </div>

                <script>
                    function calculatePercentage() {
                        // First calculation: What is X % of Y
                        let x1 = parseFloat(document.getElementById('x1').value) || 0;
                        let y1 = parseFloat(document.getElementById('y1').value) || 0;
                        let result1 = document.getElementById('result1');
                        if (x1 !== 0 && y1 !== 0) {
                            result1.value = ((x1 / 100) * y1).toFixed(2);
                        } else {
                            result1.value = '';
                        }

                        // Second calculation: X is what percent of Y
                        let x2 = parseFloat(document.getElementById('x2').value) || 0;
                        let y2 = parseFloat(document.getElementById('y2').value) || 0;
                        let result2 = document.getElementById('result2');
                        if (x2 !== 0 && y2 !== 0) {
                            result2.value = ((x2 / y2) * 100).toFixed(2);
                        } else {
                            result2.value = '';
                        }

                        // Third calculation: Percentage increase/decrease from to
                        let from = parseFloat(document.getElementById('from').value) || 0;
                        let to = parseFloat(document.getElementById('to').value) || 0;
                        let result3 = document.getElementById('result3');
                        if (from !== 0 && to !== 0) {
                            let difference = to - from;
                            result3.value = ((difference / from) * 100).toFixed(2);
                        } else {
                            result3.value = '';
                        }
                    }
                </script>
            ";
        }

        public void Stop()
        {
            return;
        }
    }
}