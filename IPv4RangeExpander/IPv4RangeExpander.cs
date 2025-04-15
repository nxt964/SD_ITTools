using System.Net;
using System.Text.Json;
using ToolInterface;

namespace IPv4RangeExpander;

public class IPv4RangeRequest
{
    public string StartAddress { get; set; } = "";
    public string EndAddress { get; set; } = "";
}

public class IPv4RangeExpanderTool : ITool
{
    public string Name => "IPv4 Range Expander";
    public string Description => "Calculate the smallest subnet that contains the given IP range.";
    public string Category => "Networking";

    public object Execute(object? input)
    {
        try
        {
            var json = input?.ToString();
            if (string.IsNullOrWhiteSpace(json)) return "Invalid Request";

            var request = JsonSerializer.Deserialize<IPv4RangeRequest>(json);
            if (request == null ||
                !IPAddress.TryParse(request.StartAddress, out IPAddress startIP) ||
                !IPAddress.TryParse(request.EndAddress, out IPAddress endIP))
            {
                return "Invalid input";
            }

            uint ToUInt(IPAddress ip) => BitConverter.ToUInt32(ip.GetAddressBytes().Reverse().ToArray(), 0);
            IPAddress ToIP(uint val) => new(val >> 24 | ((val >> 8) & 0xFF00) | ((val << 8) & 0xFF0000) | (val << 24));

            uint start = ToUInt(startIP);
            uint end = ToUInt(endIP);
            if (start > end) (start, end) = (end, start);

            uint combined = start;
            uint diff = start ^ end;
            int cidr = 32;
            while ((diff & (1U << (32 - cidr))) != 0)
                cidr--;

            uint network = start & (0xFFFFFFFF << (32 - cidr));
            uint broadcast = network | ~(0xFFFFFFFF << (32 - cidr));

            var result = new Dictionary<string, object>
            {
                { "OldStart", ToIP(start).ToString() },
                { "OldEnd", ToIP(end).ToString() },
                { "NewStart", ToIP(network).ToString() },
                { "NewEnd", ToIP(broadcast).ToString() },
                { "AddressesInRange", (end - start + 1) },
                { "CIDR", $"{ToIP(network)}/{cidr}" }
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
                <h1 class='text-start m-0'>IPv4 range expander</h1>
                <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                <p class='text-start text-muted mb-0'>Given a start and an end IPv4 address, this tool calculates a valid IPv4 subnet along with its CIDR notation.</p>
            </div>
            <div class='card shadow-sm p-4'>
                <div class='mb-3'>
                    <label class='form-label fw-bold'>Start address</label>
                    <input type='text' id='startIP' class='form-control' placeholder='192.168.1.1'>
                </div>
                <div class='mb-3'>
                    <label class='form-label fw-bold'>End address</label>
                    <input type='text' id='endIP' class='form-control' placeholder='192.168.6.255'>
                </div>
                <button id='expandBtn' class='btn btn-primary w-100 mb-4'>Expand</button>
                <table class='table table-bordered'>
                    <thead><tr><th></th><th>Old Value</th><th>New Value</th></tr></thead>
                    <tbody id='resultTable'></tbody>
                </table>
            </div>
        </div>

        <script>
            document.getElementById('expandBtn').addEventListener('click', function () {
                let start = document.getElementById('startIP').value.trim();
                let end = document.getElementById('endIP').value.trim();
                if (!start || !end) return;

                let data = { StartAddress: start, EndAddress: end };

                fetch(window.location.pathname + '/execute', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(data)
                })
                .then(res => res.json())
                .then(res => {
                    let data = res.result;
                    let html = `
                        <tr><td class='fw-bold'>Start address</td><td>${data.OldStart}</td><td>${data.NewStart}</td></tr>
                        <tr><td class='fw-bold'>End address</td><td>${data.OldEnd}</td><td>${data.NewEnd}</td></tr>
                        <tr><td class='fw-bold'>Addresses in range</td><td colspan='2'>${data.AddressesInRange.toLocaleString()}</td></tr>
                        <tr><td class='fw-bold'>CIDR</td><td colspan='2'>${data.CIDR}</td></tr>
                    `;
                    document.getElementById('resultTable').innerHTML = html;
                })
                .catch(e => alert('Error: ' + e));
            });
        </script>
        ";
    }

    public void Stop() { }
}
