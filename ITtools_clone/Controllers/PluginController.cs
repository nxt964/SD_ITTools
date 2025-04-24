using ITtools_clone.Helpers;
using Microsoft.AspNetCore.Mvc;
using ITtools_clone.Models;
using ITtools_clone.Services;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

public class PluginController : Controller
{
    private readonly string pluginPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
    private readonly IPluginService _pluginService;

    public PluginController(IPluginService pluginService)
    {
        _pluginService = pluginService;
    }

    [HttpGet]
    public IActionResult AddTool()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadTool(IFormFile file)
    {
        var result = await _pluginService.AddPluginFromFile(file);
        
        if (result.Success)
        {
            ViewBag.Success = result.Message;
            ViewBag.Error = null;
        }
        else
        {
            ViewBag.Error = result.Message;
            ViewBag.Success = null;
        }
        
        return View("AddTool");
    }


    [Route("{pluginSlugName}")]
    public IActionResult LoadTool(string pluginSlugName)
    {
        var plugin = _pluginService.GetPluginBySlugName(pluginSlugName, checkEnabled: !(HttpContext.Session.GetInt32("isAdmin") == 1));
        if (plugin == null)
        {
            return NotFound("Plugin not found.");
        }

        ViewBag.PluginName = plugin.Name;
        ViewBag.PluginUI = plugin.GetUI();
        return View();
    }

    [HttpPost]
    [Route("{pluginSlugName}/execute")]
    public IActionResult ExecuteTool(string pluginSlugName, [FromBody] object inputData)
    {
        var plugin = _pluginService.GetPluginBySlugName(pluginSlugName, checkEnabled: !(HttpContext.Session.GetInt32("isAdmin") == 1));
        if (plugin == null)
        {
            return NotFound("Plugin not found.");
        }
        
        object result = plugin.Execute(inputData);

        return Json(new { success = true, result });
    }
}
