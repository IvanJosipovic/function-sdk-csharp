using Function.SDK.CSharp;

namespace Function.SDK.CSharp.Sample;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.ConfigureFunction(args);

        var app = builder.Build();

        app.MapFunctionService<RunFunctionService>();

        app.Run();
    }
}
