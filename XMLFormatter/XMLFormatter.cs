using ToolInterface;
using System.Xml.Linq;
using System.Text.Json;

namespace XMLFormatter
{
    public class XMLFormatterTool : ITool
    {
        public string Name => "XML Formatter";
        public string Description => "Format your XML into a human-readable format with an option to collapse content.";
        public string Category => "Development";

        // Phương thức Execute nhận dữ liệu từ frontend
        public object Execute(object? input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.ToString()))
                return "Invalid or empty XML input.";

            try
            {
                // Deserialize input data to ExecuteInput object
                var inputData = JsonSerializer.Deserialize<ExecuteInput>(input.ToString());

                if (inputData == null || string.IsNullOrWhiteSpace(inputData.Input))
                    return "Invalid XML input.";

                // Parse XML string
                var xml = XElement.Parse(inputData.Input);

                // Apply collapse content if requested
                if (inputData.CollapseContent)
                {
                    // Convert XML to a collapsed format (single-line)
                    return xml.ToString(SaveOptions.DisableFormatting);
                }
                else
                {
                    // Return formatted XML with indentation
                    return xml.ToString();
                }
            }
            catch (Exception)
            {
                return "Invalid XML format.";
            }
        }

        // Giao diện UI để người dùng nhập XML và chọn chế độ Collapse content
        public string GetUI()
        {
            return @"
<div class='container py-5 mx-auto' style='max-width: 900px;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>XML Formatter</h1>
        <div class='separator my-2' style='width: 300px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Format your XML into a human-readable format with an option to collapse content.</p>
    </div>

    <div class='card shadow-sm p-4'>
        <h5 class='fw-bold mb-3'>📝 Raw XML Input</h5>
        <textarea id='rawXmlInput' class='form-control mb-4' rows='10' placeholder='Enter your raw XML here...'></textarea>

        <div class='form-check'>
            <input type='checkbox' class='form-check-input' id='collapseContent' />
            <label class='form-check-label' for='collapseContent'>Collapse content</label>
        </div>

        <button class='btn btn-primary mt-3' id='formatButton'>Format XML</button>

        <h5 class='fw-bold mt-4'>📜 Formatted XML</h5>
        <pre id='formattedXml' class='border p-3' style='white-space: pre-wrap; word-wrap: break-word;'></pre>
    </div>
</div>

<script>
    document.getElementById('formatButton').addEventListener('click', function() {
        const rawXml = document.getElementById('rawXmlInput').value;
        const collapseContent = document.getElementById('collapseContent').checked;
        fetch(window.location.pathname + '/execute', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ Input: rawXml, CollapseContent: collapseContent })
        })
        .then(response => response.json())
        .then(data => {
            document.getElementById('formattedXml').textContent = data.result;
        })
        .catch(error => {
            document.getElementById('formattedXml').textContent = 'Error: ' + error.message;
        });
    });
</script>
</div>";
        }

        // Phương thức Stop (không sử dụng trong trường hợp này)
        public void Stop() { }
    }

    // Class ExecuteInput để lưu dữ liệu nhận từ frontend
    public class ExecuteInput
    {
        public string Input { get; set; }
        public bool CollapseContent { get; set; }
    }
}
