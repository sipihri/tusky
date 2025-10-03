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
    private readonly AppState _state;

    public App(TaskService taskService)
    {
        TaskService = taskService;
        _state = new AppState();
        _view = new MainView(this);
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
        if (_state.ShowTextInput)
        {
            TextInput.TextInputState state = _state.TextInput.HandleInput(input);
            if (state == TextInput.TextInputState.Committed)
            {
                if (string.IsNullOrWhiteSpace(_state.TextInput.Text) == false)
                {
                    if (_state.CurrentTextInputActionType == AppState.TextInputActionType.Add)
                    {
                        TaskService.AddTask(_state.TextInput.Text);

                        if (_state.SelectedIndex == -1 && TaskService.Count == 1)
                            _state.SelectedIndex = 0;
                    }
                    else
                    {
                        if (_state.SelectedIndex >= 0 && _state.SelectedIndex < TaskService.Count)
                        {
                            TaskItem selectedTask = TaskService.GetTaskByIndex(_state.SelectedIndex)!;
                            TaskService.EditTaskDescription(selectedTask.Id, _state.TextInput.Text);
                        }
                    }
                }

                _state.ShowTextInput = false;
            }
            else if (state == TextInput.TextInputState.Cancelled)
            {
                _state.ShowTextInput = false;
            }
        }
        else
        {
            if (input.KeyChar == 'q')
            {
                _state.ShouldQuit = true;
                return;
            }

            if (input.KeyChar == 'a')
            {
                _state.ShowTextInput = true;
                _state.CurrentTextInputActionType = AppState.TextInputActionType.Add;
                _state.TextInput.Reset();
            }
            else if (input.KeyChar == 'f')
            {
                _state.FilterMode = _state.FilterMode switch
                {
                    TaskService.FilterMode.None => TaskService.FilterMode.Incomplete,
                    TaskService.FilterMode.Incomplete => TaskService.FilterMode.Completed,
                    _ => TaskService.FilterMode.None
                };
                TaskService.Filter(_state.FilterMode);

                if (TaskService.Count == 0) _state.SelectedIndex = -1;
                else if (_state.SelectedIndex >= TaskService.Count) _state.SelectedIndex = TaskService.Count - 1;
            }
            else if (input.KeyChar == 's')
            {
                _state.SortMode = _state.SortMode switch
                {
                    TaskService.SortMode.DescriptionAscending => TaskService.SortMode.DescriptionDescending,
                    TaskService.SortMode.DescriptionDescending => TaskService.SortMode.StatusAscending,
                    TaskService.SortMode.StatusAscending => TaskService.SortMode.StatusDescending,
                    TaskService.SortMode.StatusDescending => TaskService.SortMode.DateAscending,
                    TaskService.SortMode.DateAscending => TaskService.SortMode.DateDescending,
                    TaskService.SortMode.DateDescending => TaskService.SortMode.None,
                    _ => TaskService.SortMode.DescriptionAscending
                };
                TaskService.Sort(_state.SortMode);
            }
            else if (TaskService.Count > 0 && _state.SelectedIndex >= 0 && _state.SelectedIndex < TaskService.Count)
            {
                TaskItem selectedTask = TaskService.GetTaskByIndex(_state.SelectedIndex)!;

                if (input.KeyChar == 'e')
                {
                    _state.ShowTextInput = true;
                    _state.TextInput.Reset(selectedTask.Description);
                    _state.CurrentTextInputActionType = AppState.TextInputActionType.Edit;
                }
                else if (input.Key == ConsoleKey.DownArrow || input.KeyChar == 'j')
                {
                    _state.SelectedIndex = Math.Min(_state.SelectedIndex + 1, TaskService.Count - 1);
                }
                else if (input.Key == ConsoleKey.UpArrow || input.KeyChar == 'k')
                {
                    _state.SelectedIndex = Math.Max(_state.SelectedIndex - 1, 0);
                }
                else if (input.KeyChar == 'x')
                {
                    TaskService.MarkTaskCompleted(selectedTask.Id, !selectedTask.IsCompleted);
                }
                else if (input.KeyChar == 'd')
                {
                    TaskService.DeleteTask(selectedTask.Id);

                    if (TaskService.Count == 0) _state.SelectedIndex = -1;
                    else if (_state.SelectedIndex >= TaskService.Count) _state.SelectedIndex = TaskService.Count - 1;
                }
            }
        }
    }

    private void Render()
    {
        Console.SetCursorPosition(0, 0);
        _view.Draw(_state);
    }
}