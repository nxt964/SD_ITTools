using ToolInterface;

namespace CSVtoJSONTool;

public class CSVtoJSONTool : ITool
{
    public string Name => "CSV To JSON Formatter";
    public string Category => "Data";
    public string Description => "Convert your CSV data into a structured JSON format instantly.";

    public object Execute(object? input) => null;

    public string GetUI() => @"
<div class='container py-5 mx-auto' style='max-width: 700px;'>
    <!-- Header -->
    <div class='header mb-4'>
        <h1 class='text-start m-0'>CSV To JSON Formatter</h1>
        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Convert your CSV data into a structured JSON format instantly.</p>
    </div>

    <!-- Card content -->
    <div class='card shadow p-4'>
        <div class='mb-3'>
            <label for='csvInput' class='form-label'>CSV Input</label>
            <textarea id='csvInput' class='form-control' rows='6' placeholder='Paste CSV content here'></textarea>
        </div>

        <button onclick='convertCSVtoJSON()' class='btn btn-primary mb-3'>Convert</button>

        <div class='mb-2 d-flex justify-content-between align-items-center'>
            <label for='jsonResult' class='form-label mb-0'>Resulting JSON</label>
            <button onclick='copyJSON()' class='btn btn-sm btn-outline-secondary'>
                <i class=""bi bi-copy""></i>
                Copy
            </button>
        </div>

        <textarea id='jsonResult' class='form-control' rows='6' readonly placeholder='Resulting JSON'></textarea>
    </div>
</div>

<script>
    function convertCSVtoJSON() {
        const csv = document.getElementById('csvInput').value.trim();
        const lines = csv.split(/\r?\n/).filter(line => line.trim() !== '');
        if (lines.length < 2) {
            document.getElementById('jsonResult').value = 'Invalid CSV format';
            return;
        }

        const headers = lines[0].split(',').map(h => h.trim());
        const result = lines.slice(1).map(line => {
            const values = line.split(',').map(v => v.trim());
            const obj = {};
            headers.forEach((key, i) => {
                obj[key] = values[i] ?? null;
            });
            return obj;
        });

        document.getElementById('jsonResult').value = JSON.stringify(result, null, 4);
    }

    function copyJSON() {
        const output = document.getElementById('jsonResult');
        output.select();
        output.setSelectionRange(0, 99999);
        document.execCommand('copy');
    }
</script>

";
    public void Stop() { }
}
