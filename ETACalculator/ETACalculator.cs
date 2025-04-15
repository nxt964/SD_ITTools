using System;
using System.Text.Json;
using ToolInterface;

namespace ETACalculator
{
    public class ETACalculatorRequest
    {
        public double TotalAmount { get; set; }
        public DateTime StartTime { get; set; }
        public double UnitsConsumed { get; set; }
        public double TimeSpanValue { get; set; }
        public string TimeSpanUnit { get; set; } = "seconds";
    }

    public class ETACalculatorTool : ITool
    {
        public string Name => "ETA Calculator";
        public string Description => "Estimate total time and end time based on consumption rate.";
        public string Category => "Math";

        public object Execute(object? input)
        {
            if (input is null) return new { error = "Input is null" };

            var json = JsonSerializer.Serialize(input);
            var request = JsonSerializer.Deserialize<ETACalculatorRequest>(json);
            if (request is null) return new { error = "Invalid input" };

            double totalUnits = request.TotalAmount;
            double unitsPerSpan = request.UnitsConsumed;
            double span = request.TimeSpanValue;

            double msPerUnit = GetMilliseconds(span, request.TimeSpanUnit) / unitsPerSpan;
            var totalTime = TimeSpan.FromMilliseconds(msPerUnit * totalUnits);
            var endTime = request.StartTime.Add(totalTime);

            string totalTimeFormatted = FormatTimeSpan(totalTime);

            return new
            {
                TotalDuration = totalTimeFormatted,
                ItWillEnd = endTime.ToString("dd/MM/yyyy HH:mm:ss")
            };
        }

        private double GetMilliseconds(double value, string unit)
        {
            return unit switch
            {
                "milliseconds" => value,
                "seconds" => value * 1000,
                "minutes" => value * 60 * 1000,
                "hours" => value * 60 * 60 * 1000,
                _ => value * 1000
            };
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            int hours = (int)ts.TotalHours;
            int minutes = ts.Minutes;
            int seconds = ts.Seconds;
            return $"{hours}h {minutes}m {seconds}s";
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 600px; background-color: #f0f4f8;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>ETA Calculator</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>An ETA (Estimated Time of Arrival) calculator to determine the approximate end time of a task, for example, the end time and duration of a file download.</p>
                    </div>
                    <div class='card shadow p-4'>
                        <div class='mb-3'>
                            <label class='form-label fw-bold'>Amount of element to consume</label>
                            <input type='number' id='totalAmount' class='form-control' placeholder='e.g., 100'>
                        </div>
                        <div class='mb-3'>
                            <label class='form-label fw-bold'>The consumption started at</label>
                            <input type='datetime-local' id='startTime' class='form-control'>
                        </div>
                        <div class='mb-3'>
                            <label class='form-label fw-bold'>Amount of unit consumed by time span</label>
                            <div class='input-group'>
                                <input type='number' id='unitsConsumed' class='form-control' placeholder='e.g., 5'>
                                <span class='input-group-text'>every</span>
                                <input type='number' id='timeSpanValue' class='form-control' placeholder='e.g., 10'>
                                <select id='timeSpanUnit' class='form-select'>
                                    <option value='milliseconds'>milliseconds</option>
                                    <option value='seconds' selected>seconds</option>
                                    <option value='minutes'>minutes</option>
                                    <option value='hours'>hours</option>
                                </select>
                            </div>
                        </div>
                        <button class='btn btn-primary w-100 mt-3' onclick='calculateETA()'>Calculate</button>
                        <div class='mt-4 p-3 bg-light rounded'>
                            <p><strong>Total duration:</strong> <span id='durationResult'></span></p>
                            <p><strong>It will end at:</strong> <span id='endTime'></span></p>
                        </div>
                    </div>
                </div>
                <script>
                function calculateETA() {
                    const totalAmount = parseFloat(document.getElementById('totalAmount').value);
                    const startTime = document.getElementById('startTime').value;
                    const unitsConsumed = parseFloat(document.getElementById('unitsConsumed').value);
                    const timeSpanValue = parseFloat(document.getElementById('timeSpanValue').value);
                    const timeSpanUnit = document.getElementById('timeSpanUnit').value;

                    if (!totalAmount || !startTime || !unitsConsumed || !timeSpanValue) {
                        alert('Please fill in all fields.');
                        return;
                    }

                    fetch(window.location.pathname + '/execute', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({
                            TotalAmount: totalAmount,
                            StartTime: startTime,
                            UnitsConsumed: unitsConsumed,
                            TimeSpanValue: timeSpanValue,
                            TimeSpanUnit: timeSpanUnit
                        })
                    })
                    .then(res => res.json())
                    .then(res => {
                        if (res.result) {
                            document.getElementById('durationResult').innerText = res.result.totalDuration;
                            document.getElementById('endTime').innerText = res.result.itWillEnd;
                        } else {
                            alert('Calculation failed');
                        }
                    });
                }
                </script>
                ";
        }

        public void Stop()
        {
        }
    }
}
