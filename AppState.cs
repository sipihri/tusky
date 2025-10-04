using Tusky.Services;
using Tusky.Utils;
using Tusky.Views;

namespace Tusky;

public class AppState
{
    public ViewType CurrentView;
    
    public enum TextInputActionType
    {
        AddNewTask,
        EditSelectedTask
    }
    
    public bool ShowTextInput;
    
    /// <summary>
    /// Action that should be performed when <see cref="TextInput"/> is commited 
    /// </summary>
    public TextInputActionType TextInputCommitAction;

    public TaskService.FilterMode FilterMode;

    public TaskService.SortMode SortMode;

    public int SelectedIndex;
    public bool ShouldQuit;
}