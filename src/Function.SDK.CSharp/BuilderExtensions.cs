using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using static Apiextensions.Fn.Proto.V1.FunctionRunnerService;

namespace Function.SDK.CSharp;

//Args:
// --tls_certs_dir: A directory containing tls.crt, tls.key, and ca.crt.
// --address: The address at which to listen for requests.
// --creds: The credentials used to authenticate requests.
// --insecure: Serve insecurely, without credentials or encryption.
// --debug -d

public static class BuilderExtensions
{
    /// <summary>
    /// Configures the WebApplicationBuilder for Crossplane Function.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder to configure.</param>
    /// <param name="args">The command line arguments passed to the application.</param>
    public static void ConfigureFunction(this WebApplicationBuilder builder, string[] args)
    {
        bool IsDebug()
        {
            return args.Contains("-d") || args.Contains("--debug");
        }

        bool IsInsecure()
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--insecure")
                {
                    return true;
                }
            }

            return false;
        }

        string GetAddress()
        {
            var address = "0.0.0.0:9443";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--address")
                {
                    address = args[i + 1];
                }
            }

            var insecure = IsInsecure();

            return (IsInsecure() ? "http://" : GetTLSCertDir() != null ? "https://" : "http://") + address;
        }

        string? GetTLSCertDir()
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--tls_certs_dir")
                {
                    return args[i + 1];
                }
            }

            return Environment.GetEnvironmentVariable("TLS_SERVER_CERTS_DIR");
        }

        builder.WebHost.UseUrls(GetAddress());

        var tls = GetTLSCertDir();

        builder.Services.Configure<KestrelServerOptions>(options =>
        {
            options.ConfigureEndpointDefaults(lo =>
            {
                lo.Protocols = HttpProtocols.Http2;

                if (IsInsecure() == false && tls != null)
                {
                    lo.UseHttps(sslOpts =>
                    {
                        sslOpts.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                        sslOpts.ServerCertificate = X509Certificate2.CreateFromPemFile(Path.Combine(tls, "tls.crt"), Path.Combine(tls, "tls.key"));
                        sslOpts.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;

                        // Client Certs are not signed by the provided CA...
                        sslOpts.AllowAnyClientCertificate();
                    });
                }
            });
        });

        builder.Logging.AddFilter("Default", IsDebug() ? LogLevel.Debug : LogLevel.Information);
        builder.Logging.AddFilter("Microsoft.AspNetCore", IsDebug() ? LogLevel.Debug : LogLevel.Warning);
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
