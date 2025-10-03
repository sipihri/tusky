using Tusky.Services;
using Tusky.Utils;

namespace Tusky;

public class AppState
{
    public enum TextInputActionType
    {
        AddNewTask,
        EditSelectedTask
    }
    
    public bool ShowTextInput;
    public readonly TextInput TextInput = new();
    
    /// <summary>
    /// Action that should be performed when <see cref="TextInput"/> is commited 
    /// </summary>
    public TextInputActionType TextInputCommitAction;

    public TaskService.FilterMode FilterMode;

    public TaskService.SortMode SortMode;

    public int SelectedIndex;
    public bool ShouldQuit;
    
    /// <summary>
    /// Returns true if the text input was committed and the requested action was to add task
    /// </summary>
    public bool IsAddingTask => ShowTextInput && TextInputCommitAction == TextInputActionType.AddNewTask;
}