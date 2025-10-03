namespace Tusky.Views;

public abstract class View
{
    protected readonly App App;

    protected View(App app)
    {
        App = app;
    }

    public abstract void Draw(AppState state);
}