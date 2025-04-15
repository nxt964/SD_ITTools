using ToolInterface;

namespace JSONtoCSVTool;

public class JSONToCSVFormatter : ITool
{
    public string Name => "JSON To CSV Formatter";
    public string Category => "Data";
    public string Description => "Convert your JSON data into a flat CSV format instantly.";

    public object Execute(object? input) => null;

    public string GetUI() => @"
<div class='container py-5 mx-auto' style='max-width: 700px;'>
    <!-- Header -->
    <div class='header mb-4'>
        <h1 class='text-start m-0'>JSON To CSV Formatter</h1>
        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Convert your JSON data into a flat CSV format instantly.</p>
    </div>

    <!-- Card content -->
    <div class='card shadow p-4'>
        <div class='mb-3'>
            <label for='jsonInput' class='form-label'>JSON Input</label>
            <textarea id='jsonInput' class='form-control' rows='6' placeholder='Paste JSON content here'></textarea>
        </div>

        <button onclick='convertJSONtoCSV()' class='btn btn-primary mb-3'>Convert</button>

        <div class='mb-2 d-flex justify-content-between align-items-center'>
            <label for='csvResult' class='form-label mb-0'>Resulting CSV</label>
            <button onclick='copyCSV()' class='btn btn-sm btn-outline-secondary'>
                <i class='bi bi-copy'></i>
                Copy
            </button>
        </div>

        <textarea id='csvResult' class='form-control' rows='6' readonly placeholder='Resulting CSV'></textarea>
    </div>
</div>

<script>
    function convertJSONtoCSV() {
        const jsonText = document.getElementById('jsonInput').value.trim();
        if (!jsonText) {
            document.getElementById('csvResult').value = 'Please enter JSON data.';
            return;
        }

        try {
            const json = JSON.parse(jsonText);
            const array = Array.isArray(json) ? json : [json];

            if (array.length === 0) {
                document.getElementById('csvResult').value = 'Empty JSON array.';
                return;
            }

            const headers = Object.keys(array[0]);
            const csv = [
                headers.join(','),
                ...array.map(obj => headers.map(h => JSON.stringify(obj[h] ?? '')).join(','))
            ].join('\n');

            document.getElementById('csvResult').value = csv;
        } catch (err) {
            document.getElementById('csvResult').value = 'Invalid JSON format.';
        }
    }

    function copyCSV() {
        const output = document.getElementById('csvResult');
        output.select();
        output.setSelectionRange(0, 99999);
        document.execCommand('copy');
    }
</script>
";
    public void Stop() { }
}
