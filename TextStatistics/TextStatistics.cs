using ToolInterface;
using System.Text;

namespace TextStatistics;

public class TextStatisticsTool : ITool
{
    public string Name => "Text Statistics";
    public string Category => "Text";
    public string Description => "Calculate character count, word count, byte size, and line count of input text.";

    public object? Execute(object input) => null;

    public string GetUI()
    {
        return @"
<div class='container py-5 mx-auto' style='max-width: 600px;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>Text Statistics</h1>
        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Real-time statistics for your input text: character count, word count, byte size, and line count.</p>
    </div>
    <div class='card shadow p-4'>
        <textarea id='textInput' class='form-control mb-3' rows='8' placeholder='Enter your text here...'></textarea>
        <ul class='list-group'>
            <li class='list-group-item d-flex justify-content-between align-items-center'>
                Character count <span id='charCount' class='badge bg-primary rounded-pill'>0</span>
            </li>
            <li class='list-group-item d-flex justify-content-between align-items-center'>
                Word count <span id='wordCount' class='badge bg-secondary rounded-pill'>0</span>
            </li>
            <li class='list-group-item d-flex justify-content-between align-items-center'>
                Byte size <span id='byteSize' class='badge bg-success rounded-pill'>0</span>
            </li>
            <li class='list-group-item d-flex justify-content-between align-items-center'>
                Line count <span id='lineCount' class='badge bg-info rounded-pill'>0</span>
            </li>
        </ul>
    </div>
</div>

<script>
    const textarea = document.getElementById('textInput');

    function updateStats() {
        const text = textarea.value;
        document.getElementById('charCount').innerText = text.length;
        document.getElementById('wordCount').innerText = (text.trim().match(/\S+/g) || []).length;
        document.getElementById('byteSize').innerText = new TextEncoder().encode(text).length;
        document.getElementById('lineCount').innerText = text.split(/\r?\n/).length;
    }

    textarea.addEventListener('input', updateStats);
</script>";
    }

    public void Stop() { }
}
