using System.Reflection;
using System.Runtime.Loader;
using ToolInterface;

public static class PluginLoader
{
    private static readonly List<ITool> _plugins = new();
    private static readonly Dictionary<string, (AssemblyLoadContext Context, Assembly Assembly)> _loadedAssemblies = new();
    private static FileSystemWatcher? _watcher;
    public static List<ITool> GetPlugins() => _plugins;

    static PluginLoader()
    {
        LoadPlugins();
    }

    public static void LoadPlugins()
    {
        string pluginsPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");

        if (!Directory.Exists(pluginsPath))
            Directory.CreateDirectory(pluginsPath);

        // Load các plugin hiện có
        foreach (string file in Directory.GetFiles(pluginsPath, "*.dll"))
        {
            LoadPlugin(file);
        }

        // Theo dõi thay đổi thư mục Plugins
        _watcher = new FileSystemWatcher(pluginsPath, "*.dll")
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
        };

        // Thêm file đã được xử lý ở Controller & LoadPlugin(byte[], string filePath)
        //_watcher.Created += OnPluginAdded;
        _watcher.Deleted += OnPluginRemoved;
        _watcher.EnableRaisingEvents = true;
    }

    public static ITool? LoadPlugin(string filePath)
    {
        if (!File.Exists(filePath) || !filePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("❌ Invalid plugin file.");
            return null;
        }

        if (_loadedAssemblies.ContainsKey(filePath)) {
            Console.WriteLine($"⚠️ Plugin already loaded: {filePath}");
            return null; 
        }

        try
        {
            var context = new AssemblyLoadContext(filePath, true);

            byte[] fileBytes = File.ReadAllBytes(filePath); // Đọc toàn bộ file vào RAM
            using var stream = new MemoryStream(fileBytes); // Dùng MemoryStream để tránh khóa file
            Assembly assembly = context.LoadFromStream(stream);

            var types = assembly.GetTypes().Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface);
            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is ITool plugin)
                {
                    _plugins.Add(plugin);
                    _loadedAssemblies[filePath] = (context, assembly);
                    Console.WriteLine($"🔌 Plugin loaded: {plugin.Name}");
                    return plugin;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading plugin: {ex.Message}");
        }

        return null;
    }

    public static ITool? LoadPlugin(byte[] fileBytes, string filePath)
    {
        if (_loadedAssemblies.ContainsKey(filePath)) return null;

        try
        {
            var context = new AssemblyLoadContext(filePath, true);
            using var stream = new MemoryStream(fileBytes);
            Assembly assembly = context.LoadFromStream(stream);

            var types = assembly.GetTypes().Where(t => typeof(ITool).IsAssignableFrom(t) && !t.IsInterface);
            foreach (var type in types)
            {
                if (Activator.CreateInstance(type) is ITool plugin)
                {
                    _plugins.Add(plugin);
                    _loadedAssemblies[filePath] = (context, assembly);
                    Console.WriteLine($"🔌 Plugin loaded: {plugin.Name}");
                    return plugin;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading plugin: {ex.Message}");
        }

        return null;
    }


    private static void OnPluginAdded(object sender, FileSystemEventArgs e)
    {
        Thread.Sleep(1000); // Đợi file được release hẳn
        Console.WriteLine($"🆕 Plugin added: {e.Name}");
        LoadPlugin(e.FullPath);
    }

    private static void OnPluginRemoved(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"🗑️ Plugin removed: {e.Name}");

        if (_loadedAssemblies.TryGetValue(e.FullPath, out var pluginData))
        {
            var (context, assembly) = pluginData;

            var pluginsToRemove = _plugins.Where(p => p.GetType().Assembly == assembly).ToList();
            foreach (var plugin in pluginsToRemove)
            {
                if (plugin is ITool tool)
                {
                    tool.Stop(); // 🔥 Dừng plugin trước khi unload
                }

                _plugins.Remove(plugin);
            }

            _loadedAssemblies.Remove(e.FullPath);

            // Dọn sạch tham chiếu
            assembly = null;
            context.Unload();
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine($"✅ Plugin {e.Name} completely unloaded.");
        }
    }

}
