using ToolInterface;

namespace Chronometer
{
    public class ChronometerTool : ITool
    {
        public string Name => "Chronometer";
        public string Category => "Measurement";
        public string Description => "Monitor the duration of a thing. Basically a chronometer with simple chronometer features.";

        public object? Execute(object input) => null;

        public string GetUI()
        {
            return @"
<div class='container py-5 mx-auto' style='max-width: 600px;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>Chronometer</h1>
        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>A simple stopwatch that lets you start, stop, and reset time tracking with millisecond precision.</p>
    </div>

    <div class='card shadow p-4 text-center'>
        <div id='chronometer-display' class='mb-4' style='font-size: 2rem; font-family: monospace;'>00:00:00.000</div>
        <div>
            <button class='btn btn-primary me-2' id='chronometer-toggle'>Start</button>
            <button class='btn btn-secondary' id='chronometer-reset'>Reset</button>
        </div>
    </div>
</div>

<script>
    let isRunning = false;
    let startTime = 0;
    let elapsed = 0;
    let timerInterval;

    function formatTime(ms) {
        const totalSeconds = Math.floor(ms / 1000);
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;
        const milliseconds = ms % 1000;

        return (
            String(hours).padStart(2, '0') + ':' +
            String(minutes).padStart(2, '0') + ':' +
            String(seconds).padStart(2, '0') + '.' +
            String(milliseconds).padStart(3, '0')
        );
    }

    function updateDisplay() {
        const now = Date.now();
        const time = elapsed + (isRunning ? now - startTime : 0);
        document.getElementById('chronometer-display').textContent = formatTime(time);
    }

    document.getElementById('chronometer-toggle').addEventListener('click', function () {
        if (!isRunning) {
            startTime = Date.now();
            timerInterval = setInterval(updateDisplay, 31);
            this.textContent = 'Stop';
            isRunning = true;
        } else {
            clearInterval(timerInterval);
            elapsed += Date.now() - startTime;
            this.textContent = 'Start';
            isRunning = false;
        }
    });

    document.getElementById('chronometer-reset').addEventListener('click', function () {
        clearInterval(timerInterval);
        isRunning = false;
        startTime = 0;
        elapsed = 0;
        document.getElementById('chronometer-display').textContent = '00:00:00.000';
        document.getElementById('chronometer-toggle').textContent = 'Start';
    });
</script>";
        }

        public void Stop() { }
    }
}
