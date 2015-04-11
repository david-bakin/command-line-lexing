using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using FakeItEasy;
using FluentAssertions;

using BakinsBits.Utilities;

namespace BakinsBits.UtilitiesTests
{
    public class StringExtensionsTests
    {
        #region FormatWith
        [Fact]
        public void FormatWithCannotHaveNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => ((string)null).FormatWith(null));
        }

        [Fact]
        public void FormatWithWithNoFormatting()
        {
            Assert.Equal("abcd", "abcd".FormatWith(1, "def", new object()));
        }

        [Fact]
        public void FormatWithBasicFormatting()
        {
            Assert.Equal("123abcd456", "{0}abcd{1}".FormatWith(123, "456"));
        }
        #endregion

        #region FormatWithBindings
        [Fact]
        public void FormatWithBindingsCannotHaveNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => ((string)null).FormatWithBindings(null));
        }

        [Fact]
        public void FormatWithBindingsWithNoFormatting()
        {
            Assert.Equal("abcd", "abcd".FormatWithBindings(this));
        }

        [Fact]
        public void FormatWithBindingsSimpleTests()
        {
            Assert.Equal("(1.0,2.0)", "({X:F1},{Y:F1})".FormatWithBindings(new System.Windows.Point(1.0, 2.0)));
                // Anonymous type
            Assert.Equal("abc-123", "{First}-{Second}".FormatWithBindings(new { First = "abc", Second = 123}));
        }
        #endregion

        [Theory]
        [InlineData("foo.bar", "bar", "zebra", "foo.zebra")]
        [InlineData("foo.bar", "bear", "zebra", "foo.bar.zebra")]
        [InlineData("foo.bar.bie", "bie", "ken", "foo.bar.ken")]
        [InlineData("foo.bar.bie", "bar", "ken", "foo.bar.bie.ken")]
        [InlineData("foo", "bar", "ken", "foo.ken")]
        [InlineData("foo.bar", "bar", null, "foo")]
        [InlineData("foo.bar", "bear", null, "foo.bar")]
        [InlineData("junk.bar", null, null, "junk")]
        [InlineData("junk.bar", null, "zebra", "junk.zebra")]
        public void AddOrChangeFileExtensionTests(string fileName, string oldExtension, string newExtension, string expectedResult)
        {
            Assert.Equal(expectedResult, fileName.AddOrChangeFileExtension(oldExtension, newExtension));
        }

        [Theory]
        [InlineData("abcdef", "abc", "def")]
        [InlineData("abcdef", "def", "abcdef")]
        [InlineData("abcdef", "", "abcdef")]
        [InlineData("abcdef", "abcdef", "")]
        public void TrimLeadingNoComparerTests(string str, string prefix, string expectedResult)
        {
            Assert.Equal(expectedResult, str.TrimLeading(prefix));
        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("abc", "", "abc")]
        [InlineData("", "def", "def")]
        [InlineData("abc", null, "abc")]
        [InlineData(null, "def", "def")]
        [InlineData("abc", "def", "abcdef")]
        [InlineData("abcdef", "def", "abcdef")]
        [InlineData("abcde", "def", "abcdedef")]
        [InlineData("def", "def", "def")]
        [InlineData("de", "def", "dedef")]
        [InlineData("ef", "def", "efdef")]
        public void AppendIfMissingStringNoComparerTests(string str, string suffix, string expectedResult)
        {
            Assert.Equal(expectedResult, str.AppendIfMissing(suffix));
        }

        delegate object[] SplitRespectingQuotesTestCase(string input, params string[] results);

        public static IEnumerable<object[]> SplitRespectingQuotesTestCases
        {
            get
            {
                SplitRespectingQuotesTestCase tc = (input, results) => new Object[] {input, results};

                yield return tc("");
                yield return tc("   ");
                yield return tc("abc", "abc");
                yield return tc("   abc   ", "abc");
                yield return tc("abc  def", "abc", "def");
                yield return tc(@"abc  ""def ghi""  jkl", "abc", @"""def ghi""", "jkl");
                yield return tc(@"abc def:""ghi"" jkl", "abc", @"def:""ghi""", "jkl");
                yield return tc(@"""abc"":def ghi ""jkl""", @"""abc"":def", "ghi", @"""jkl""");
            }
        }

        [Theory]
        [MemberData("SplitRespectingQuotesTestCases")]
        public void SplitRespectingQuotesTests(string input, string[] expectedResult)
        {
            Assert.Equal(expectedResult.ToList(), input.SplitRespectingQuotes().ToList());
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("abcdefghijklmnopqrstuvwxyz")]
        public void CollectTests(string input)
        {
            var source = input.ToCharArray().AsEnumerable();
            var result = source.Collect();
            Assert.Equal(input, result);
        }

        [Theory]
        [InlineData("abc", "abc")]
        [InlineData("a\'\"\\\0\a\b\f\n\r\t\vb", @"a\'\""\\\0\a\b\f\n\r\t\vb")]
        [InlineData("a\x00c1\x00b6\x02a3\x21d5\x2591\x2006b", "aÁ¶ʣ⇕░ b")]
        [InlineData("a\x000E\x200F\x202c\x2028b", "a\\x000E\\x200F\\x202C\\x2028b")]
        //[InlineData("a\xe123\xd867b\xdc8a", "a\\xE123\\xD867\\xDC8Ab")] - these fail even though they are private use and surrogates which should expand to hex
        public void ToLiteralFormatStringTests(string input, string expected)
        {
            string result = input.ToLiteralFormat();
            result.Should().Be(expected, "given input \"{0}\"", input);
        }
    }
}
