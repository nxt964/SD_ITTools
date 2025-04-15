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
    private readonly IToolService _toolService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PluginController(IToolService toolService, IHttpContextAccessor httpContextAccessor)
    {
        _toolService = toolService;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    public IActionResult AddTool()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadTool(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ViewBag.Error = "Please select a DLL file to upload.";
            return View("AddTool");
        }

        if (!file.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            ViewBag.Error = "Only .dll files are allowed.";
            return View("AddTool");
        }

        if (!Directory.Exists(pluginPath))
        {
            Directory.CreateDirectory(pluginPath);
        }

        string filePath = Path.Combine(pluginPath, file.FileName);
        byte[] fileBytes;

        try
        {
            // Đọc file vào RAM trước
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // Load plugin từ stream để kiểm tra hợp lệ
            var plugin = PluginLoader.LoadPlugin(fileBytes, filePath); // cần overload LoadPlugin thêm
            if (plugin == null)
            {
                ViewBag.Error = "Failed to load plugin. Please check file content.";
                return View("AddTool");
            }

            // Ghi file thực sự sau khi load thành công
            await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

            var tool = new Tool
            {
                tool_name = plugin.Name,
                description = plugin.Description,
                enabled = true,
                premium_required = false,
                category_name = plugin.Category,
                file_name = file.FileName
            };

            _toolService.AddTool(tool);
            ViewBag.Error = null;
            ViewBag.Success = "Tool uploaded successfully!";
        }
        catch (Exception ex)
        {
            ViewBag.Error = "Upload failed: " + ex.Message;

            // Cleanup nếu file đã ghi (an toàn trong mọi trường hợp)
            try
            {
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        return View("AddTool");
    }


    [Route("{pluginSlugName}")]
    public IActionResult LoadTool(string pluginSlugName)
    {
        var plugin = PluginLoader.GetPlugins().FirstOrDefault(p => Utils.Slugify(p.Name) == pluginSlugName);
        if (plugin == null) return NotFound("Plugin not found");

        ViewBag.PluginName = plugin.Name;
        ViewBag.PluginUI = plugin.GetUI();
        return View();
    }

    [HttpPost]
    [Route("{pluginSlugName}/execute")]
    public IActionResult ExecuteTool(string pluginSlugName, [FromBody] object inputData)
    {
        var plugin = PluginLoader.GetPlugins().FirstOrDefault(p => Utils.Slugify(p.Name) == pluginSlugName);
        if (plugin == null) return NotFound("Plugin not found");

        // Thực thi công cụ và lấy kết quả
        object result = plugin.Execute(inputData);

        return Json(new { success = true, result });
    }
}
