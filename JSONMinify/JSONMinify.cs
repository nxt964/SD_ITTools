using ToolInterface;
using System.Text.Json;

namespace JSONMinify
{
    public class JSONMinifyTool : ITool
    {
        public string Name => "JSON Minify";
        public string Description => "Minify your JSON string into a compact format without unnecessary whitespaces.";
        public string Category => "Development";

        // Execute method to minify the input JSON
        public object Execute(object? input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ToString()))
                return "Invalid or empty JSON input.";

            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(input.ToString());
                var minifiedJson = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = false });
                return minifiedJson;
            }
            catch (JsonException)
            {
                return "Invalid JSON format.";
            }
        }

        // Return the HTML UI for the tool
        public string GetUI()
        {
            return @"
<div class='container py-5 mx-auto' style='max-width: 900px;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>JSON Minify</h1>
        <div class='separator my-2' style='width: 300px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Minify your JSON string into a compact format without unnecessary whitespaces.</p>
    </div>

    <div class='card shadow-sm p-4'>
        <h5 class='fw-bold mb-3'>📝 Raw JSON Input</h5>
        <textarea id='rawJsonInput' class='form-control mb-4' rows='10' placeholder='Enter your raw JSON here...'></textarea>

        <button class='btn btn-primary' id='minifyButton'>Minify JSON</button>

        <h5 class='fw-bold mt-4'>📜 Minified JSON</h5>
        <pre id='minifiedJson' class='border p-3' style='white-space: pre-wrap; word-wrap: break-word;'></pre>
    </div>
</div>

<script>
    document.getElementById('minifyButton').addEventListener('click', function() {
        const rawJson = document.getElementById('rawJsonInput').value;
        fetch(window.location.pathname + '/execute', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(rawJson)  // Truyền trực tiếp rawJson
        })
        .then(response => response.json())
        .then(data => {
            document.getElementById('minifiedJson').textContent = data.result;
        })
        .catch(error => {
            document.getElementById('minifiedJson').textContent = 'Error: ' + error.message;
        });
    });
</script>
</div>";
        }

        public void Stop() { }
    }
}
