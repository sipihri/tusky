using System.Text.Json;

namespace Tusky;

public class Cache
{
    public string LastProjectPath { get; set; } = string.Empty;
    
    public static Cache Load(string path)
    {
        if (File.Exists(path) == false)
        {
            var cache = new Cache();
            cache.Save(path);
            return cache;
        }
        try
        {
            string json = File.ReadAllText(path);
            var cache = JsonSerializer.Deserialize<Cache>(json);

            return cache ?? new Cache();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new Cache();
        }
    }

    public void Save(string path)
    {
        try
        {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}