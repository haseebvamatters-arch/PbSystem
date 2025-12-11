using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace PbSystem.Interfaces
{
    public interface ISignatureService
    {
        string Sign(string unsignedXml, X509Certificate2 certificate);
    }


    public class SignatureService : ISignatureService
    {
        public string Sign(string unsignedXml, X509Certificate2 certificate)
        {
            var xmlDoc = new XmlDocument { PreserveWhitespace = true };
            xmlDoc.LoadXml(unsignedXml);

            var signedXml = new SignedXml(xmlDoc)
            {
                SigningKey = certificate.GetRSAPrivateKey()
            };

            var reference = new Reference { Uri = "" };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            signedXml.AddReference(reference);

            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(certificate));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            XmlElement xmlSig = signedXml.GetXml();
            xmlDoc.DocumentElement?.AppendChild(xmlDoc.ImportNode(xmlSig, true));

            return xmlDoc.OuterXml;
        }
    }


    public enum EncryptionMethodEnum { Rsa, Ecdsa }

    public class SelfSignedCertificateForSignatureBuilder
    {
        private string _given = "Demo";
        private string _surname = "User";
        private string _serial = Guid.NewGuid().ToString("N").Substring(0, 8);
        private string _cn = "Demo Cert";
        private EncryptionMethodEnum _enc = EncryptionMethodEnum.Rsa;

        public static SelfSignedCertificateForSignatureBuilder Create() => new();

        public SelfSignedCertificateForSignatureBuilder WithGivenName(string given) { _given = given; return this; }
        public SelfSignedCertificateForSignatureBuilder WithSurname(string surname) { _surname = surname; return this; }
        public SelfSignedCertificateForSignatureBuilder WithSerialNumber(string serial) { _serial = serial; return this; }
        public SelfSignedCertificateForSignatureBuilder WithCommonName(string cn) { _cn = cn; return this; }
        public SelfSignedCertificateForSignatureBuilder AndEncryptionType(EncryptionMethodEnum enc) { _enc = enc; return this; }

        public X509Certificate2 Build()
        {
            var subject = new X500DistinguishedName($"CN={_cn}, SN={_surname}, GN={_given}, SERIALNUMBER={_serial}");

            if (_enc == EncryptionMethodEnum.Rsa)
            {
                using var rsa = RSA.Create(2048);
                var csr = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                csr.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
                csr.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(csr.PublicKey, false));

                var cert = csr.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
                var pfx = cert.Export(X509ContentType.Pfx, string.Empty);
                return new X509Certificate2(pfx, string.Empty, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);
            }

            throw new NotSupportedException("Only RSA demo is implemented.");
        }
    }
}
