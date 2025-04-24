// Interface
using ITtools_clone.Repositories;
using ITtools_clone.Services;
using ITtools_clone.Models;
using ITtools_clone.Helpers;
using ToolInterface;

public interface IPluginService
{
    Task<(bool Success, string Message, Tool Tool)> AddPluginFromFile(IFormFile file);
    ITool? GetPluginBySlugName(string pluginSlugName, bool checkEnabled);

    bool DeletePluginFile(string fileName);
}

// Implementation
public class PluginService : IPluginService
{
    private readonly IPluginRepository _pluginRepository;
    private readonly IToolService _toolService;
    
    public PluginService(IPluginRepository pluginRepository, IToolService toolService)
    {
        _pluginRepository = pluginRepository;
        _toolService = toolService;
    }
    
    public async Task<(bool Success, string Message, Tool Tool)> AddPluginFromFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return (false, "Please select a DLL file to upload.", null);
        }

        if (!file.FileName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Only .dll files are allowed.", null);
        }

        _pluginRepository.EnsurePluginDirectoryExists();
        string filePath = _pluginRepository.GetPluginPath(file.FileName);
        byte[] fileBytes;

        try
        {
            // Read file into memory
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            // Load plugin from memory to validate
            var plugin = PluginLoader.LoadPlugin(fileBytes, filePath);
            if (plugin == null)
            {
                return (false, "Failed to load plugin. Please check file content.", null);
            }

            // Save file after successful validation
            bool fileSaved = await _pluginRepository.SavePluginFile(file.FileName, fileBytes);
            if (!fileSaved)
            {
                return (false, "Failed to save plugin file.", null);
            }

            // Create tool record
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
            
            return (true, "Tool uploaded successfully!", tool);
        }
        catch (Exception ex)
        {
            // Cleanup if file was already written
            
            return (false, $"Upload failed: {ex.Message}", null);
        }
    }
    
    public ITool? GetPluginBySlugName(string pluginSlugName, bool checkEnabled)
    {
        var tool = _toolService.GetToolByName(Utils.Unslugify(pluginSlugName));
        if (tool == null || (!tool.enabled && checkEnabled))
        {
            return null;
        }
        return PluginLoader.GetPlugins().FirstOrDefault(p => 
            Utils.Slugify(p.Name) == pluginSlugName);
    }

    public bool DeletePluginFile(string fileName)
    {
        return _pluginRepository.DeletePluginFile(fileName);
    }
}