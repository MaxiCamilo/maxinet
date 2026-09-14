using Xunit.Abstractions;

namespace MaxiNet.Tests;

public class DebugTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task TestTocMain()
    {
        testOutputHelper.WriteLine("Starting Toc.Ruminate test...");
        var result1 = Toc.Ruminate(async () =>
        {
            testOutputHelper.WriteLine("Hello, world 1!");
            await Task.Delay(3000);
            testOutputHelper.WriteLine("Goodbye, world 1!");
            return Res.Value(1);
        });
        var result2 = Toc.Ruminate(async () =>
        {
            testOutputHelper.WriteLine("Hello, world 2!");
            await Task.Delay(5000);
            testOutputHelper.WriteLine("Goodbye, world!");
            return Res.Value(2);
        });
        var result3 = Toc.Ruminate(async () =>
        {
            testOutputHelper.WriteLine("Hello, world 3!");
            await Task.Delay(7500);
            testOutputHelper.WriteLine("Goodbye, world 3!");
            return Res.Value(3);
        });

        var result4 = Toc.Ruminate(async () =>
        {
            testOutputHelper.WriteLine("Hello, world 4!");
            await Task.Delay(12000);
            testOutputHelper.WriteLine("Goodbye, world 4!");
            return Res.Value(4);
        });

        var result5 = Toc.Ruminate(async () =>
        {
            testOutputHelper.WriteLine("Hello, world 5!");
            await Task.Delay(7500);
            return Res.ValError<int>(new Oration("An error occurred"));
        });


        var results = await Task.WhenAll(result1, result2, result3, result4, result5);

        Assert.Equal(Res.Value(1), results[0]);
        Assert.Equal(Res.Value(2), results[1]);
        Assert.Equal(Res.Value(3), results[2]);
        Assert.Equal(Res.Value(4), results[3]);
    }
}