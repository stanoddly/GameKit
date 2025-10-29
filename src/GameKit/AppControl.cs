namespace GameKit;

public class AppControl
{
    public bool QuitRequested { get; private set; } = false;

    public void Quit()
    {
        QuitRequested = true;
    }
}
