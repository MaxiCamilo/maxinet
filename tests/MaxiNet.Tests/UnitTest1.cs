using MaxiNet;

namespace MaxiNetTests;

public class PruebasTests
{
    [Fact]
    public async Task UnaPruebita()
    {
        var resultado = await Task.FromResult(42);
        Assert.Equal(42, resultado);
    }


    [Fact]
    public async Task TestTocMain()
    {
        Console.WriteLine("Starting Toc.Ruminate test...");
        var result = await Toc.Ruminate(async () =>
        {
            Console.WriteLine("Hello, world!");
            await Task.Delay(3000);
            Console.WriteLine("Goodbye, world!");
            return Res.Ok;
        });

        Assert.Equal(Res.Ok, result);



    }
}