namespace Function.SDK.CSharp;

internal sealed record FunctionServerOptions(
    string Address,
    string? TlsCertsDirectory,
    bool Insecure,
    bool Debug)
{
    private const string DefaultAddress = ":9443";

    internal static FunctionServerOptions Parse(string[] args, string? tlsCertsDirectoryFromEnvironment)
    {
        ArgumentNullException.ThrowIfNull(args);

        var address = DefaultAddress;
        var tlsCertsDirectory = NormalizeDirectory(tlsCertsDirectoryFromEnvironment);
        var insecure = false;
        var debug = false;

        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            var separatorIndex = argument.IndexOf('=');
            var name = separatorIndex < 0 ? argument : argument[..separatorIndex];
            var inlineValue = separatorIndex < 0 ? null : argument[(separatorIndex + 1)..];

            switch (name)
            {
                case "--address":
                    address = ReadValue(args, ref index, name, inlineValue);
                    break;
                case "--tls-certs-dir":
                    tlsCertsDirectory = NormalizeDirectory(ReadValue(args, ref index, name, inlineValue));
                    break;
                case "--tls_certs_dir":
                    throw new ArgumentException(
                        "Option '--tls_certs_dir' has been removed; use '--tls-certs-dir'.",
                        nameof(args));
                case "--insecure":
                    insecure = ReadBoolean(name, inlineValue);
                    break;
                case "--debug":
                case "-d":
                    debug = ReadBoolean(name, inlineValue);
                    break;
                case "--creds":
                    throw new ArgumentException(
                        "'--creds' is not a supported function option. Configure mTLS with '--tls-certs-dir' or explicitly use '--insecure'.",
                        nameof(args));
            }
        }

        if (string.IsNullOrWhiteSpace(address) || address.Contains("://", StringComparison.Ordinal))
        {
            throw new ArgumentException("'--address' must be a host:port address, such as ':9443'.", nameof(args));
        }

        return new FunctionServerOptions(address, tlsCertsDirectory, insecure, debug);
    }

    internal string GetUrl()
    {
        var scheme = Insecure ? "http" : "https";
        var address = Address.StartsWith(':') ? $"*{Address}" : Address;
        return $"{scheme}://{address}";
    }

    internal void EnsureCredentialsConfigured()
    {
        if (!Insecure && TlsCertsDirectory is null)
        {
            throw new InvalidOperationException(
                "No credentials provided. Configure '--tls-certs-dir' or TLS_SERVER_CERTS_DIR, or explicitly use '--insecure'.");
        }
    }

    private static string ReadValue(string[] args, ref int index, string optionName, string? inlineValue)
    {
        if (inlineValue is not null)
        {
            return inlineValue;
        }

        if (index + 1 >= args.Length || args[index + 1].StartsWith("-", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Option '{optionName}' requires a value.", nameof(args));
        }

        return args[++index];
    }

    private static bool ReadBoolean(string optionName, string? value)
    {
        if (value is null)
        {
            return true;
        }

        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Option '{optionName}' must be a boolean value.");
    }

    private static string? NormalizeDirectory(string? directory)
    {
        return string.IsNullOrWhiteSpace(directory) ? null : directory;
    }
}
