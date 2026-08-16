namespace GameKit.Tests;

public class WindowConfigTests
{
    [Test]
    public void DefaultCloseBehavior_QuitsApplication()
    {
        WindowConfig config = new();

        Assert.That(config.CloseBehavior, Is.EqualTo(WindowCloseBehavior.QuitApplication));
    }
}
