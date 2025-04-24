

// Interface
public interface IPluginRepository
{
    Task<bool> SavePluginFile(string fileName, byte[] fileBytes);
    bool EnsurePluginDirectoryExists();
    string GetPluginPath(string fileName);
    bool DeletePluginFile(string fileName);
    // Other file operations would go here
}

// Implementation
public class PluginRepository : IPluginRepository
{
    private readonly string _pluginsPath;
    
    public PluginRepository()
    {
        _pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
    }
    
    public bool EnsurePluginDirectoryExists()
    {
        if (!Directory.Exists(_pluginsPath))
        {
            Directory.CreateDirectory(_pluginsPath);
        }
        return true;
    }

    public string GetPluginPath(string fileName)
    {
        return Path.Combine(_pluginsPath, fileName);
    }
    
    public async Task<bool> SavePluginFile(string fileName, byte[] fileBytes)
    {
        try
        {
            string filePath = GetPluginPath(fileName);
            await File.WriteAllBytesAsync(filePath, fileBytes);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving plugin: {ex.Message}");
            return false;
        }
    }

    public bool DeletePluginFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(_pluginsPath, fileName);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"Deleted plugin file: {filePath}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
            return false;
        }
    }
}