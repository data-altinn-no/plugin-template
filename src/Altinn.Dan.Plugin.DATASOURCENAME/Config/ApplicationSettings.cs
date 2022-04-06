using System;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;

namespace Altinn.Dan.Plugin.DATASOURCENAME.Config
{
    public class ApplicationSettings
    {
        private X509Certificate2 _certificate;
        private KeyVaultSecret _keyVaultSecret;

        public string RedisConnectionString { get; set; }
        public TimeSpan BreakerRetryWaitTime { get; set; }
        public string DATASETNAME1URL { get; set; }
        public string DATASETNAME2URL { get; set; }
        public string KeyVaultName { get; set; }
        public string CertificateName { get; set; }
        public string SecretName { get; set; }
        public X509Certificate2 Certificate
        {
            get
            {
                if (_certificate == null)
                {
                    var certificateClient = new CertificateClient(
                        new Uri($"https://{KeyVaultName}.vault.azure.net/"),
                        new DefaultAzureCredential());
                    var keyVaultCertificateWithPolicy = certificateClient.GetCertificate(CertificateName).Value;
                    _certificate = new X509Certificate2(keyVaultCertificateWithPolicy.Cer);
                }

                return _certificate;
            }

            set => _certificate = value;
        }
        public KeyVaultSecret Secret
        {
            get
            {
                if (_keyVaultSecret == null)
                {
                    var secretClient = new SecretClient(
                        new Uri($"https://{KeyVaultName}.vault.azure.net/"),
                        new DefaultAzureCredential());
                    _keyVaultSecret = secretClient.GetSecret(SecretName).Value;
                }

                return _keyVaultSecret;
            }

            set => _keyVaultSecret = value;
        }
    }
}
