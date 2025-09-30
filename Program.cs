using Spectre.Console;
using Tusky;
using Tusky.Data;
using Tusky.Services;

var filePath = "";

if (args.Length > 0)
{
    filePath = args[0];
}

if (string.IsNullOrEmpty(filePath))
{
    var projectsView = new ProjectsView("./");
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
