using FCIWeb.Core.Security;
using Xunit;

namespace FCIWeb.Core.Tests
{
    public class SecretRedactorTests
    {
        [Fact]
        public void Redact_MasksPassword()
        {
            var result = SecretRedactor.Redact("Password=Secret123");
            Assert.Equal("Password=***REDACTED***", result);
        }

        [Fact]
        public void Redact_MasksPwdWithColon()
        {
            var result = SecretRedactor.Redact("pwd: hunter2");
            Assert.Equal("pwd: ***REDACTED***", result);
        }

        [Fact]
        public void Redact_MasksConnectionStringCredentials()
        {
            var cs = "Data Source=sql;Initial Catalog=db;User ID=admin;Password=p@ss";
            var result = SecretRedactor.RedactConnectionString(cs);
            Assert.DoesNotContain("admin", result);
            Assert.DoesNotContain("p@ss", result);
            Assert.Contains("Data Source=sql", result);
            Assert.Contains(SecretRedactor.Mask, result);
        }

        [Fact]
        public void Redact_MasksBearerToken()
        {
            var result = SecretRedactor.Redact("Authorization: Bearer eyJhbGciOiJIUzI1NiJ9.payload.sig");
            Assert.DoesNotContain("eyJhbGciOiJIUzI1NiJ9", result);
            Assert.Contains(SecretRedactor.Mask, result);
        }

        [Fact]
        public void Redact_MasksPrivateKey()
        {
            var text = "-----BEGIN RSA PRIVATE KEY-----\nMIIEow...\n-----END RSA PRIVATE KEY-----";
            var result = SecretRedactor.Redact(text);
            Assert.DoesNotContain("MIIEow", result);
            Assert.Equal(SecretRedactor.Mask, result);
        }

        [Fact]
        public void Redact_LeavesCleanTextUnchanged()
        {
            const string clean = "Elaborazione completata per la UO 123.";
            Assert.Equal(clean, SecretRedactor.Redact(clean));
        }

        [Fact]
        public void Redact_NullOrEmpty_ReturnsInput()
        {
            Assert.Null(SecretRedactor.Redact(null));
            Assert.Equal("", SecretRedactor.Redact(""));
        }
    }
}
