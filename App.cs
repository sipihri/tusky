using Tusky.Services;
using Tusky.Utils;
using Tusky.Views;

namespace Tusky;

public class App
{
    public readonly TaskService? TaskService;
    public readonly string[]? Projects;
    public readonly TextInput TextInput;
    
    private readonly View _view;
    private readonly InputHandler _inputHandler;
    private readonly AppState _state;

    public App(TaskService? taskService, string[]? projects)
    {
        TaskService = taskService;
        Projects = projects;
        TextInput = new TextInput();
        
        _state = new AppState();
        _view = new MainView(this);
        _inputHandler = new InputHandler(this);
    }

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
                while (_state.ShouldQuit == false)
                {
                    if (Console.WindowWidth != lastW || Console.WindowHeight != lastH)
                    {
                        lastW = Console.WindowWidth;
                        lastH = Console.WindowHeight;
                        Console.Clear();
                        Draw();
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

        while (_state.ShouldQuit == false)
        {
            Draw();

            ConsoleKeyInfo input = Console.ReadKey(true);
            _inputHandler.HandleInput(_state, input);
        }

        Console.CursorVisible = true;
    }
    
    private void Draw()
    {
        Console.SetCursorPosition(0, 0);
        _view.Draw(_state);
    }
}