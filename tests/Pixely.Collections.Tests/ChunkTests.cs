namespace Pixely.Collections.Tests;

public class ChunkTests
{
    [Test]
    public void ChunkWorksAsExpected()
    {
        var source1 = Enumerable.Range(0, 320).Select(i => (double)i).ToArray();
        var source2 = Enumerable.Range(0, 320).Select(i => (double)(i * 2)).ToArray();
        var source3 = Enumerable.Range(0, 320).Select(i => (double)(i * 3)).ToArray();
        var source4 = Enumerable.Range(0, 320).Select(i => (double)(i * 4)).ToArray();
        var source5 = Enumerable.Range(0, 320).Select(i => (double)(i * 5)).ToArray();

        var chunks = new Chunks<int, double, double, double, double, double>(
            stackalloc byte[512],
            source1.AsSpan(),
            source2.AsSpan(),
            source3.AsSpan(),
            source4.AsSpan(),
            source5.AsSpan());

        List<(int[] Values, double[] Multipliers)> results = new();

        foreach (var chunk in chunks)
        {
            Span<int> targetChunk = chunk.Target;

            for (int i = 0; i < chunk.Source1.Length; i++)
            {
                // Store average of all sources as integer
                targetChunk[i] = (int)((chunk.Source1[i] + chunk.Source2[i] + chunk.Source3[i] + chunk.Source4[i] + chunk.Source5[i]) / 5);
            }
            
            results.Add((
                targetChunk.ToArray(),
                chunk.Source1.ToArray()
            ));
        }

        // Expected chunk sizes for 320 items with max 128 ints per chunk
        Assert.That(results.Count, Is.EqualTo(3));
        Assert.That(results[0].Values.Length, Is.EqualTo(128));
        Assert.That(results[1].Values.Length, Is.EqualTo(128));
        Assert.That(results[2].Values.Length, Is.EqualTo(64));

        // Verify first chunk average calculations
        for (int i = 0; i < results[0].Values.Length; i++)
        {
            double expected = results[0].Multipliers[i] * 3; // Average of 1x,2x,3x,4x,5x
            Assert.That(results[0].Values[i], Is.EqualTo((int)expected));
        }
    }
}
