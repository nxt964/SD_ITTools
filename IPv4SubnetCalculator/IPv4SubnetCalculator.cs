using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using ToolInterface;

namespace IPv4SubnetCalculator;

public class IPv4SubnetRequest
{
    public string AddressWithMask { get; set; } = "";
}

public class IPv4SubnetCalculatorTool : ITool
{
    public string Name => "IPv4 Subnet Calculator";
    public string Description => "Parse your IPv4 CIDR blocks and get all the info you need about your subnet.";
    public string Category => "Networking";

    public object Execute(object? input)
    {
        try
        {
            var json = input?.ToString();
            if (string.IsNullOrWhiteSpace(json)) return "Invalid Request";

            var request = JsonSerializer.Deserialize<IPv4SubnetRequest>(json);
            if (request == null || string.IsNullOrEmpty(request.AddressWithMask)) return "Invalid input";

            string[] parts = request.AddressWithMask.Split('/');
            if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out IPAddress ip) || !int.TryParse(parts[1], out int cidr))
                return "Invalid CIDR format";

            uint ipUint = BitConverter.ToUInt32(ip.GetAddressBytes().Reverse().ToArray(), 0);
            uint mask = cidr == 0 ? 0 : 0xFFFFFFFF << (32 - cidr);
            uint network = ipUint & mask;
            uint broadcast = network | ~mask;
            uint first = network + 1;
            uint last = broadcast - 1;
            uint wildcard = ~mask;

            IPAddress Net(uint val) => new(val >> 24 | ((val >> 8) & 0xFF00) | ((val << 8) & 0xFF0000) | (val << 24));
            string GetClass(byte firstOctet) =>
                firstOctet < 128 ? "A" :
                firstOctet < 192 ? "B" :
                firstOctet < 224 ? "C" :
                firstOctet < 240 ? "D" : "E";

            var result = new Dictionary<string, object>
            {
                { "Netmask", request.AddressWithMask },
                { "NetworkAddress", Net(network).ToString() },
                { "NetworkMask", Net(mask).ToString() },
                { "NetworkMaskBinary", Convert.ToString(mask, 2).PadLeft(32, '0').Insert(8, ".").Insert(17, ".").Insert(26, ".") },
                { "CIDRNotation", "/" + cidr },
                { "WildcardMask", Net(wildcard).ToString() },
                { "NetworkSize", (uint)(broadcast - network + 1) },
                { "FirstAddress", Net(first).ToString() },
                { "LastAddress", Net(last).ToString() },
                { "BroadcastAddress", Net(broadcast).ToString() },
                { "IPClass", GetClass(ip.GetAddressBytes()[0]) }
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
                        <h1 class='text-start m-0'>IPv4 Subnet Calculator</h1>
                        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
                        <p class='text-start text-muted mb-0'>Parse your IPv4 CIDR blocks and get all the info you need about your subnet.</p>
                    </div>
                    <div class='card shadow-sm p-4'>
                        <div class='mb-3'>
                            <label for='cidrInput' class='form-label fw-bold'>An IPv4 address with or without mask</label>
                            <input type='text' id='cidrInput' class='form-control' placeholder='192.168.0.0/24'>
                        </div>
                        <button id='calcBtn' class='btn btn-primary w-100 mb-4'>Calculate</button>
                        <table class='table table-bordered'>
                            <tbody id='resultTable'></tbody>
                        </table>
                    </div>
                </div>

                <script>
                    document.getElementById('calcBtn').addEventListener('click', function () {
                        let input = document.getElementById('cidrInput').value.trim();
                        if (!input) return;

                        let data = { AddressWithMask: input };

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
