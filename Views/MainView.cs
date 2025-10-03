using Spectre.Console;
using Spectre.Console.Rendering;
using Tusky.Models;
using Tusky.Services;
using Tusky.Utils;

namespace Tusky.Views;

public class MainView : View
{
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

    public MainView(App app) : base(app)
    {
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

    public override void Draw(AppState state)
    {
        const int headerSize = 2;
        int footerSize = state.ShowTextInput ? 2 : 1;

        string filterIcon = state.FilterMode switch
        {
            TaskService.FilterMode.Completed => "󰪥",
            TaskService.FilterMode.Incomplete => "󰄰",
            _ => "󰪡"
        };

        string sortIcon = state.SortMode switch
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

        int tasksCount = App.TaskService.Count;

        _taskRenderables.Clear();

        if (tasksCount == 0)
        {
            _taskRenderables.Add(_noTasksRenderable);
        }
        else
        {
            int bodySize = Console.BufferHeight - (headerSize + footerSize);
            (int startIndex, int endIndex) = CalculateViewableTasksRange(state, bodySize, tasksCount);

            for (int i = startIndex; i < endIndex; i++)
            {
                TaskItem task = App.TaskService.GetTaskByIndex(i)!;
                string str = GetRow($"{i + 1}", task, i == state.SelectedIndex);
                _taskRenderables.Add(new Markup(str));
            }
        }

        layout["Body"].Update(new Rows(_taskRenderables));

        _footerRenderables[0] = _footerHintRenderable;
        _footerRenderables[1] = _emptyTextRenderable;
        if (state.ShowTextInput)
        {
            string label = state.IsAddingTask ? "Add Task" : "Edit Task";
            _footerRenderables[1] = new Markup(state.TextInput.Render(label, $"black on {_selectedPrimaryColor}"));
        }

        layout["Footer"].Update(new Rows(_footerRenderables));
        AnsiConsole.Write(layout);
    }
    
    private (int startIndex, int endIndex) CalculateViewableTasksRange(AppState state, int bodySize, int tasksCount)
    {
        int availableDisplayRows = bodySize;
        if (availableDisplayRows <= 0) availableDisplayRows = 1;

        var start = 0;
        int end = tasksCount;
        if (tasksCount > availableDisplayRows)
        {
            start = Math.Max(0, state.SelectedIndex - availableDisplayRows / 2);
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