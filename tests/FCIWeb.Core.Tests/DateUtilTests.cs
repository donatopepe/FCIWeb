using System;
using System.Globalization;
using FCIWeb.Core.Utility;
using Xunit;

namespace FCIWeb.Core.Tests
{
    public class DateUtilTests
    {
        private readonly DateUtil _sut = new DateUtil();

        [Fact]
        public void DataToText_Null_ReturnsEmpty()
        {
            Assert.Equal(String.Empty, _sut.DataToText(null));
        }

        [Fact]
        public void DataToText_ValidDate_FormatsInvariant()
        {
            var d = new DateTime(2024, 1, 5);
            Assert.Equal("05/01/2024", _sut.DataToText(d));
        }

        [Fact]
        public void DataToText_TooOld_ReturnsEmpty()
        {
            var tooOld = DateTime.Now.AddYears(-121);
            Assert.Equal(String.Empty, _sut.DataToText(tooOld));
        }

        [Fact]
        public void TextToDate_EmptyOrWhitespace_ReturnsNull()
        {
            Assert.Null(_sut.TextToDate(null));
            Assert.Null(_sut.TextToDate(""));
            Assert.Null(_sut.TextToDate("   "));
        }

        [Theory]
        [InlineData("050124", 2024, 1, 5)]      // ddMMyy
        [InlineData("05012024", 2024, 1, 5)]    // ddMMyyyy
        [InlineData("05/01/2024", 2024, 1, 5)]  // dd/MM/yyyy
        [InlineData("05/01/24", 2024, 1, 5)]    // dd/MM/yy
        public void TextToDate_SupportedFormats(string input, int y, int m, int d)
        {
            Assert.Equal(new DateTime(y, m, d), _sut.TextToDate(input));
        }

        [Fact]
        public void TextToDate_Invalid_ReturnsNull()
        {
            Assert.Null(_sut.TextToDate("not-a-date"));
        }

        [Fact]
        public void TextToDate_TooOld_ReturnsNull()
        {
            var tooOld = DateTime.Now.AddYears(-121).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            Assert.Null(_sut.TextToDate(tooOld));
        }

        [Fact]
        public void RoundTrip_DataToText_TextToDate()
        {
            var d = new DateTime(2023, 12, 31);
            var text = _sut.DataToText(d);
            Assert.Equal(d, _sut.TextToDate(text));
        }
    }
}
