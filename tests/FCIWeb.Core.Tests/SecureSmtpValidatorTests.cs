using FCIWeb.Core.Security;
using Xunit;

namespace FCIWeb.Core.Tests
{
    public class SecureSmtpValidatorTests
    {
        private static SecureSmtpOptions Valid() => new SecureSmtpOptions
        {
            Host = "smtp.example.com",
            Port = 587,
            UseSsl = true,
            Username = "user",
            Password = "secret"
        };

        [Fact]
        public void Validate_ValidConfiguration_HasNoErrors()
        {
            Assert.Empty(SecureSmtpValidator.Validate(Valid()));
        }

        [Fact]
        public void Validate_NullOptions_ReportsError()
        {
            Assert.Single(SecureSmtpValidator.Validate(null));
        }

        [Fact]
        public void Validate_CredentialsWithoutSslOnRemoteHost_IsRejected()
        {
            var o = Valid();
            o.UseSsl = false;
            var errors = SecureSmtpValidator.Validate(o);
            Assert.Contains(errors, e => e.Contains("chiaro"));
        }

        [Fact]
        public void Validate_CredentialsWithoutSslOnLocalhost_IsAllowed()
        {
            var o = Valid();
            o.UseSsl = false;
            o.Host = "localhost";
            Assert.Empty(SecureSmtpValidator.Validate(o));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(70000)]
        public void Validate_InvalidPort_IsRejected(int port)
        {
            var o = Valid();
            o.Port = port;
            Assert.Contains(SecureSmtpValidator.Validate(o), e => e.Contains("Porta"));
        }

        [Fact]
        public void Validate_MissingHost_IsRejected()
        {
            var o = Valid();
            o.Host = "";
            Assert.Contains(SecureSmtpValidator.Validate(o), e => e.Contains("Host"));
        }

        [Fact]
        public void Validate_PasswordWithoutUsername_IsRejected()
        {
            var o = Valid();
            o.Username = "";
            Assert.Contains(SecureSmtpValidator.Validate(o), e => e.Contains("username"));
        }

        [Fact]
        public void Validate_UsernameWithoutPassword_IsRejected()
        {
            var o = Valid();
            o.Password = "";
            Assert.Contains(SecureSmtpValidator.Validate(o), e => e.Contains("password"));
        }

        [Fact]
        public void Validate_Simulated_SkipsHostAndCredentials()
        {
            var o = new SecureSmtpOptions { IsSimulated = true, Host = "", Username = "u" };
            Assert.Empty(SecureSmtpValidator.Validate(o));
        }

        [Theory]
        [InlineData("localhost", true)]
        [InlineData("127.0.0.1", true)]
        [InlineData("::1", true)]
        [InlineData("smtp.example.com", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsLoopback_ClassifiesHosts(string host, bool expected)
        {
            Assert.Equal(expected, SecureSmtpValidator.IsLoopback(host));
        }

        [Fact]
        public void EnsureValid_ThrowsWithErrors()
        {
            var o = Valid();
            o.UseSsl = false;
            var ex = Assert.Throws<InsecureSmtpConfigurationException>(() => SecureSmtpValidator.EnsureValid(o));
            Assert.NotEmpty(ex.Errors);
        }
    }
}
