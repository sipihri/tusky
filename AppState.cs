using Tusky.Services;
using Tusky.Utils;

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