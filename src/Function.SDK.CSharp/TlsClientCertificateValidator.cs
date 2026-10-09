using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using BouncyCastleCertificate = Org.BouncyCastle.X509.X509Certificate;

namespace Function.SDK.CSharp;

internal static class TlsClientCertificateValidator
{
    private const string ClientAuthenticationOid = "1.3.6.1.5.5.7.3.2";
    private const string AnyExtendedKeyUsageOid = "2.5.29.37.0";

    public static bool Validate(
        X509Certificate2? clientCertificate,
        X509Certificate2Collection trustedCertificateAuthorities,
        X509Chain? presentedChain = null)
    {
        ArgumentNullException.ThrowIfNull(trustedCertificateAuthorities);

        var validAuthorities = trustedCertificateAuthorities
            .Cast<X509Certificate2>()
            .Where(IsValidAuthority)
            .ToArray();

        if (clientCertificate is null ||
            validAuthorities.Length == 0 ||
            !AllowsClientAuthentication(clientCertificate))
        {
            return false;
        }

        try
        {
            var parser = new X509CertificateParser();
            var target = parser.ReadCertificate(clientCertificate.RawData);
            var authorities = validAuthorities
                .Select(authority => parser.ReadCertificate(authority.RawData))
                .ToArray();
            var trustAnchors = authorities
                .Select(authority => new TrustAnchor(authority, null))
                .ToHashSet();
            var candidates = new HashSet<BouncyCastleCertificate> { target };

            if (presentedChain is not null)
            {
                foreach (X509ChainElement element in presentedChain.ChainElements)
                {
                    candidates.Add(parser.ReadCertificate(element.Certificate.RawData));
                }
            }

            var parameters = new PkixBuilderParameters(
                trustAnchors,
                new X509CertStoreSelector { Certificate = target })
            {
                IsRevocationEnabled = false
            };
            parameters.AddStoreCert(new CertificateStore(candidates));

            new PkixCertPathBuilder().Build(parameters);
            return true;
        }
        catch (PkixCertPathBuilderException)
        {
            return false;
        }
        catch (CertificateException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static bool AllowsClientAuthentication(X509Certificate2 certificate)
    {
        var keyUsages = certificate.Extensions.OfType<X509KeyUsageExtension>().ToArray();
        if (keyUsages.Length > 1 ||
            (keyUsages.Length == 1 &&
             (keyUsages[0].KeyUsages & X509KeyUsageFlags.DigitalSignature) == 0))
        {
            return false;
        }

        var enhancedKeyUsages = certificate.Extensions.OfType<X509EnhancedKeyUsageExtension>().ToArray();
        return enhancedKeyUsages.Length <= 1 &&
            (enhancedKeyUsages.Length == 0 ||
             enhancedKeyUsages[0].EnhancedKeyUsages
                 .Cast<Oid>()
                 .Any(usage => usage.Value is ClientAuthenticationOid or AnyExtendedKeyUsageOid));
    }

    private static bool IsValidAuthority(X509Certificate2 authority)
    {
        var constraints = authority.Extensions.OfType<X509BasicConstraintsExtension>().ToArray();
        var keyUsages = authority.Extensions.OfType<X509KeyUsageExtension>().ToArray();
        var now = DateTime.UtcNow;

        return constraints.Length == 1 &&
            constraints[0].CertificateAuthority &&
            keyUsages.Length <= 1 &&
            (keyUsages.Length == 0 ||
             (keyUsages[0].KeyUsages & X509KeyUsageFlags.KeyCertSign) != 0) &&
            authority.NotBefore.ToUniversalTime() <= now &&
            now <= authority.NotAfter.ToUniversalTime();
    }

    private sealed class CertificateStore(IEnumerable<BouncyCastleCertificate> certificates)
        : IStore<BouncyCastleCertificate>
    {
        public IEnumerable<BouncyCastleCertificate> EnumerateMatches(ISelector<BouncyCastleCertificate> selector)
        {
            return certificates.Where(selector.Match);
        }
    }
}
