using ToolInterface;

namespace StringObfuscator;

public class StringObfuscatorTool : ITool
{
    public string Name => "String Obfuscator";
    public string Category => "Text";
    public string Description => "Hide parts of your string by replacing the middle section with asterisks.";

    public object? Execute(object input) => null;

    public string GetUI() => @"
<div class='container py-5 mx-auto' style='max-width: 600px;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>String Obfuscator</h1>
        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Hide parts of your string by keeping the beginning and end, replacing the middle with asterisks.</p>
    </div>

    <div class='card shadow p-4'>
        <div class='mb-3'>
            <label class='form-label'>Input Text</label>
            <textarea id='inputText' class='form-control' rows='3' placeholder='Enter your text here...'></textarea>
        </div>

        <div class='row g-3 mb-3'>
            <div class='col-md-4'>
                <label class='form-label'>Keep First</label>
                <input type='number' id='keepFirst' class='form-control' value='2' min='0' />
            </div>
            <div class='col-md-4'>
                <label class='form-label'>Keep Last</label>
                <input type='number' id='keepLast' class='form-control' value='2' min='0' />
            </div>
            <div class='col-md-4 d-flex align-items-end'>
                <div class='form-check'>
                    <input class='form-check-input' type='checkbox' id='keepSpaces' />
                    <label class='form-check-label' for='keepSpaces'>Keep spaces</label>
                </div>
            </div>
        </div>

        <div class='mb-3'>
            <label class='form-label'>Obfuscated Output</label>
            <div class='input-group'>
                <input id='output' class='form-control' readonly />
                <button class='btn btn-outline-secondary' onclick='copyOutput()'><i class=""bi bi-copy""></i></button>
            </div>
        </div>
    </div>
</div>

<script>
    const inputText = document.getElementById('inputText');
    const keepFirst = document.getElementById('keepFirst');
    const keepLast = document.getElementById('keepLast');
    const keepSpaces = document.getElementById('keepSpaces');
    const output = document.getElementById('output');

    function obfuscate() {
        const text = inputText.value;
        const first = parseInt(keepFirst.value) || 0;
        const last = parseInt(keepLast.value) || 0;
        const keepSpace = keepSpaces.checked;

        if (text.length <= first + last) {
            output.value = text;
            return;
        }

        const start = text.slice(0, first);
        const end = text.slice(text.length - last);
        const middlePart = text.slice(first, text.length - last);

        let obfuscatedMiddle = '';
        for (let ch of middlePart) {
            if (keepSpace && ch === ' ') {
                obfuscatedMiddle += ' ';
            } else {
                obfuscatedMiddle += '*';
            }
        }

        output.value = start + obfuscatedMiddle + end;
    }


    function copyOutput() {
        output.select();
        document.execCommand('copy');
    }

    [inputText, keepFirst, keepLast, keepSpaces].forEach(el => {
        el.addEventListener(el.type === 'checkbox' ? 'change' : 'input', obfuscate);
    });
</script>";

    public void Stop() { }
}
