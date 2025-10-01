using Spectre.Console;
using Tusky.Utils;

namespace Tusky;

public class ProjectsView
{
    private readonly string[] _projects;

    private int _selectedIndex;
    private bool _shouldQuit;

    public ProjectsView(string path)
    {
        _projects = Directory.GetFiles(path, "*.json");
    }
    
    public bool ExitRequested { get; private set; }
    public string SelectedProject => _projects[_selectedIndex];

    /// <summary>
    /// Starts a background thread and checks for console window resize
    /// every 100ms. If window is resized, it will re-render the app
    /// </summary>
    private void CheckWindowResize()
    {
        int lastW = Console.WindowWidth;
        int lastH = Console.WindowHeight;

        var resizeThread = new Thread(() =>
            {
                while (_shouldQuit == false && ExitRequested == false)
                {
                    if (Console.WindowWidth != lastW || Console.WindowHeight != lastH)
                    {
                        lastW = Console.WindowWidth;
                        lastH = Console.WindowHeight;
                        Console.Clear();
                        Render();
                    }

                    Thread.Sleep(100);
                }
            })
            { IsBackground = true };

        resizeThread.Start();
    }

    public void Run()
    {
        Console.CursorVisible = false;

        CheckWindowResize();

        while (_shouldQuit == false && ExitRequested == false)
        {
            Render();

            ConsoleKeyInfo input = Console.ReadKey(true);
            HandleInput(input);
        }

        Console.CursorVisible = true;
    }

    private void HandleInput(ConsoleKeyInfo input)
    {
        if (input.Key == ConsoleKey.Escape || input.KeyChar == 'q')
        {
            _shouldQuit = true;
            ExitRequested = true;
            return;
        }

        if (input.Key == ConsoleKey.Enter)
        {
            _shouldQuit = true;
            return;
        }

        if (input.Key == ConsoleKey.DownArrow || input.KeyChar == 'j')
        {
            _selectedIndex = Math.Min(_selectedIndex + 1, _projects.Length - 1);
        }
        else if (input.Key == ConsoleKey.UpArrow || input.KeyChar == 'k')
        {
            _selectedIndex = Math.Max(_selectedIndex - 1, 0);
        }
    }

    private void Render()
    {
        Console.SetCursorPosition(0, 0);

        for (var i = 0; i < _projects.Length; i++)
        {
            string project = _projects[i];
            project = project.PadLeft(project.Length + 4).PadRight(Console.BufferWidth);
            project = project.WrapInStyleIf("on grey15", i == _selectedIndex);
            AnsiConsole.Markup(project);
        }
    }
}