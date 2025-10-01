using Spectre.Console;
using Tusky;
using Tusky.Data;
using Tusky.Services;

var filePath = "";

if (args.Length > 0)
{
    filePath = args[0];
}

const string cachePath = "./cache";
Config config = Config.Load("./config.json");
Cache cache = Cache.Load(cachePath);

if (string.IsNullOrEmpty(filePath))
{
    bool showProjects = config.ShowProjects;
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
    
    if (showProjects)
    {
        var projectsView = new ProjectsView(config.ProjectsPath);
        AnsiConsole.AlternateScreen(() =>
        {
            try
            {
                projectsView.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        });

        if (projectsView.ExitRequested)
        {
            return;
        }

        filePath = projectsView.SelectedProject;
        cache.LastProjectPath = filePath;
        cache.Save(cachePath);
    }
}

ITaskRepository repository = new FileSystemTaskRepository(filePath);
TaskService service = new(repository);
var app = new App(service);
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
