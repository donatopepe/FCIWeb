using FCIWeb.Core.Utility;
using Xunit;

namespace FCIWeb.Core.Tests
{
    public class ExtensionsTests
    {
        [Theory]
        [InlineData("AB", "ab", true)]
        [InlineData("a b", "AB", true)]
        [InlineData("Hello World", "helloworld", true)]
        [InlineData("abc", "abd", false)]
        public void EqualsNormalized_IgnoresSpacesAndCase(string a, string b, bool expected)
        {
            Assert.Equal(expected, a.EqualsNormalized(b));
        }

        [Fact]
        public void EqualsNormalized_BothNull_AreEqual()
        {
            string a = null;
            Assert.True(a.EqualsNormalized(null));
        }

        [Theory]
        [InlineData("Hello World", "lowo", true)]
        [InlineData("Hello World", "xyz", false)]
        [InlineData("HelloWorld", "hello", true)]
        public void ContainsNormalized_IgnoresSpacesAndCase(string a, string b, bool expected)
        {
            Assert.Equal(expected, a.ContainsNormalized(b));
        }

        [Fact]
        public void ContainsNormalized_NullSource_IsSafe()
        {
            string a = null;
            Assert.False(a.ContainsNormalized("x"));
            Assert.True(a.ContainsNormalized(null));
        }

        [Fact]
        public void ContainsNormalized_EmptyNeedle_IsTrue()
        {
            Assert.True("anything".ContainsNormalized(""));
        }
    }
}
