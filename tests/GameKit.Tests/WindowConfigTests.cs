namespace GameKit.Tests;

public sealed class WindowConfigTests
{
    [Test]
    public void Defaults_AreInitiallyVisibleAndQuitApplicationOnClose()
    {
        WindowConfig config = new();

        Assert.Multiple(() =>
        {
            Assert.That(config.InitiallyVisible, Is.True);
            Assert.That(config.CloseBehavior, Is.EqualTo(WindowCloseBehavior.QuitApplication));
        });
    }

    [Test]
    public void Configuration_CanStartHiddenAndHideOnClose()
    {
        WindowConfig config = new(
            InitiallyVisible: false,
            CloseBehavior: WindowCloseBehavior.HideWindow);

        Assert.Multiple(() =>
        {
            Assert.That(config.InitiallyVisible, Is.False);
            Assert.That(config.CloseBehavior, Is.EqualTo(WindowCloseBehavior.HideWindow));
        });
    }
}
