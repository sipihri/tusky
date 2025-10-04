using Spectre.Console;
using Tusky;
using Tusky.Views;

var filePath = "";

if (args.Length > 0)
{
    filePath = args[0];
}

const string cachePath = "./cache";
Config config = Config.Load("./config.json");
Cache cache = Cache.Load(cachePath);

var showProjects = false;

if (string.IsNullOrEmpty(filePath))
{
    showProjects = config.ShowProjects;
    if (showProjects == false)
    {
        string p = cache.LastProjectPath;
        if (string.IsNullOrEmpty(p) == false && p.EndsWith(".json") && File.Exists(p))
        {
            filePath = p;
        }
        else
        {
            showProjects = true;
        }
    }
}

ViewType initialView = showProjects ? ViewType.ProjectsView : ViewType.MainView;
var app = new App(initialView, config, cache, filePath);
// app.Run();
AnsiConsole.AlternateScreen(() =>
{
    try
    {
        app.Run();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
        Console.ReadKey();
    }
});
