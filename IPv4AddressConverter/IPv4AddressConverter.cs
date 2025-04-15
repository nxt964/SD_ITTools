using System.Net;
using System.Text.Json;
using ToolInterface;

namespace IPv4AddressConverter;

public class IPv4AddressRequest
{
    public string IPv4 { get; set; } = "";
}

public class IPv4AddressConverterTool : ITool
{
    public string Name => "IPv4 Address Converter";
    public string Description => "Convert an IP address into decimal, binary, hexadecimal, or even an IPv6 representation of it.";
    public string Category => "Networking";

    public object Execute(object? input)
    {
        try
        {
            var json = input?.ToString();
            if (string.IsNullOrWhiteSpace(json)) return "Invalid Request";

            var request = JsonSerializer.Deserialize<IPv4AddressRequest>(json);
            if (request == null || string.IsNullOrWhiteSpace(request.IPv4)) return "Invalid input";

            if (!IPAddress.TryParse(request.IPv4, out var ip)) return "Invalid IPv4 address";

            byte[] bytes = ip.GetAddressBytes();
            if (bytes.Length != 4) return "Only IPv4 addresses are supported";

            uint decimalVal = (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
            string hexVal = string.Concat(bytes.Select(b => b.ToString("X2")));
            string binaryVal = string.Concat(bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));

            string ipv6 = $"0000:0000:0000:0000:0000:ffff:{bytes[0]:x2}{bytes[1]:x2}:{bytes[2]:x2}{bytes[3]:x2}";
            string ipv6Short = $"::ffff:{bytes[0]:x2}{bytes[1]:x2}:{bytes[2]:x2}{bytes[3]:x2}";

            var result = new Dictionary<string, object>
            {
                { "Decimal", decimalVal },
                { "Hexadecimal", hexVal },
                { "Binary", binaryVal },
                { "Ipv6", ipv6 },
                { "Ipv6 (short)", ipv6Short }
            };

            return result;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public string GetUI()
    {
        return @"
                <div class='container py-5 mx-auto' style='max-width: 600px;'>
                    <div class='header mb-4'>
                        <h1 class='text-start m-0'>IPv4 Address Converter</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Convert an IP address into decimal, binary, hexadecimal, or even an IPv6 representation of it.</p>
                    </div>
                    <div class='card shadow-sm p-4'>
                        <div class='mb-3'>
                            <label for='ipInput' class='form-label fw-bold'>The ipv4 address:</label>
                            <input type='text' id='ipInput' class='form-control' placeholder='192.168.1.1'>
                        </div>
                        <button id='convertBtn' class='btn btn-primary w-100 mb-4'>Convert</button>
                        <table class='table table-bordered'>
                            <tbody id='resultTable'></tbody>
                        </table>
                    </div>
                </div>

                <script>
                    document.getElementById('convertBtn').addEventListener('click', function () {
                        let input = document.getElementById('ipInput').value.trim();
                        if (!input) return;

                        let data = { IPv4: input };

                        fetch(window.location.pathname + '/execute', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify(data)
                        })
                        .then(res => res.json())
                        .then(res => {
                            let data = res.result;
                            let html = '';
                            for (const [key, value] of Object.entries(data)) {
                                html += `<tr><td class='fw-bold'>${key}</td><td>${value}</td></tr>`;
                            }
                            document.getElementById('resultTable').innerHTML = html;
                        })
                        .catch(e => alert('Error: ' + e));
                    });
                </script>
                ";
    }

    public void Stop() { }
}
