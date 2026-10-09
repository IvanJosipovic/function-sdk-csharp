using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using static Apiextensions.Fn.Proto.V1.FunctionRunnerService;

namespace Function.SDK.CSharp;

// Supported function options:
// --tls-certs-dir, --address, --insecure, --debug, and -d.

public static class BuilderExtensions
{
    /// <summary>
    /// Configures the WebApplicationBuilder for Crossplane Function.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder to configure.</param>
    /// <param name="args">The command line arguments passed to the application.</param>
    public static void ConfigureFunction(this WebApplicationBuilder builder, string[] args)
    {
        var functionOptions = FunctionServerOptions.Parse(
            args,
            Environment.GetEnvironmentVariable("TLS_SERVER_CERTS_DIR"));
        functionOptions.EnsureCredentialsConfigured();

        builder.WebHost.UseUrls(functionOptions.GetUrl());

        var tls = functionOptions.Insecure ? null : functionOptions.TlsCertsDirectory;

        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.ConfigureEndpointDefaults(lo =>
            {
                lo.Protocols = HttpProtocols.Http2;

                if (!functionOptions.Insecure && tls != null)
                {
                    var clientCertificateAuthorities = new X509Certificate2Collection();
                    clientCertificateAuthorities.ImportFromPemFile(Path.Combine(tls, "ca.crt"));

                    if (clientCertificateAuthorities.Count == 0)
                    {
                        throw new InvalidOperationException($"No client certificate authorities were found in '{Path.Combine(tls, "ca.crt")}'.");
                    }

                    lo.UseHttps(sslOpts =>
                    {
                        sslOpts.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                        sslOpts.ServerCertificate = X509Certificate2.CreateFromPemFile(Path.Combine(tls, "tls.crt"), Path.Combine(tls, "tls.key"));
                        sslOpts.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                        sslOpts.ClientCertificateValidation = (certificate, chain, _) =>
                            TlsClientCertificateValidator.Validate(certificate, clientCertificateAuthorities, chain);
                    });
                }
            });
        });

        builder.Logging.AddFilter("Default", functionOptions.Debug ? LogLevel.Debug : LogLevel.Information);
        builder.Logging.AddFilter("Microsoft.AspNetCore", functionOptions.Debug ? LogLevel.Debug : LogLevel.Warning);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Critical);
        builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);

        builder.Services.AddGrpc();
        builder.Services.AddGrpcReflection();

        builder.Logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
        });
    }

    /// <summary>
    /// Configures the WebApplication to the Function gRPC service.
    /// </summary>
    /// <typeparam name="TService">Type which inherits from FunctionRunnerServiceBase</typeparam>
    /// <param name="app"></param>
    public static void MapFunctionService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicMethods | DynamicallyAccessedMemberTypes.NonPublicMethods)] TService>(this WebApplication app) where TService : FunctionRunnerServiceBase
    {
        var versionAttribute = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        app.Logger.LogInformation("Starting Application v{version}", versionAttribute);

        app.MapGrpcService<TService>();
        app.MapGrpcReflectionService();
    }
}
