using ToolInterface;

namespace LengthUnitConverter
{
    public class LengthUnitConverter : ITool
    {
        public string Name => "Length Unit Converter";

        public string Category => "Measurement";

        public string Description => "Convert length units: mm, cm, m, km, in, ft, yd, mi.";

        public object Execute(object? input) => null;

        public string GetUI()
        {
            return @"
<div class='container py-5 mx-auto' style='max-width: 600px;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>Length Unit Converter</h1>
        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Convert length values between different units: mm, cm, m, km, in, ft, yd, mi.</p>
    </div>

    <div class='card shadow p-4'>
        <div class='mb-3'>
            <label class='form-label'>Value</label>
            <input type='number' id='inputValue' class='form-control' placeholder='Enter a value'>
        </div>

        <div class='row g-3 mb-3'>
            <div class='col-md-6'>
                <label class='form-label'>From Unit</label>
                <select id='fromUnit' class='form-select'>
                    <option value='mm'>Millimeter (mm)</option>
                    <option value='cm'>Centimeter (cm)</option>
                    <option value='m' selected>Meter (m)</option>
                    <option value='km'>Kilometer (km)</option>
                    <option value='in'>Inch (in)</option>
                    <option value='ft'>Foot (ft)</option>
                    <option value='yd'>Yard (yd)</option>
                    <option value='mi'>Mile (mi)</option>
                </select>
            </div>

            <div class='col-md-6'>
                <label class='form-label'>To Unit</label>
                <select id='toUnit' class='form-select'>
                    <option value='mm'>Millimeter (mm)</option>
                    <option value='cm'>Centimeter (cm)</option>
                    <option value='m'>Meter (m)</option>
                    <option value='km' selected>Kilometer (km)</option>
                    <option value='in'>Inch (in)</option>
                    <option value='ft'>Foot (ft)</option>
                    <option value='yd'>Yard (yd)</option>
                    <option value='mi'>Mile (mi)</option>
                </select>
            </div>
        </div>

        <div class='mb-3'>
            <label class='form-label'>Converted Value</label>
            <input id='outputValue' class='form-control' readonly>
        </div>
    </div>
</div>

<script>
    const inputValue = document.getElementById('inputValue');
    const fromUnit = document.getElementById('fromUnit');
    const toUnit = document.getElementById('toUnit');
    const outputValue = document.getElementById('outputValue');

    const unitToMeter = {
        mm: 0.001,
        cm: 0.01,
        m: 1,
        km: 1000,
        in: 0.0254,
        ft: 0.3048,
        yd: 0.9144,
        mi: 1609.344
    };

    function convertLength() {
        const value = parseFloat(inputValue.value);
        const from = fromUnit.value;
        const to = toUnit.value;

        if (isNaN(value)) {
            outputValue.value = '';
            return;
        }

        const inMeters = value * unitToMeter[from];
        const converted = inMeters / unitToMeter[to];
        outputValue.value = converted.toFixed(6);
    }

    [inputValue, fromUnit, toUnit].forEach(el => {
        el.addEventListener('input', convertLength);
        el.addEventListener('change', convertLength);
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
