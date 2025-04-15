using static System.Runtime.InteropServices.JavaScript.JSType;
using System.ComponentModel;
using System.Reflection.Metadata;
using System;
using ToolInterface;

namespace TemperatureConverter
{
    public class TemperatureConverterTool : ITool
    {
        public string Name => "Temperature Converter";
        public string Category => "Measurement";
        public string Description => "Convert between Kelvin, Celsius, Fahrenheit, Rankine, Delisle, Newton, Réaumur, and Rømer.";

        public object? Execute(object input) => null;

        public string GetUI()
        {
            return @"
<div class='container py-5 mx-auto' style='max-width: 600px; background-color: #f0f4f8;'>
    <div class='header mb-4'>
        <h1 class='text-start m-0'>Temperature Converter</h1>
        <div class='separator my-2' style='width: 350px; height: 1.5px; opacity: 0.3; background: #a1a1a1'></div>
        <p class='text-start text-muted mb-0'>Degrees temperature conversions for Kelvin, Celsius, Fahrenheit, Rankine, Delisle, Newton, Réaumur, and Rømer.</p>
    </div>
    <div id='temp-container' class='card shadow p-4'>
        <div id='temp-fields'></div>
    </div>
</div>

<script>
    const units = [
        { name: 'Kelvin', abbr: 'K' },
        { name: 'Celsius', abbr: '°C' },
        { name: 'Fahrenheit', abbr: '°F' },
        { name: 'Rankine', abbr: '°R' },
        { name: 'Delisle', abbr: '°De' },
        { name: 'Newton', abbr: '°N' },
        { name: 'Réaumur', abbr: '°Ré' },
        { name: 'Rømer', abbr: '°Rø' }
    ];

    let currentEditing = false;

    function createRow(unit) {
        return `
        <div class='input-group mb-2'>
            <span class='input-group-text' style='width: 100px;'>${unit.name}</span>
            <input class='form-control temp-input' id=${unit.name} placeholder=""${unit.name}"" type='number'/>
            <button class='btn btn-outline-secondary' onclick=""adjust('${unit.name}', -1)"">−</button>
            <button class='btn btn-outline-secondary' onclick=""adjust('${unit.name}', 1)"">+</button>
            <span class='input-group-text' style='width: 50px;'>${unit.abbr}</span>
        </div>`;
    }

    function renderRows()
    {
        const container = document.getElementById('temp-fields');
        container.innerHTML = units.map(createRow).join('');
        document.querySelectorAll('.temp-input').forEach(input => {
            input.addEventListener('input', (e) => {
                if (currentEditing) return;
                currentEditing = true;
                convertFrom(e.target.id, parseFloat(e.target.value));
                currentEditing = false;
            });
        });
    }

    function adjust(unit, delta)
    {
        console.log(unit, delta);
        const input = document.getElementById(unit);
        let val = parseFloat(input.value || '0') + delta;
        input.value = val;
        currentEditing = true;
        convertFrom(unit, val);
        currentEditing = false;
    }

    function convertFrom(unit, value)
    {
        if (isNaN(value)) return;
        let kelvin;
        switch (unit)
        {
            case 'Kelvin': kelvin = value; break;
            case 'Celsius': kelvin = value + 273.15; break;
            case 'Fahrenheit': kelvin = (value + 459.67) * 5 / 9; break;
            case 'Rankine': kelvin = value * 5 / 9; break;
            case 'Delisle': kelvin = 373.15 - value * 2 / 3; break;
            case 'Newton': kelvin = value * 100 / 33 + 273.15; break;
            case 'Réaumur': kelvin = value * 5 / 4 + 273.15; break;
            case 'Rømer': kelvin = (value - 7.5) * 40 / 21 + 273.15; break;
        }

        document.getElementById('Kelvin').value = round(kelvin);
        document.getElementById('Celsius').value = round(kelvin - 273.15);
        document.getElementById('Fahrenheit').value = round(kelvin * 9 / 5 - 459.67);
        document.getElementById('Rankine').value = round(kelvin * 9 / 5);
        document.getElementById('Delisle').value = round((373.15 - kelvin) * 3 / 2);
        document.getElementById('Newton').value = round((kelvin - 273.15) * 33 / 100);
        document.getElementById('Réaumur').value = round((kelvin - 273.15) * 4 / 5);
        document.getElementById('Rømer').value = round((kelvin - 273.15) * 21 / 40 + 7.5);
    }

    function round(num)
    {
        return Math.round(num * 100) / 100;
    }

    renderRows();
    convertFrom('Kelvin', 0);
</script>";
    }

    public void Stop() { }
}
}
