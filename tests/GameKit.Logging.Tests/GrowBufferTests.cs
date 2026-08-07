using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace GameKit.Logging.Tests;

public class GrowBufferTests
{
    [Test]
    public async Task GrowBuffer_WhenOutputIsStalled_DoesNotBlockProducerAndDrainsInOrder()
    {
        BlockingStream stream = new();
        List<Exception> errors = new();
        ILoggerFactory loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Trace);
            logging.AddZLoggerStream(stream, options =>
            {
                options.FullMode = BackgroundBufferFullMode.Grow;
                options.InternalErrorLogger = errors.Add;
            });
        });
        ILogger logger = loggerFactory.CreateLogger<GrowBufferTests>();

        logger.ZLogInformation($"entry 0");
        Assert.That(stream.WaitForWrite(TimeSpan.FromSeconds(2)), Is.True);

        Task producer = Task.Run(() =>
        {
            for (int i = 1; i <= 1_000; i++)
            {
                logger.ZLogInformation($"entry {i}");
            }
        });

        Task completed = await Task.WhenAny(producer, Task.Delay(TimeSpan.FromSeconds(2)));
        Assert.That(completed, Is.SameAs(producer));

        stream.ReleaseWrites();
        await producer;
        loggerFactory.Dispose();

        string contents = stream.GetContents();
        Assert.Multiple(() =>
        {
            Assert.That(contents.IndexOf("entry 0", StringComparison.Ordinal), Is.LessThan(contents.IndexOf("entry 1", StringComparison.Ordinal)));
            Assert.That(contents.IndexOf("entry 999", StringComparison.Ordinal), Is.LessThan(contents.IndexOf("entry 1000", StringComparison.Ordinal)));
            Assert.That(errors, Is.Empty);
        });
    }

    [Test]
    public void GrowBuffer_AfterDisposal_ReleasesCapturedValues()
    {
        BlockingStream stream = new();
        stream.ReleaseWrites();
        ILoggerFactory loggerFactory = LoggerFactory.Create(logging =>
        {
            logging.AddZLoggerStream(stream, static options =>
            {
                options.FullMode = BackgroundBufferFullMode.Grow;
                options.InternalErrorLogger = static _ => { };
            });
        });
        ILogger logger = loggerFactory.CreateLogger<GrowBufferTests>();

        WeakReference capturedValue = EnqueueCapturedValue(logger);
        loggerFactory.Dispose();

        GC.Collect(2, GCCollectionMode.Forced, true, true);

        Assert.That(capturedValue.IsAlive, Is.False);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference EnqueueCapturedValue(ILogger logger)
    {
        CapturedValue value = new();
        WeakReference reference = new(value);
        logger.ZLogInformation($"{value}");
        return reference;
    }

    private sealed class CapturedValue;

    private sealed class BlockingStream : Stream
    {
        private readonly MemoryStream _inner = new();
        private readonly ManualResetEventSlim _writeStarted = new();
        private readonly ManualResetEventSlim _writesReleased = new();

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => throw new NotSupportedException();
        }

        public bool WaitForWrite(TimeSpan timeout)
        {
            return _writeStarted.Wait(timeout);
        }

        public void ReleaseWrites()
        {
            _writesReleased.Set();
        }

        public string GetContents()
        {
            return Encoding.UTF8.GetString(_inner.ToArray());
        }

        public override void Flush()
        {
            _inner.Flush();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _writeStarted.Set();
            _writesReleased.Wait();
            _inner.Write(buffer, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writeStarted.Dispose();
                _writesReleased.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
