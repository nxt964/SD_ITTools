using System.Text;
using System.Text.Json;
using ToolInterface;

namespace SvgPlaceholder
{
    public class SvgPlaceholderRequest
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string BgColor { get; set; } = "#cccccc";
        public string TextColor { get; set; } = "#666666";
        public string Text { get; set; } = "";
        public int FontSize { get; set; } = 20;
    }

    public class SvgPlaceholderTool : ITool
    {
        public string Name => "SVG Placeholder Generator";
        public string Category => "Images and videos";
        public string Description => "Generate an SVG placeholder image with customizable dimensions and styles.";

        public object Execute(object? input)
        {
            try
            {
                var json = input?.ToString();
                if (string.IsNullOrWhiteSpace(json)) return "Invalid input";

                var request = JsonSerializer.Deserialize<SvgPlaceholderRequest>(json);
                if (request == null || request.Width <= 0 || request.Height <= 0)
                    return "Invalid request data";

                var svg = GenerateSvg(request);
                return new { svg };
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string GenerateSvg(SvgPlaceholderRequest r)
        {
            string text = string.IsNullOrWhiteSpace(r.Text)
                ? $"{r.Width}x{r.Height}"
                : r.Text;

            var sb = new StringBuilder();
            sb.AppendLine($"<svg xmlns='http://www.w3.org/2000/svg' width='{r.Width}' height='{r.Height}'>");
            sb.AppendLine($"  <rect width='100%' height='100%' fill='{r.BgColor}' />");
            sb.AppendLine($"  <text x='50%' y='50%' dominant-baseline='middle' text-anchor='middle' fill='{r.TextColor}' font-size='{r.FontSize}px' font-family='Arial, sans-serif'>{text}</text>");
            sb.AppendLine("</svg>");
            return sb.ToString();
        }

        public string GetUI()
        {
            return @"
                <div class='container py-5 mx-auto' style='max-width: 700px;'>
                  <div class='header mb-4'>
                    <h1 class='text-start m-0'>SVG Placeholder Generator</h1>
                    <div class='separator my-2' style='width: 400px; height: 1.5px; background: #ccc;'></div>
                    <p class='text-muted'>Generate an SVG image placeholder with custom size, colors, and text.</p>
                  </div>

                  <div class='card shadow p-4'>
                    <div class='row mb-3'>
                      <div class='col'>
                        <label class='form-label fw-bold'>Width</label>
                        <input id='svgWidth' type='number' class='form-control' value='300'>
                      </div>
                      <div class='col'>
                        <label class='form-label fw-bold'>Height</label>
                        <input id='svgHeight' type='number' class='form-control' value='150'>
                      </div>
                    </div>

                    <div class='row mb-3'>
                      <div class='col'>
                        <label class='form-label fw-bold'>Background Color</label>
                        <input id='bgColor' type='color' class='form-control form-control-color' value='#cccccc'>
                      </div>
                      <div class='col'>
                        <label class='form-label fw-bold'>Text Color</label>
                        <input id='textColor' type='color' class='form-control form-control-color' value='#666666'>
                      </div>
                    </div>

                    <div class='mb-3'>
                      <label class='form-label fw-bold'>Text (optional)</label>
                      <input id='text' type='text' class='form-control' placeholder='Default is WIDTHxHEIGHT'>
                    </div>

                    <div class='mb-3'>
                      <label class='form-label fw-bold'>Font Size (px)</label>
                      <input id='fontSize' type='number' class='form-control' value='20'>
                    </div>

                    <button id='generateSVG' class='btn btn-success w-100 my-3'>Generate SVG</button>
                    
                    <div id='svgPreview' class='border rounded p-3 text-center mb-3' style='background-color: #f9f9f9; min-height: 150px; overflow:auto;'></div>

                    <label class='form-label fw-bold'>SVG Code:</label>
                    <textarea id='svgCode' class='form-control mb-2' rows='6' readonly></textarea>
                    <button id='copySvg' class='btn btn-outline-primary btn-sm'>Copy SVG Code</button>
                  </div>
                </div>

                <script>
                document.getElementById('generateSVG').addEventListener('click', () => {
                    const Width = parseInt(document.getElementById('svgWidth').value);
                    const Height = parseInt(document.getElementById('svgHeight').value);
                    const BgColor = document.getElementById('bgColor').value;
                    const TextColor = document.getElementById('textColor').value;
                    const Text = document.getElementById('text').value.trim();
                    const FontSize = parseInt(document.getElementById('fontSize').value);

                    const payload = { Width, Height, BgColor, TextColor, Text, FontSize };

                    fetch(window.location.pathname + '/execute', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    })
                    .then(res => res.json())
                    .then(data => {
                        const svg = data.result.svg;
                        document.getElementById('svgPreview').innerHTML = svg;
                        document.getElementById('svgCode').value = svg;
                    })
                    .catch(err => console.error('Error:', err));
                });

                document.getElementById('copySvg').addEventListener('click', () => {
                    const svgCode = document.getElementById('svgCode');
                    svgCode.select();
                    document.execCommand('copy');
                });
                </script>
                ";
        }

        public void Stop()
        {
            return;
        }
    }
}
