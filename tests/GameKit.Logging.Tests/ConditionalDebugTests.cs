using GameKit.Logging;
using Microsoft.Extensions.Logging;

namespace GameKit.Logging.Tests;

public class ConditionalDebugTests
{
#if DEBUG
    [Test]
    public void ZLogConditionalDebug_WhenDebugIsEnabled_EvaluatesAndWrites()
    {
        TestLogger logger = new(true);
        int evaluationCount = 0;

        logger.ZLogConditionalDebug($"Value {Evaluate(ref evaluationCount)}");

        Assert.That(evaluationCount, Is.EqualTo(1));
        Assert.That(logger.WriteCount, Is.EqualTo(1));
        Assert.That(logger.LastMessage, Does.Contain("Value 1"));
    }

    [Test]
    public void ZLogConditionalDebug_WhenDebugIsDisabled_DoesNotEvaluateOrWrite()
    {
        TestLogger logger = new(false);
        int evaluationCount = 0;

        logger.ZLogConditionalDebug($"Value {Evaluate(ref evaluationCount)}");

        Assert.That(evaluationCount, Is.Zero);
        Assert.That(logger.WriteCount, Is.Zero);
    }
#else
    [Test]
    public void ZLogConditionalDebug_WithoutDebugSymbol_DoesNotEvaluateOrWrite()
    {
        TestLogger logger = new(true);
        int evaluationCount = 0;

        logger.ZLogConditionalDebug($"Value {Evaluate(ref evaluationCount)}");

        Assert.That(evaluationCount, Is.Zero);
        Assert.That(logger.WriteCount, Is.Zero);
    }
#endif

    private static int Evaluate(ref int evaluationCount)
    {
        evaluationCount++;
        return evaluationCount;
    }

    private sealed class TestLogger : ILogger
    {
        private readonly bool _enabled;

        public int WriteCount { get; private set; }
        public string? LastMessage { get; private set; }

        public TestLogger(bool enabled)
        {
            _enabled = enabled;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _enabled;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            WriteCount++;
            LastMessage = formatter(state, exception);
        }
    }
}
