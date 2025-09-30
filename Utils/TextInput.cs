namespace Tusky.Utils;

public class TextInput
{
    public enum TextInputState
    {
        Editing,
        Committed,
        Cancelled
    }

    private int _cursorPosition;
    
    public string Text { get; private set; } = string.Empty;

    public TextInputState HandleInput(ConsoleKeyInfo input)
    {
        bool ctrl = (input.Modifiers & ConsoleModifiers.Control) != 0;
        
        switch (input.Key)
        {
            case ConsoleKey.LeftArrow:
                _cursorPosition = ctrl
                    ? GetPreviousWordPosition()
                    : Math.Max(0, _cursorPosition - 1);
                break;
                
            case ConsoleKey.RightArrow:
                _cursorPosition = ctrl
                    ? GetNextWordPosition()
                    : Math.Min(Text.Length, _cursorPosition + 1);
                break;
                
            case ConsoleKey.Backspace:
                if (ctrl)
                {
                    int prevWordPos = GetPreviousWordPosition();
                    if (prevWordPos < _cursorPosition)
                    {
                        Text = Text.Remove(prevWordPos, _cursorPosition - prevWordPos);
                        _cursorPosition = prevWordPos;
                    }
                }
                else if (_cursorPosition > 0)
                {
                    Text = Text.Remove(_cursorPosition - 1, 1);
                    _cursorPosition--;
                }
                break;
                
            case ConsoleKey.Delete:
                if (ctrl)
                {
                    int nextWordPos = GetNextWordPosition();
                    if (nextWordPos > _cursorPosition)
                    {
                        Text = Text.Remove(_cursorPosition, nextWordPos - _cursorPosition);
                    }
                }
                else if (_cursorPosition < Text.Length)
                {
                    Text = Text.Remove(_cursorPosition, 1);
                }
                break;
                
            case ConsoleKey.Home:
                _cursorPosition = 0;
                break;
                
            case ConsoleKey.End:
                _cursorPosition = Text.Length;
                break;
            
            case ConsoleKey.Enter: return TextInputState.Committed;
            case ConsoleKey.Escape: return TextInputState.Cancelled;
                
            default:
                if (char.IsControl(input.KeyChar) == false)
                {
                    Text = Text.Insert(_cursorPosition, input.KeyChar.ToString());
                    _cursorPosition++;
                }
                break;
        }
        return TextInputState.Editing;
    }

    public void Reset(string initial = "")
    {
        Text = initial;
        _cursorPosition = Text.Length;
    }

    public string Render(string label, string labelStyle = "black on #85b5ba", string cursorStyle = "black on white")
    {
        string before = Text[.._cursorPosition];
        string cursorChar = _cursorPosition < Text.Length ? Text[_cursorPosition].ToString() : " ";
        string after = _cursorPosition < Text.Length ? Text[(_cursorPosition + 1)..] : "";
        
        return $"[{labelStyle}] {label} [/]: {before}[{cursorStyle}]{cursorChar}[/]{after}";
    }

    private int GetPreviousWordPosition()
    {
        if (_cursorPosition <= 0) return 0;
        
        int pos = _cursorPosition - 1;
        
        while (pos >= 0 && char.IsWhiteSpace(Text[pos])) pos--;
        while (pos >= 0 && char.IsWhiteSpace(Text[pos]) == false) pos--;
        
        return Math.Max(0, pos + 1);
    }
    
    private int GetNextWordPosition()
    {
        if (_cursorPosition >= Text.Length) return Text.Length;
        
        int pos = _cursorPosition;
        
        while (pos < Text.Length && char.IsWhiteSpace(Text[pos]) == false) pos++;
        while (pos < Text.Length && char.IsWhiteSpace(Text[pos])) pos++;
        
        return Math.Min(Text.Length, pos);
    }
}