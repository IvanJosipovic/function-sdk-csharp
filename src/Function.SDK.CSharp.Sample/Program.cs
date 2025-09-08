using Function.SDK.CSharp.Models;

namespace Function.SDK.CSharp.Sample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.ConfigureFunction(args);

        var app = builder.Build();

        app.MapFunctionService<RunFunctionService>();

        app.MapPost("/convert", static (V1ConversionReview conversion) => ConversionWebhook.Convert(conversion))
            .Produces<V1ConversionReview>(200, "application/json");

        app.Run();
    }
}
