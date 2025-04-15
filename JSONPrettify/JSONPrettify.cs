using ToolInterface;
using System.Text.Json;

namespace JSONPrettify
{
    public class JSONPrettifyTool : ITool
    {
        public string Name => "JSON Prettify";
        public string Description => "Prettify your JSON string into a friendly, human-readable format.";
        public string Category => "Development";

        public object Execute(object? input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ToString()))
            {
                return "Invalid or empty JSON input."; // Trả về thông báo lỗi trực tiếp
            }

            try
            {
                // Parse và prettify JSON input
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(input.ToString());
                var prettifiedJson = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });

                // Trả về JSON đã được prettified
                return prettifiedJson;
            }
            catch (JsonException)
            {
                return "Invalid JSON format."; // Trả về thông báo lỗi nếu không phải JSON hợp lệ
            }
        }

        public string GetUI()
        {
            return @"
<div class='container py-5 mx-auto' style='max-width: 900px;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>JSON Prettify</h1>
        <div class='separator my-2' style='width: 300px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Prettify your JSON string into a friendly, human-readable format.</p>
    </div>

    <div class='card shadow-sm p-4'>
        <h5 class='fw-bold mb-3'>📝 Raw JSON Input</h5>
        <textarea id='rawJsonInput' class='form-control mb-4' rows='10' placeholder='Enter your raw JSON here...'></textarea>

        <button class='btn btn-primary' id='prettifyButton'>Prettify JSON</button>

        <h5 class='fw-bold mt-4'>📜 Prettified JSON</h5>
        <pre id='prettifiedJson' class='border p-3' style='white-space: pre-wrap; word-wrap: break-word;'></pre>
    </div>
</div>

<script>
    document.getElementById('prettifyButton').addEventListener('click', function() {
        const rawJson = document.getElementById('rawJsonInput').value;

        fetch(window.location.pathname + '/execute', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(rawJson)
        })
        .then(response => response.json())
        .then(data => {
            // Nếu không có lỗi, hiển thị kết quả prettified JSON
            if (data.success) {
                document.getElementById('prettifiedJson').textContent = data.result;
            } else {
                // Nếu có lỗi, hiển thị thông báo lỗi
                document.getElementById('prettifiedJson').textContent = 'Error: ' + data.result;
            }
        })
        .catch(error => {
            // Nếu có lỗi khi gọi API, hiển thị lỗi
            document.getElementById('prettifiedJson').textContent = 'Error: ' + error.message;
        });
    });
</script>
</div>";
        }

        public void Stop() { }
    }
}
