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
            TextInput.TextInputState textInputState = state.TextInput.HandleInput(input);
            if (textInputState == TextInput.TextInputState.Committed)
            {
                if (string.IsNullOrWhiteSpace(state.TextInput.Text) == false)
                {
                    if (state.TextInputCommitAction == AppState.TextInputActionType.AddNewTask)
                    {
                        _app.TaskService.AddTask(state.TextInput.Text);

                        if (state.SelectedIndex == -1 && _app.TaskService.Count == 1)
                            state.SelectedIndex = 0;
                    }
                    else
                    {
                        if (state.SelectedIndex >= 0 && state.SelectedIndex < _app.TaskService.Count)
                        {
                            TaskItem selectedTask = _app.TaskService.GetTaskByIndex(state.SelectedIndex)!;
                            _app.TaskService.EditTaskDescription(selectedTask.Id, state.TextInput.Text);
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
        else
        {
            if (input.KeyChar == 'q')
            {
                state.ShouldQuit = true;
                return;
            }

            if (input.KeyChar == 'a')
            {
                state.ShowTextInput = true;
                state.TextInputCommitAction = AppState.TextInputActionType.AddNewTask;
                state.TextInput.Reset();
            }
            else if (input.KeyChar == 'f')
            {
                state.FilterMode = state.FilterMode switch
                {
                    TaskService.FilterMode.None => TaskService.FilterMode.Incomplete,
                    TaskService.FilterMode.Incomplete => TaskService.FilterMode.Completed,
                    _ => TaskService.FilterMode.None
                };
                _app.TaskService.Filter(state.FilterMode);

                if (_app.TaskService.Count == 0) state.SelectedIndex = -1;
                else if (state.SelectedIndex >= _app.TaskService.Count) state.SelectedIndex = _app.TaskService.Count - 1;
            }
            else if (input.KeyChar == 's')
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
                _app.TaskService.Sort(state.SortMode);
            }
            else if (_app.TaskService.Count > 0 && state.SelectedIndex >= 0 && state.SelectedIndex < _app.TaskService.Count)
            {
                TaskItem selectedTask = _app.TaskService.GetTaskByIndex(state.SelectedIndex)!;

                if (input.KeyChar == 'e')
                {
                    state.ShowTextInput = true;
                    state.TextInput.Reset(selectedTask.Description);
                    state.TextInputCommitAction = AppState.TextInputActionType.EditSelectedTask;
                }
                else if (input.Key == ConsoleKey.DownArrow || input.KeyChar == 'j')
                {
                    state.SelectedIndex = Math.Min(state.SelectedIndex + 1, _app.TaskService.Count - 1);
                }
                else if (input.Key == ConsoleKey.UpArrow || input.KeyChar == 'k')
                {
                    state.SelectedIndex = Math.Max(state.SelectedIndex - 1, 0);
                }
                else if (input.KeyChar == 'x')
                {
                    _app.TaskService.MarkTaskCompleted(selectedTask.Id, !selectedTask.IsCompleted);
                }
                else if (input.KeyChar == 'd')
                {
                    _app.TaskService.DeleteTask(selectedTask.Id);

                    if (_app.TaskService.Count == 0) state.SelectedIndex = -1;
                    else if (state.SelectedIndex >= _app.TaskService.Count) state.SelectedIndex = _app.TaskService.Count - 1;
                }
            }
        }
    }
}