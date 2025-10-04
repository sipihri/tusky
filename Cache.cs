using System.Text.Json;

namespace Tusky;

public class Cache
{
    public Cache() : this(string.Empty) { }
    
    private Cache(string savePath)
    {
        SavePath = savePath;
    }
    
    private string SavePath { get; }

    public string LastProjectPath { get; set; } = string.Empty;
    
    public static Cache Load(string path)
    {
        if (File.Exists(path) == false)
        {
            var cache = new Cache(path);
            cache.Save();
            return cache;
        }
        try
        {
            string json = File.ReadAllText(path);
            var cache = JsonSerializer.Deserialize<Cache>(json);

            return cache ?? new Cache(path);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new Cache(path);
        }
    }

    public void Save()
    {
        if (string.IsNullOrEmpty(SavePath))
            return;
        
        try
        {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}