using Spectre.Console;
using Tusky.Utils;

namespace Tusky.Views;

public class ProjectsView : View
{
    public ProjectsView(App app) : base(app)
    {
    }

    public override void Draw(AppState state)
    {
        for (var i = 0; i < App.Projects.Length; i++)
        {
            string project = App.Projects[i];
            project = project.PadLeft(project.Length + 4).PadRight(Console.BufferWidth);
            project = project.WrapInStyleIf("on grey15", i == state.SelectedIndex);
            AnsiConsole.Markup(project);
        }
    }
}