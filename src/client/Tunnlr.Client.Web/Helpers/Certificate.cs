using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Tunnlr.Client.Utilities;

namespace Tunnlr.Client.Web.Helpers;

public static class Certificate
{
    internal static X509Certificate2 GetSelfSignedCertificate()
    {
        var storageDirectory = Storage.GetTunnlrStorageDirectory();
        const string certificateFileName = "tunnlr-web-certificate.pfx";
        var certificateFilePath = Path.Combine(storageDirectory, certificateFileName);

        X509Certificate2 certificate;
        if (File.Exists(certificateFilePath))
        {
            certificate = new X509Certificate2(certificateFilePath);
            if (certificate.NotAfter > DateTimeOffset.Now)
            {
                return certificate;
            }
        }

        certificate = GenerateSelfSignedCertificate();
        File.WriteAllBytes(certificateFilePath, certificate.Export(X509ContentType.Pfx, string.Empty));

        return certificate;
    }

    private static X509Certificate2 GenerateSelfSignedCertificate()
    {
        var commonName = "Tunnlr Web UI";
        var ecdsaCurve = ECCurve.NamedCurves.nistP256;
        var years = 5;
        var hashAlgorithm = HashAlgorithmName.SHA256;

        using var ecdsa = ECDsa.Create(ecdsaCurve);
        
        var request = new CertificateRequest(new X500DistinguishedName($"cn={commonName}"), ecdsa, hashAlgorithm);

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment |
                X509KeyUsageFlags.DigitalSignature, false)
        );
        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false)
        );

        // Create a self-signed certificate
        var certificate = request.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(years));

        // Export the certificate with a private key into a PFX file
        var pfxBytes = certificate.Export(X509ContentType.Pfx, string.Empty);

        // Return the certificate from the PFX data
        return new X509Certificate2(pfxBytes, string.Empty, X509KeyStorageFlags.Exportable);
    }
}