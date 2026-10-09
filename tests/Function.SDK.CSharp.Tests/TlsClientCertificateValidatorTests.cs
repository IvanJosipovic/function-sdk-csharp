using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Function.SDK.CSharp;
using Shouldly;

namespace Function.SDK.CSharp.Example.Tests;

public class TlsClientCertificateValidatorTests
{
    private const string ClientAuthenticationOid = "1.3.6.1.5.5.7.3.2";
    private const string ServerAuthenticationOid = "1.3.6.1.5.5.7.3.1";

    [Fact]
    public void ValidateAcceptsClientCertificateIssuedByTrustedAuthority()
    {
        using var authority = new TestCertificateAuthority("CN=Crossplane");
        using var clientCertificate = authority.IssueClient(new X500DistinguishedName("CN=Function Client"));
        var trustedAuthorities = GetTrustedAuthorities(authority.Certificate);

        TlsClientCertificateValidator.Validate(clientCertificate, trustedAuthorities).ShouldBeTrue();
    }

    [Fact]
    public void ValidateAcceptsSelfIssuedClientCertificateSignedByTrustedAuthority()
    {
        using var authority = new TestCertificateAuthority("CN=Crossplane");
        using var clientCertificate = authority.IssueClient(authority.Certificate.SubjectName);
        var trustedAuthorities = GetTrustedAuthorities(authority.Certificate);

        TlsClientCertificateValidator.Validate(clientCertificate, trustedAuthorities).ShouldBeTrue();
    }

    [Fact]
    public void ValidateRejectsSelfIssuedCertificateSignedByUntrustedAuthority()
    {
        using var authority = new TestCertificateAuthority("CN=Crossplane");
        using var untrustedAuthority = new TestCertificateAuthority(authority.Certificate.SubjectName);
        using var clientCertificate = untrustedAuthority.IssueClient(untrustedAuthority.Certificate.SubjectName);
        var trustedAuthorities = GetTrustedAuthorities(authority.Certificate);

        TlsClientCertificateValidator.Validate(clientCertificate, trustedAuthorities).ShouldBeFalse();
    }

    [Fact]
    public void ValidateRejectsCertificateWithoutClientAuthenticationUsage()
    {
        using var authority = new TestCertificateAuthority("CN=Crossplane");
        using var clientCertificate = authority.IssueClient(
            authority.Certificate.SubjectName,
            ServerAuthenticationOid);
        var trustedAuthorities = GetTrustedAuthorities(authority.Certificate);

        TlsClientCertificateValidator.Validate(clientCertificate, trustedAuthorities).ShouldBeFalse();
    }

    [Fact]
    public void ValidateRejectsCertificateWithoutDigitalSignatureUsage()
    {
        using var authority = new TestCertificateAuthority("CN=Crossplane");
        using var clientCertificate = authority.IssueClient(
            new X500DistinguishedName("CN=Function Client"),
            ClientAuthenticationOid,
            X509KeyUsageFlags.KeyEncipherment);
        var trustedAuthorities = GetTrustedAuthorities(authority.Certificate);

        TlsClientCertificateValidator.Validate(clientCertificate, trustedAuthorities).ShouldBeFalse();
    }

    [Fact]
    public void ValidateRejectsMissingClientCertificate()
    {
        using var authority = new TestCertificateAuthority("CN=Crossplane");
        var trustedAuthorities = GetTrustedAuthorities(authority.Certificate);

        TlsClientCertificateValidator.Validate(null, trustedAuthorities).ShouldBeFalse();
    }

    private static X509Certificate2Collection GetTrustedAuthorities(X509Certificate2 authority)
    {
        var trustedAuthorities = new X509Certificate2Collection();
        trustedAuthorities.Add(authority);
        return trustedAuthorities;
    }

    private sealed class TestCertificateAuthority : IDisposable
    {
        private readonly RSA signingKey = RSA.Create(2048);

        public TestCertificateAuthority(string subjectName)
            : this(new X500DistinguishedName(subjectName))
        {
        }

        public TestCertificateAuthority(X500DistinguishedName subjectName)
        {
            var request = new CertificateRequest(
                subjectName,
                signingKey,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(
                X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign,
                true));
            request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

            var now = DateTimeOffset.UtcNow;
            Certificate = request.CreateSelfSigned(now.AddMinutes(-5), now.AddYears(10));
        }

        public X509Certificate2 Certificate { get; }

        public X509Certificate2 IssueClient(
            X500DistinguishedName subjectName,
            string extendedKeyUsageOid = ClientAuthenticationOid,
            X509KeyUsageFlags keyUsages = X509KeyUsageFlags.DigitalSignature)
        {
            using var clientKey = RSA.Create(2048);
            var request = new CertificateRequest(
                subjectName,
                clientKey,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(keyUsages, true));
            var enhancedKeyUsages = new OidCollection();
            enhancedKeyUsages.Add(new Oid(extendedKeyUsageOid));
            request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(enhancedKeyUsages, true));

            var now = DateTimeOffset.UtcNow;
            return request.Create(
                Certificate.SubjectName,
                X509SignatureGenerator.CreateForRSA(signingKey, RSASignaturePadding.Pkcs1),
                now.AddMinutes(-5),
                now.AddYears(1),
                CreateSerialNumber());
        }

        public void Dispose()
        {
            Certificate.Dispose();
            signingKey.Dispose();
        }

        private static byte[] CreateSerialNumber()
        {
            var serialNumber = RandomNumberGenerator.GetBytes(16);
            serialNumber[0] &= 0x7F;
            serialNumber[^1] |= 1;
            return serialNumber;
        }
    }
}
