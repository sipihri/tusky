using Tusky.Models;
using Tusky.Services;
using Tusky.Utils;
using Tusky.Views;

namespace Tusky;

public class App
{
    public readonly TaskService TaskService;
    private readonly View _view;

    private enum TextInputActionType
    {
        Add,
        Edit
    }

    public bool ShowTextInput;
    public readonly TextInput TextInput = new();
    private TextInputActionType _textInputActionType;

    public TaskService.FilterMode FilterMode;

    public TaskService.SortMode SortMode;

    public int SelectedIndex;
    private bool _shouldQuit;

    public App(TaskService taskService)
    {
        TaskService = taskService;
        _view = new MainView(this);
    }
    
    /// <summary>
    /// Returns true if the text input was committed and the requested action was to add task
    /// </summary>
    public bool IsAddingTask => ShowTextInput && _textInputActionType == TextInputActionType.Add;

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
                while (_shouldQuit == false)
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

        while (_shouldQuit == false)
        {
            Render();

            ConsoleKeyInfo input = Console.ReadKey(true);
            HandleInput(input);
        }

        Console.CursorVisible = true;
    }

    private void HandleInput(ConsoleKeyInfo input)
    {
        if (ShowTextInput)
        {
            TextInput.TextInputState state = TextInput.HandleInput(input);
            if (state == TextInput.TextInputState.Committed)
            {
                if (string.IsNullOrWhiteSpace(TextInput.Text) == false)
                {
                    if (_textInputActionType == TextInputActionType.Add)
                    {
                        TaskService.AddTask(TextInput.Text);

                        if (SelectedIndex == -1 && TaskService.Count == 1)
                            SelectedIndex = 0;
                    }
                    else
                    {
                        if (SelectedIndex >= 0 && SelectedIndex < TaskService.Count)
                        {
                            TaskItem selectedTask = TaskService.GetTaskByIndex(SelectedIndex)!;
                            TaskService.EditTaskDescription(selectedTask.Id, TextInput.Text);
                        }
                    }
                }

                ShowTextInput = false;
            }
            else if (state == TextInput.TextInputState.Cancelled)
            {
                ShowTextInput = false;
            }
        }
        else
        {
            if (input.KeyChar == 'q')
            {
                _shouldQuit = true;
                return;
            }

            if (input.KeyChar == 'a')
            {
                ShowTextInput = true;
                _textInputActionType = TextInputActionType.Add;
                TextInput.Reset();
            }
            else if (input.KeyChar == 'f')
            {
                FilterMode = FilterMode switch
                {
                    TaskService.FilterMode.None => TaskService.FilterMode.Incomplete,
                    TaskService.FilterMode.Incomplete => TaskService.FilterMode.Completed,
                    _ => TaskService.FilterMode.None
                };
                TaskService.Filter(FilterMode);

                if (TaskService.Count == 0) SelectedIndex = -1;
                else if (SelectedIndex >= TaskService.Count) SelectedIndex = TaskService.Count - 1;
            }
            else if (input.KeyChar == 's')
            {
                SortMode = SortMode switch
                {
                    TaskService.SortMode.DescriptionAscending => TaskService.SortMode.DescriptionDescending,
                    TaskService.SortMode.DescriptionDescending => TaskService.SortMode.StatusAscending,
                    TaskService.SortMode.StatusAscending => TaskService.SortMode.StatusDescending,
                    TaskService.SortMode.StatusDescending => TaskService.SortMode.DateAscending,
                    TaskService.SortMode.DateAscending => TaskService.SortMode.DateDescending,
                    TaskService.SortMode.DateDescending => TaskService.SortMode.None,
                    _ => TaskService.SortMode.DescriptionAscending
                };
                TaskService.Sort(SortMode);
            }
            else if (TaskService.Count > 0 && SelectedIndex >= 0 && SelectedIndex < TaskService.Count)
            {
                TaskItem selectedTask = TaskService.GetTaskByIndex(SelectedIndex)!;

                if (input.KeyChar == 'e')
                {
                    ShowTextInput = true;
                    TextInput.Reset(selectedTask.Description);
                    _textInputActionType = TextInputActionType.Edit;
                }
                else if (input.Key == ConsoleKey.DownArrow || input.KeyChar == 'j')
                {
                    SelectedIndex = Math.Min(SelectedIndex + 1, TaskService.Count - 1);
                }
                else if (input.Key == ConsoleKey.UpArrow || input.KeyChar == 'k')
                {
                    SelectedIndex = Math.Max(SelectedIndex - 1, 0);
                }
                else if (input.KeyChar == 'x')
                {
                    TaskService.MarkTaskCompleted(selectedTask.Id, !selectedTask.IsCompleted);
                }
                else if (input.KeyChar == 'd')
                {
                    TaskService.DeleteTask(selectedTask.Id);

                    if (TaskService.Count == 0) SelectedIndex = -1;
                    else if (SelectedIndex >= TaskService.Count) SelectedIndex = TaskService.Count - 1;
                }
            }
        }
    }

    private void Render()
    {
        Console.SetCursorPosition(0, 0);
        _view.Draw();
    }
}