using Tusky.Data;
using Tusky.Services;
using Tusky.Utils;
using Tusky.Views;

namespace Tusky;

public class App
{
    public readonly TextInput TextInput;

    private readonly InputHandler _inputHandler;
    private readonly AppState _state;

    private View _view;
    private ViewType _lastView;
    private readonly Config _config;
    private readonly Cache _cache;

    public App(ViewType initialView, Config config, Cache cache, string lastProjectPath)
    {
        _config = config;
        _cache = cache;
        
        TaskService = GetTaskService(lastProjectPath);
        Projects = GetProjects(config.ProjectsPath);
        TextInput = new TextInput();

        _state = new AppState
        {
            CurrentView = initialView
        };
        _lastView = initialView;

        _view = CreateView(_state.CurrentView);
        _inputHandler = new InputHandler(this);
    }
    
    public TaskService TaskService { get; private set; }
    public string[] Projects { get; private set; }

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

            if (_state.CurrentView != _lastView)
            {
                _view = CreateView(_state.CurrentView);
                if (_lastView == ViewType.ProjectsView && _state.CurrentView == ViewType.MainView)
                {
                    // Switch to MainView from ProjectsView
                    string projectPath = Projects[_state.SelectedIndex];
                    TaskService = GetTaskService(projectPath);
                    _cache.LastProjectPath = projectPath;
                    _cache.Save();
                }
                else
                {
                    Projects = GetProjects(_config.ProjectsPath);
                }

                _lastView = _state.CurrentView;
            }
        }

        Console.CursorVisible = true;
    }

    private void Draw()
    {
        Console.SetCursorPosition(0, 0);
        _view.Draw(_state);
    }

    private View CreateView(ViewType viewType) => viewType switch
    {
        ViewType.MainView => new MainView(this),
        _ => new Views.ProjectsView(this)
    };
    
    private static TaskService GetTaskService(string projectPath)
    {
        ITaskRepository repository = new FileSystemTaskRepository(projectPath);
        TaskService service = new(repository);
        return service;
    }

    private static string[] GetProjects(string projectsPath)
    {
        return Directory.GetFiles(projectsPath, "*.json");
    }
}