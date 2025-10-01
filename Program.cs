using Spectre.Console;
using Tusky;
using Tusky.Data;
using Tusky.Services;

var filePath = "";

if (args.Length > 0)
{
    filePath = args[0];
}

Config config = Config.Load("./config.json");

if (string.IsNullOrEmpty(filePath))
{
    if (config.ShowProjects)
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
        else
        {
            filePath = projectsView.SelectedProject;
        }
    }
    else
    {
        // Todo: get the last opened project and set filePath to the path of that project 
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
