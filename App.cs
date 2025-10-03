using Tusky.Models;
using Tusky.Services;
using Tusky.Utils;
using Tusky.Views;

namespace Tusky;

public class AppState
{
    public enum TextInputActionType
    {
        Add,
        Edit
    }
    
    public bool ShowTextInput;
    public readonly TextInput TextInput = new();
    public TextInputActionType CurrentTextInputActionType;

    public TaskService.FilterMode FilterMode;

    public TaskService.SortMode SortMode;

    public int SelectedIndex;
    public bool ShouldQuit;
    
    /// <summary>
    /// Returns true if the text input was committed and the requested action was to add task
    /// </summary>
    public bool IsAddingTask => ShowTextInput && CurrentTextInputActionType == TextInputActionType.Add;
}

public class App
{
    public readonly TaskService TaskService;
    private readonly View _view;
    private readonly InputHandler _inputHandler;
    private readonly AppState _state;

    public App(TaskService taskService)
    {
        TaskService = taskService;
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

        while (_state.ShouldQuit == false)
        {
            Render();

            ConsoleKeyInfo input = Console.ReadKey(true);
            HandleInput(input);
        }

        Console.CursorVisible = true;
    }

    private void HandleInput(ConsoleKeyInfo input)
    {
        _inputHandler.HandleInput(_state, input);
    }

    private void Render()
    {
        Console.SetCursorPosition(0, 0);
        _view.Draw(_state);
    }
}