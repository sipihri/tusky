using Tusky.Models;
using Tusky.Services;
using Tusky.Utils;

namespace Tusky;

public class InputHandler
{
    private readonly App _app;

    public InputHandler(App app)
    {
        _app = app;
    }

    public void HandleInput(AppState state, ConsoleKeyInfo input)
    {
        if (state.ShowTextInput)
        {
            HandleTextInput(state, input);
        }
        else
        {
            bool isTaskSelected = _app.TaskService is { Count: > 0 } && state.SelectedIndex >= 0 &&
                                  state.SelectedIndex < _app.TaskService.Count;
            TaskItem? selectedTask = isTaskSelected ? _app.TaskService!.GetTaskByIndex(state.SelectedIndex)! : null;
            
            switch (input.KeyChar)
            {
                case 'q': QuitApp(state); break;
                case 'a': AddTask(state); break;
                case 'f' when _app.TaskService != null: SwitchFilterMode(state); break;
                case 's' when _app.TaskService != null: SwitchSortMode(state); break;
                case 'e' when isTaskSelected: EditTask(state, selectedTask!); break;
                case 'x' when isTaskSelected: ToggleCompletion(selectedTask!); break;
                case 'd' when isTaskSelected: DeleteTask(state, selectedTask!); break;
                case 'j':
                case '\0' when input.Key == ConsoleKey.DownArrow:
                    SelectNext(state);
                    break;
                case 'k':
                case '\0' when input.Key == ConsoleKey.UpArrow:
                    SelectPrevious(state);
                    break;
            }
        }
    }

    private static void SelectPrevious(AppState state)
    {
        state.SelectedIndex = Math.Max(state.SelectedIndex - 1, 0);
    }

    private void SelectNext(AppState state)
    {
        int itemsCount = _app.TaskService?.Count ?? _app.Projects?.Length ?? 0;
        state.SelectedIndex = Math.Min(state.SelectedIndex + 1, itemsCount - 1);
    }

    private void ToggleCompletion(TaskItem selectedTask)
    {
        _app.TaskService!.MarkTaskCompleted(selectedTask.Id, !selectedTask.IsCompleted);
    }

    private void DeleteTask(AppState state, TaskItem selectedTask)
    {
        _app.TaskService!.DeleteTask(selectedTask.Id);

        if (_app.TaskService.Count == 0) state.SelectedIndex = -1;
        else if (state.SelectedIndex >= _app.TaskService.Count) state.SelectedIndex = _app.TaskService.Count - 1;
    }

    private void EditTask(AppState state, TaskItem selectedTask)
    {
        state.ShowTextInput = true;
        _app.TextInput.Reset(selectedTask.Description);
        state.TextInputCommitAction = AppState.TextInputActionType.EditSelectedTask;
    }

    private void SwitchSortMode(AppState state)
    {
        state.SortMode = state.SortMode switch
        {
            TaskService.SortMode.DescriptionAscending => TaskService.SortMode.DescriptionDescending,
            TaskService.SortMode.DescriptionDescending => TaskService.SortMode.StatusAscending,
            TaskService.SortMode.StatusAscending => TaskService.SortMode.StatusDescending,
            TaskService.SortMode.StatusDescending => TaskService.SortMode.DateAscending,
            TaskService.SortMode.DateAscending => TaskService.SortMode.DateDescending,
            TaskService.SortMode.DateDescending => TaskService.SortMode.None,
            _ => TaskService.SortMode.DescriptionAscending
        };
        _app.TaskService!.Sort(state.SortMode);
    }

    private void SwitchFilterMode(AppState state)
    {
        state.FilterMode = state.FilterMode switch
        {
            TaskService.FilterMode.None => TaskService.FilterMode.Incomplete,
            TaskService.FilterMode.Incomplete => TaskService.FilterMode.Completed,
            _ => TaskService.FilterMode.None
        };
        _app.TaskService!.Filter(state.FilterMode);

        if (_app.TaskService.Count == 0) state.SelectedIndex = -1;
        else if (state.SelectedIndex >= _app.TaskService.Count) state.SelectedIndex = _app.TaskService.Count - 1;
    }

    private void AddTask(AppState state)
    {
        state.ShowTextInput = true;
        state.TextInputCommitAction = AppState.TextInputActionType.AddNewTask;
        _app.TextInput.Reset();
    }

    private void HandleTextInput(AppState state, ConsoleKeyInfo input)
    {
        TextInput.TextInputState textInputState = _app.TextInput.HandleInput(input);
        if (textInputState == TextInput.TextInputState.Committed)
        {
            if (string.IsNullOrWhiteSpace(_app.TextInput.Text) == false)
            {
                if (state.TextInputCommitAction == AppState.TextInputActionType.AddNewTask)
                {
                    _app.TaskService!.AddTask(_app.TextInput.Text);

                    if (state.SelectedIndex == -1 && _app.TaskService.Count == 1)
                        state.SelectedIndex = 0;
                }
                else
                {
                    if (state.SelectedIndex >= 0 && state.SelectedIndex < _app.TaskService!.Count)
                    {
                        TaskItem selectedTask = _app.TaskService.GetTaskByIndex(state.SelectedIndex)!;
                        _app.TaskService.EditTaskDescription(selectedTask.Id, _app.TextInput.Text);
                    }
                }
            }

            state.ShowTextInput = false;
        }
        else if (textInputState == TextInput.TextInputState.Cancelled)
        {
            state.ShowTextInput = false;
        }
    }

    private static void QuitApp(AppState state) => state.ShouldQuit = true;
}