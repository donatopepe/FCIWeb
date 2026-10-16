using System;
using FCIWeb.Core.Security;
using Xunit;

namespace FCIWeb.Core.Tests
{
    public class SecurityGuardsTests
    {
        [Theory]
        [InlineData("application/json", true)]
        [InlineData("application/json; charset=utf-8", true)]
        [InlineData("APPLICATION/JSON", true)]
        [InlineData("text/plain", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("   ", false)]
        public void IsJsonContentType_IsNullSafe(string contentType, bool expected)
        {
            Assert.Equal(expected, SecurityGuards.IsJsonContentType(contentType));
        }

        [Fact]
        public void EnsureWithinSize_AllowsUnderLimit()
        {
            SecurityGuards.EnsureWithinSize(100, 1024);
        }

        [Fact]
        public void EnsureWithinSize_ThrowsOverLimit()
        {
            Assert.Throws<SecurityGuardException>(() => SecurityGuards.EnsureWithinSize(2048, 1024));
        }

        [Fact]
        public void EnsureWithinSize_RejectsNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SecurityGuards.EnsureWithinSize(-1, 1024));
        }

        [Theory]
        [InlineData("report.pdf", "report.pdf")]
        [InlineData("../../etc/passwd", "passwd")]
        [InlineData("..\\..\\windows\\system32\\cmd.exe", "cmd.exe")]
        [InlineData("a/b/c.txt", "c.txt")]
        [InlineData("CON", "_CON")]
        [InlineData("....", "")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void SanitizeFileName_NeutralisesTraversalAndReserved(string input, string expected)
        {
            Assert.Equal(expected, SecurityGuards.SanitizeFileName(input));
        }

        [Theory]
        [InlineData("docs/file.txt", true)]
        [InlineData("a/b/c.png", true)]
        [InlineData("../secret", false)]
        [InlineData("..\\secret", false)]
        [InlineData("/etc/passwd", false)]
        [InlineData("C:\\Windows\\x", false)]
        [InlineData("a/../../b", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsSafeRelativePath_RejectsTraversalAndAbsolute(string path, bool expected)
        {
            Assert.Equal(expected, SecurityGuards.IsSafeRelativePath(path));
        }

        [Fact]
        public void CombineUnderRoot_ResolvesInsideRoot()
        {
            var result = SecurityGuards.CombineUnderRoot("/tmp/root", "a/b.txt");
            Assert.Equal("/tmp/root/a/b.txt", result);
        }

        [Fact]
        public void CombineUnderRoot_RejectsTraversal()
        {
            Assert.Throws<SecurityGuardException>(() => SecurityGuards.CombineUnderRoot("/tmp/root", "../escape"));
        }
    }
}
