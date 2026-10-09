using Function.SDK.CSharp;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;

namespace Function.SDK.CSharp.Tests;

public class FunctionServerOptionsTests
{
    [Fact]
    public void ParseUsesGoDefaultsAndEnvironmentTlsDirectory()
    {
        var options = FunctionServerOptions.Parse([], "/tls");

        options.Address.ShouldBe(":9443");
        options.TlsCertsDirectory.ShouldBe("/tls");
        options.Insecure.ShouldBeFalse();
        options.Debug.ShouldBeFalse();
        options.GetUrl().ShouldBe("https://*:9443");
    }

    [Fact]
    public void ParseSupportsGoStyleFlagsAndInsecureOverridesTls()
    {
        var options = FunctionServerOptions.Parse(
            ["--address=127.0.0.1:9444", "--tls-certs-dir", "/tls", "--insecure", "--debug"],
            null);

        options.Address.ShouldBe("127.0.0.1:9444");
        options.TlsCertsDirectory.ShouldBe("/tls");
        options.Insecure.ShouldBeTrue();
        options.Debug.ShouldBeTrue();
        options.GetUrl().ShouldBe("http://127.0.0.1:9444");
        Should.NotThrow(options.EnsureCredentialsConfigured);
    }

    [Fact]
    public void ParseCommandLineValuesOverrideEnvironmentValues()
    {
        var options = FunctionServerOptions.Parse(
            ["--address", ":9444", "--tls-certs-dir", "/cli-tls", "--insecure=false", "--debug=true"],
            "/environment-tls");

        options.Address.ShouldBe(":9444");
        options.TlsCertsDirectory.ShouldBe("/cli-tls");
        options.Insecure.ShouldBeFalse();
        options.Debug.ShouldBeTrue();
        options.GetUrl().ShouldBe("https://*:9444");
    }

    [Fact]
    public void ParseRejectsLegacyTlsDirectoryFlagAndSupportsDebugShortFlag()
    {
        var exception = Should.Throw<ArgumentException>(
            () => FunctionServerOptions.Parse(["--tls_certs_dir", "/tls"], null));

        exception.Message.ShouldContain("has been removed");

        var options = FunctionServerOptions.Parse(["--tls-certs-dir", "/tls", "-d"], null);
        options.TlsCertsDirectory.ShouldBe("/tls");
        options.Debug.ShouldBeTrue();
    }

    [Fact]
    public void ParseAllowsTlsDirectoryToBeExplicitlyCleared()
    {
        var options = FunctionServerOptions.Parse(["--tls-certs-dir="], "/environment-tls");

        options.TlsCertsDirectory.ShouldBeNull();
        Should.Throw<InvalidOperationException>(options.EnsureCredentialsConfigured);
    }

    [Theory]
    [InlineData("--address")]
    [InlineData("--tls-certs-dir")]
    public void ParseRejectsMissingOptionValues(string option)
    {
        var exception = Should.Throw<ArgumentException>(
            () => FunctionServerOptions.Parse([option], null));

        exception.Message.ShouldContain("requires a value");
    }

    [Theory]
    [InlineData("--insecure=invalid")]
    [InlineData("--debug=invalid")]
    public void ParseRejectsInvalidBooleanValues(string option)
    {
        Should.Throw<ArgumentException>(() => FunctionServerOptions.Parse([option], null));
    }

    [Fact]
    public void ParseAllowsDisablingDebugWithEitherDebugFlag()
    {
        var options = FunctionServerOptions.Parse(["--debug", "-d=false"], null);

        options.Debug.ShouldBeFalse();
    }

    [Fact]
    public void ParseRejectsTheUnimplementedCredentialsOption()
    {
        var exception = Should.Throw<ArgumentException>(
            () => FunctionServerOptions.Parse(["--creds", "credentials"], null));

        exception.Message.ShouldContain("not a supported function option");
    }

    [Fact]
    public void MissingTlsCredentialsRequireExplicitInsecureMode()
    {
        var options = FunctionServerOptions.Parse([], null);

        var exception = Should.Throw<InvalidOperationException>(options.EnsureCredentialsConfigured);

        exception.Message.ShouldContain("No credentials provided");
    }

    [Fact]
    public void ConfigureFunctionRejectsMissingCredentials()
    {
        var builder = WebApplication.CreateSlimBuilder([]);

        var exception = Should.Throw<InvalidOperationException>(
            () => builder.ConfigureFunction(["--tls-certs-dir="]));

        exception.Message.ShouldContain("No credentials provided");
    }

    [Fact]
    public void ConfigureFunctionAllowsInsecureModeToOverrideTlsDirectory()
    {
        var builder = WebApplication.CreateSlimBuilder([]);

        builder.ConfigureFunction(["--address", ":9555", "--tls-certs-dir", "/does-not-exist", "--insecure"]);

        builder.WebHost.GetSetting("urls").ShouldBe("http://*:9555");
    }

    [Fact]
    public void ConfigureFunctionUsesTlsDirectoryAndAddress()
    {
        var builder = WebApplication.CreateSlimBuilder([]);

        builder.ConfigureFunction(["--address", "127.0.0.1:9444", "--tls-certs-dir", "/tls"]);

        builder.WebHost.GetSetting("urls").ShouldBe("https://127.0.0.1:9444");
    }

    [Fact]
    public void ConfigureFunctionUsesDebugLoggingWhenRequested()
    {
        var builder = WebApplication.CreateSlimBuilder([]);
        builder.ConfigureFunction(["--insecure", "--debug"]);
        using var app = builder.Build();

        var rules = app.Services.GetRequiredService<IOptions<LoggerFilterOptions>>().Value.Rules;

        Assert.Contains(rules, rule => rule.CategoryName == "Default" && rule.LogLevel == LogLevel.Debug);
        Assert.Contains(rules, rule => rule.CategoryName == "Microsoft.AspNetCore" && rule.LogLevel == LogLevel.Debug);
    }

    [Fact]
    public void InsecureCanBeExplicitlyDisabled()
    {
        var options = FunctionServerOptions.Parse(["--insecure", "--insecure=false"], "/tls");

        options.Insecure.ShouldBeFalse();
        options.GetUrl().ShouldBe("https://*:9443");
    }
}
