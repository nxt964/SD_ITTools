using ToolInterface;

namespace NumeronymGenerator;

public class NumeronymGeneratorTool : ITool
{
    public string Name => "Numeronym Generator";
    public string Category => "Text";
    public string Description => "Convert words into numeronyms (e.g., international → i18n).";

    public object? Execute(object input) => null;

    public string GetUI() => @"
<div class='container py-5 mx-auto' style='max-width: 600px;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>Numeronym Generator</h1>
        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Enter words separated by commas and convert them into numeronyms like <code>international → i18n</code>, <code>accessibility → a11y</code>.</p>
    </div>

    <div class='card shadow p-4'>
        <div class='mb-3'>
            <label class='form-label'>Input Words (separated by commas)</label>
            <textarea id='inputWords' class='form-control' rows='3' placeholder='e.g., international, accessibility, localization'></textarea>
        </div>

        <div class='mb-3'>
            <label class='form-label'>Numeronym Output</label>
            <div class='input-group'>
                <input id='output' class='form-control' readonly />
                
            </div>
        </div>

        <div class='mb-3 d-flex justify-content-center gap-4'>
            <button class='btn btn-outline-secondary' onclick='clearInput()'>
                <i class='bi bi-trash'></i>
                Clear
            </button>
            <button class='btn btn-outline-secondary' onclick='copyOutput()'>
                    <i class='bi bi-copy'></i> 
                    Copy
            </button>
        </div>
    </div>
</div>

<script>
    const input = document.getElementById('inputWords');
    const output = document.getElementById('output');

    function generateNumeronyms() {
        const text = input.value;
        const words = text.split(',').map(w => w.trim()).filter(w => w.length > 0);
        const result = words.map(word => {
            if (word.length <= 3) return word;
            return word[0] + (word.length - 2) + word[word.length - 1];
        }).join(', ');
        output.value = result;
    }

    function copyOutput() {
        output.select();
        document.execCommand('copy');
    }

    function clearInput() {
        input.value = '';
        output.value = '';
    }

    input.addEventListener('input', generateNumeronyms);
</script>";

    public void Stop() { }
}
