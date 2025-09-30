using Spectre.Console;
using Spectre.Console.Rendering;
using Tusky.Models;
using Tusky.Services;
using Tusky.Utils;

namespace Tusky;

public class App
{
    private readonly TaskService _taskService;

    private readonly IRenderable _headerRenderable;
    private readonly IRenderable _footerHintRenderable;
    private readonly List<IRenderable> _taskRenderables = [];
    private readonly IRenderable _noTasksRenderable;
    private readonly IRenderable[] _footerRenderables;
    private readonly Text _emptyTextRenderable;

    private readonly string[] _primaryColors =
    [
        "red",
        "fuchsia",
        "aqua",
        "white",
        "green",
        "blue",
        "purple"
    ];

    private readonly string _selectedPrimaryColor;

    private const string DimStyle = "grey";
    private const string CompletedDescriptionStyle = "grey strikethrough italic";
    private const string InCompleteDescriptionStyle = "white";
    private const string MetaStyle = "grey27";
    private const string SelectedRowStyle = "on grey15";
    private const string IncompleteIconStyle = "grey";

    private const string CompletedIcon = "󰄲";
    private const string IncompleteIcon = "󰄱";

    private enum TextInputActionType
    {
        Add,
        Edit
    }

    private bool _showTextInput;
    private readonly TextInput _textInput = new();
    private TextInputActionType _textInputActionType;

    private TaskService.FilterMode _filterMode;

    private TaskService.SortMode _sortMode;

    private int _selectedIndex;
    private bool _shouldQuit;

    public App(TaskService taskService)
    {
        _taskService = taskService;

        int r = Random.Shared.Next(_primaryColors.Length);
        _selectedPrimaryColor = _primaryColors[r];

        _headerRenderable = GetHeaderRenderable();
        _footerHintRenderable = GetFooterRenderable();
        _noTasksRenderable = new Markup("No tasks yet. Press 'a' to add one.".WrapInStyle(DimStyle));
        _footerRenderables = new IRenderable[2];
        _emptyTextRenderable = new Text(string.Empty);
    }

    private IRenderable GetHeaderRenderable()
    {
        var title = $"[bold black on {_selectedPrimaryColor}]     Tusky     [/]";
        title += $"[{_selectedPrimaryColor}][/]";
        return new Align(new Markup(title), HorizontalAlignment.Left);
    }

    private IRenderable GetFooterRenderable()
    {
        string str = AppendBinding(string.Empty, "↑↓", "navigate");
        str += AppendBinding(str, "x", "toggle");
        str += AppendBinding(str, "a", "add");
        str += AppendBinding(str, "e", "edit");
        str += AppendBinding(str, "d", "delete");
        str += AppendBinding(str, "q", "quit");

        return new Markup(str.WrapInStyle(DimStyle));

        string AppendBinding(string current, string key, string action)
        {
            var binding = $"{key.WrapInStyle($"bold {_selectedPrimaryColor}")} {action}";
            if (string.IsNullOrEmpty(current))
                return binding;

            return " • " + binding;
        }
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
        if (_showTextInput)
        {
            TextInput.TextInputState state = _textInput.HandleInput(input);
            if (state == TextInput.TextInputState.Committed)
            {
                if (string.IsNullOrWhiteSpace(_textInput.Text) == false)
                {
                    if (_textInputActionType == TextInputActionType.Add)
                    {
                        _taskService.AddTask(_textInput.Text);

                        if (_selectedIndex == -1 && _taskService.Count == 1)
                            _selectedIndex = 0;
                    }
                    else
                    {
                        if (_selectedIndex >= 0 && _selectedIndex < _taskService.Count)
                        {
                            TaskItem selectedTask = _taskService.GetTaskByIndex(_selectedIndex)!;
                            _taskService.EditTaskDescription(selectedTask.Id, _textInput.Text);
                        }
                    }
                }

                _showTextInput = false;
            }
            else if (state == TextInput.TextInputState.Cancelled)
            {
                _showTextInput = false;
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
                _showTextInput = true;
                _textInputActionType = TextInputActionType.Add;
                _textInput.Reset();
            }
            else if (input.KeyChar == 'f')
            {
                _filterMode = _filterMode switch
                {
                    TaskService.FilterMode.None => TaskService.FilterMode.Incomplete,
                    TaskService.FilterMode.Incomplete => TaskService.FilterMode.Completed,
                    _ => TaskService.FilterMode.None
                };
                _taskService.Filter(_filterMode);

                if (_taskService.Count == 0) _selectedIndex = -1;
                else if (_selectedIndex >= _taskService.Count) _selectedIndex = _taskService.Count - 1;
            }
            else if (input.KeyChar == 's')
            {
                _sortMode = _sortMode switch
                {
                    TaskService.SortMode.DescriptionAscending => TaskService.SortMode.DescriptionDescending,
                    TaskService.SortMode.DescriptionDescending => TaskService.SortMode.StatusAscending,
                    TaskService.SortMode.StatusAscending => TaskService.SortMode.StatusDescending,
                    TaskService.SortMode.StatusDescending => TaskService.SortMode.DateAscending,
                    TaskService.SortMode.DateAscending => TaskService.SortMode.DateDescending,
                    TaskService.SortMode.DateDescending => TaskService.SortMode.None,
                    _ => TaskService.SortMode.DescriptionAscending
                };
                _taskService.Sort(_sortMode);
            }
            else if (_taskService.Count > 0 && _selectedIndex >= 0 && _selectedIndex < _taskService.Count)
            {
                TaskItem selectedTask = _taskService.GetTaskByIndex(_selectedIndex)!;

                if (input.KeyChar == 'e')
                {
                    _showTextInput = true;
                    _textInput.Reset(selectedTask.Description);
                    _textInputActionType = TextInputActionType.Edit;
                }
                else if (input.Key == ConsoleKey.DownArrow || input.KeyChar == 'j')
                {
                    _selectedIndex = Math.Min(_selectedIndex + 1, _taskService.Count - 1);
                }
                else if (input.Key == ConsoleKey.UpArrow || input.KeyChar == 'k')
                {
                    _selectedIndex = Math.Max(_selectedIndex - 1, 0);
                }
                else if (input.KeyChar == 'x')
                {
                    _taskService.MarkTaskCompleted(selectedTask.Id, !selectedTask.IsCompleted);
                }
                else if (input.KeyChar == 'd')
                {
                    _taskService.DeleteTask(selectedTask.Id);

                    if (_taskService.Count == 0) _selectedIndex = -1;
                    else if (_selectedIndex >= _taskService.Count) _selectedIndex = _taskService.Count - 1;
                }
            }
        }
    }

    private void Render()
    {
        Console.SetCursorPosition(0, 0);

        const int headerSize = 2;
        int footerSize = _showTextInput ? 2 : 1;

        string filterIcon = _filterMode switch
        {
            TaskService.FilterMode.Completed => "󰪥",
            TaskService.FilterMode.Incomplete => "󰄰",
            _ => "󰪡"
        };

        string sortIcon = _sortMode switch
        {
            TaskService.SortMode.DescriptionAscending => "󰖽",
            TaskService.SortMode.StatusAscending => "󱎅",
            TaskService.SortMode.DateAscending => "󱕇",
            TaskService.SortMode.DescriptionDescending => "󰖿",
            TaskService.SortMode.StatusDescending => "󱎇",
            TaskService.SortMode.DateDescending => "󱕈",
            _ => "󰒺"
        };

        var headerRows = new IRenderable[headerSize];
        headerRows[0] = _headerRenderable;
        headerRows[1] = new Align(new Markup($"{sortIcon}  {filterIcon}    ".WrapInStyle(DimStyle)),
            HorizontalAlignment.Right);

        Layout layout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(headerSize).Update(new Rows(headerRows)),
                new Layout("Body"),
                new Layout("Footer").Size(footerSize)
            );

        int tasksCount = _taskService.Count;

        _taskRenderables.Clear();

        if (tasksCount == 0)
        {
            _taskRenderables.Add(_noTasksRenderable);
        }
        else
        {
            int bodySize = Console.BufferHeight - (headerSize + footerSize);
            (int startIndex, int endIndex) = CalculateViewableTasksRange(bodySize, tasksCount);

            for (int i = startIndex; i < endIndex; i++)
            {
                TaskItem task = _taskService.GetTaskByIndex(i)!;
                string str = GetRow($"{i + 1}", task, i == _selectedIndex);
                _taskRenderables.Add(new Markup(str));
            }
        }

        layout["Body"].Update(new Rows(_taskRenderables));

        _footerRenderables[0] = _footerHintRenderable;
        _footerRenderables[1] = _emptyTextRenderable;
        if (_showTextInput)
        {
            string label = _textInputActionType == TextInputActionType.Add ? "Add Task" : "Edit Task";
            _footerRenderables[1] = new Markup(_textInput.Render(label, $"black on {_selectedPrimaryColor}"));
        }

        layout["Footer"].Update(new Rows(_footerRenderables));
        AnsiConsole.Write(layout);
    }

    private (int startIndex, int endIndex) CalculateViewableTasksRange(int bodySize, int tasksCount)
    {
        int availableDisplayRows = bodySize;
        if (availableDisplayRows <= 0) availableDisplayRows = 1;

        var start = 0;
        int end = tasksCount;
        if (tasksCount > availableDisplayRows)
        {
            start = Math.Max(0, _selectedIndex - availableDisplayRows / 2);
            if (start + availableDisplayRows > tasksCount)
            {
                start = Math.Max(0, tasksCount - availableDisplayRows);
            }

            end = Math.Min(tasksCount, start + availableDisplayRows);
        }

        return (start, end);
    }

    private string GetRow(string num, TaskItem task, bool isSelected)
    {
        var meta = task.UpdatedAt.ToLocalTime().ToString("dd-MM-yyyy HH:mm");

        const int numColWidth = 6;
        const int iconColWidth = 2; // For 󰄲 or 󰄱

        string numFormatted = num.PadLeft(numColWidth - 1) + " ";

        string descriptionStyle = task.IsCompleted ? CompletedDescriptionStyle : InCompleteDescriptionStyle;
        string metaStyle = isSelected ? MetaStyle : $"{MetaStyle} conceal";

        string icon = task.IsCompleted
            ? CompletedIcon.WrapInStyle(_selectedPrimaryColor)
            : IncompleteIcon.WrapInStyle(IncompleteIconStyle);

        string styledMeta = meta.WrapInStyle(metaStyle);
        int metaDisplayLength = styledMeta.StripMarkup().Length;

        // -2 because of the 2 spaces (one after icon, the other before meta)
        int availableDescriptionWidth = Console.BufferWidth - numColWidth - iconColWidth - 2 - metaDisplayLength;

        if (availableDescriptionWidth < 1) availableDescriptionWidth = 1;

        string truncatedDescription = Truncate(task.Description, availableDescriptionWidth);

        int numLength = numFormatted.StripMarkup().Length;
        int iconLength = icon.StripMarkup().Length;
        int descriptionLength = truncatedDescription.StripMarkup().Length;

        // 1s are for spaces
        int currentContentLength = numLength + iconLength + 1 + descriptionLength + 1 + metaDisplayLength;
        int remainingSpace = Math.Max(0, Console.BufferWidth - currentContentLength);

        var spaceStr = new string(' ', remainingSpace);

        string styledNum = numFormatted.WrapInStyle(DimStyle);
        string styledDesc = truncatedDescription.WrapInStyle(descriptionStyle);

        var finalRow = $"{styledNum}{icon} {styledDesc}{spaceStr}{styledMeta}";
        finalRow = finalRow.WrapInStyleIf(SelectedRowStyle, isSelected);

        return finalRow;
    }

    private static string Truncate(string s, int w)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        if (w <= 0) return string.Empty;

        if (s.Length <= w) return s;

        // No ellipsis, w is too small
        if (w < 4) return s[..Math.Min(s.Length, w)];

        return s[..(w - 1)] + "…";
    }
}