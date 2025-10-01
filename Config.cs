using System.Text.Json;

namespace Tusky;

public class Config
{
    /// <summary>
    /// The directory path where new projects will be created and existing projects will be loaded from
    /// </summary>
    public string ProjectsPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Tusky\";

    /// <summary>
    /// If enabled, the projects selection view (<see cref="ProjectsView"/>) will be launched
    /// when no project path is passed to the app. If disabled, the last project will be opened
    /// </summary>
    public bool ShowProjects { get; set; } = true;
    
    public static Config Load(string path)
    {
        if (File.Exists(path) == false)
        {
            Config config = GetDefaultConfig();
            config.Save(path);
            return config;
        }
        try
        {
            string json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<Config>(json);

            return config ?? GetDefaultConfig();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return GetDefaultConfig();
        }
    }

    private static Config GetDefaultConfig()
    {
        var config = new Config();
        Directory.CreateDirectory(config.ProjectsPath); // Ensure projects path exists
        return config;
    }

    private void Save(string path)
    {
        try
        {
            string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}