using Spectre.Console;
using Tusky;
using Tusky.Data;
using Tusky.Services;

var filePath = "tasks.json";

if (args.Length > 0)
{
    filePath = args[0];
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
