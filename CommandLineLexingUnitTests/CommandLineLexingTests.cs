using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using FakeItEasy;
using FluentAssertions;

using BakinsBits.CommandLineLexing;
using BakinsBits.CommandLineLexing.Details;
using BakinsBits.Utilities;

namespace BakinsBits.UtilitiesUnitTests
{
    public class CommandLineSplittingDetailsTests
    {
        ITestOutputHelper output_;

        public CommandLineSplittingDetailsTests(ITestOutputHelper output)
        {
            output_ = output;
        }

        [Fact]
        public void TakeUntilNulNoNulTest()
        {
            var source = "abcd".ToCharArray().AsEnumerable();
            var resultStream = source.TakeUntilNul();
            var result = resultStream.Collect();
            Assert.Equal("abcd", result);
        }

        [Fact]
        public void TakeUntilNulWithNulAtEndTest()
        {
            var source = "abcd\0".ToCharArray().AsEnumerable();
            var resultStream = source.TakeUntilNul();
            var result = resultStream.Collect();
            Assert.Equal("abcd", result);
        }

        [Fact]
        public void TakeUntilNulWithNulInMiddleTest()
        {
            var source = "abcd\0efgh".ToCharArray().AsEnumerable();
            var resultStream = source.TakeUntilNul();
            var result = resultStream.Collect();
            Assert.Equal("abcd", result);
        }

        [Fact]
        public void TakeUntilNulWithNulFirstTest()
        {
            var source = "\0efgh".ToCharArray().AsEnumerable();
            var resultStream = source.TakeUntilNul();
            var result = resultStream.Collect();
            Assert.Equal("", result);
        }

        [Theory]
        [InlineData("", "", "")]
        // no quoting
        [InlineData(" xyz", "", " xyz")]
        [InlineData("xyz", "xyz", "")]
        [InlineData("xyz def", "xyz", " def")]
        [InlineData("xyz\tdef", "xyz", "\tdef")]
        [InlineData("xyz def ghi", "xyz", " def ghi")]
        [InlineData(@"C:\b\xray.exe junk zebra", @"C:\b\xray.exe", " junk zebra")]
        [InlineData(@"C:\\b\\xray.exe junk zebra", @"C:\\b\\xray.exe", " junk zebra")]
        [InlineData(@"\\server\share\dir\exe.exe junk zebra", @"\\server\share\dir\exe.exe", " junk zebra")]
        // quoting
        [InlineData("\"xyz\"", "xyz", "")]
        [InlineData("\"xyz\" junk zebra", "xyz", " junk zebra")]
        [InlineData("xy\"zw\"uv junk zebra", "xyzwuv", " junk zebra")]
        [InlineData("\"xyz", "xyz", "")]
        [InlineData("\"xyz junk zebra", "xyz junk zebra", "")]
        [InlineData("\"xyz\"\"def junk zebra", "xyzdef junk zebra", "")]
        [InlineData("\"xyz\"\"def\" junk zebra", "xyzdef", " junk zebra")]
        public void TakeFirstArgumentTests(string commandLine, string expectedArgument, string expectedRemainingLine)
        {
            var commandLineStream = commandLine.ToCharArray().AsEnumerable();
            var firstArgument = new StringBuilder();
            var remainingLine = commandLine.RemoveFirstArgument(firstArgument).Collect();
            firstArgument.ToString().Should().Be(expectedArgument, "commandLine was \"{0}\"", commandLine.ToLiteralFormat());
            remainingLine.Should().Be(expectedRemainingLine, "commandLine was \"{0}\"", commandLine.ToLiteralFormat());
        }

        [Fact]
        public void TakeArgumentStateLogInitTest()
        {
            var sut = new RemoveArgumentStateLog();
            var initHisto = sut.ToString(true);

            const string expected = @"
                                             Start:    0
                                 LeadingWhitespace:    0
                  ScanningBackslashesOutsideQuotes:    0
                QuoteAfterBackslashesOutsideQuotes:    0
                  EmittingBackslashesOutsideQuotes:    0
                  CheckForArgumentEndOutsideQuotes:    0
 EmittingBackslashesOutsideQuotesGoingInsideQuotes:    0
                   ScanningBackslashesInsideQuotes:    0
                 QuoteAfterBackslashesInsideQuotes:    0
                   EmittingBackslashesInsideQuotes:    0
     CheckForTwoQuotesAfterBackslashesInsideQuotes:    0
 EmittingBackslashesInsideQuotesGoingOutsideQuotes:    0
 CheckForArgumentEndInsideQuotesGoingOutsideQuotes:    0
EmittingBackslashesInsideQuotesStayingInsideQuotes:    0
                  EmitFinalBackslashesInsideQuotes:    0
                                         CopyToEnd:    0
                                               End:    0
";
            Assert.Equal(expected, initHisto);
        }

        [Fact]
        public void TakeArgumentStateLogWithLoggingTest()
        {
            var sut = new RemoveArgumentStateLog();
            var sb = new StringBuilder();
            sb.Append('x');
            sut.Add("Start-a-0-x", CppArgumentLexing.ArgStates.Start, 'a', 0, sb);
            sb.Append('y');
            sut.Add("LeadingWhitespace-b-1-xy", CppArgumentLexing.ArgStates.LeadingWhitespace, 'b', 1, sb);
            sb.Append('z');
            sut.Add("EmitFinalBackslashesInsideQuotes-c-2-xyz", CppArgumentLexing.ArgStates.EmitFinalBackslashesInsideQuotes, 'c', 2, sb);
            sb.Append('w');
            sut.Add("LeadingWhitespace-d-3-xyzw", CppArgumentLexing.ArgStates.LeadingWhitespace, 'd', 3, sb);
            var log = sut.ToString();
            output_.WriteLine(log);
            const string expectedLog = @"(Start-a-0-x, Start, 'a', 0, ""x"")
(LeadingWhitespace-b-1-xy, LeadingWhitespace, 'b', 1, ""xy"")
(EmitFinalBackslashesInsideQuotes-c-2-xyz, EmitFinalBackslashesInsideQuotes, 'c', 2, ""xyz"")
(LeadingWhitespace-d-3-xyzw, LeadingWhitespace, 'd', 3, ""xyzw"")
";
            Assert.Equal(expectedLog, log);

            var histo = sut.ToString(true);
            const string expectedHisto = expectedLog + @"
                                             Start:    1
                                 LeadingWhitespace:    2
                  ScanningBackslashesOutsideQuotes:    0
                QuoteAfterBackslashesOutsideQuotes:    0
                  EmittingBackslashesOutsideQuotes:    0
                  CheckForArgumentEndOutsideQuotes:    0
 EmittingBackslashesOutsideQuotesGoingInsideQuotes:    0
                   ScanningBackslashesInsideQuotes:    0
                 QuoteAfterBackslashesInsideQuotes:    0
                   EmittingBackslashesInsideQuotes:    0
     CheckForTwoQuotesAfterBackslashesInsideQuotes:    0
 EmittingBackslashesInsideQuotesGoingOutsideQuotes:    0
 CheckForArgumentEndInsideQuotesGoingOutsideQuotes:    0
EmittingBackslashesInsideQuotesStayingInsideQuotes:    0
                  EmitFinalBackslashesInsideQuotes:    1
                                         CopyToEnd:    0
                                               End:    0
";
            Assert.Equal(expectedHisto, histo);

        }

        [Theory]
        [InlineData("", "", "")]
        [InlineData("   ", "", "")]
        [InlineData("a", "a", "")]
        [InlineData("abc", "abc", "")]
        [InlineData("a ", "a", " ")]
        [InlineData("a b", "a", " b")]
        [InlineData("abc d", "abc", " d")]
        // From MSDN https://msdn.microsoft.com/en-us/library/17w5ykft(v=vs.120).aspx :
        [InlineData("\"abc\" d", "abc", " d")]
        [InlineData(" d", "d", "")]
        [InlineData(@"a\\b d""e f""g h", @"a\\b", @" d""e f""g h")]
        [InlineData(@" d""e f""g h", "de fg", " h")]
        [InlineData(@"a\\\""b c d", @"a\""b", " c d")]
        [InlineData(@"a\\\\""b c"" d e", @"a\\b c", " d e")]
        // From D Deley http://www.daviddeley.com/autohotkey/parameters/parameters.htm 5.4:
        [InlineData("CallMeIshmael", "CallMeIshmael", "")]
        [InlineData(@"""Call Me Ishmael""", "Call Me Ishmael", "")]
        [InlineData(@"Cal""l Me I""shmael", "Call Me Ishmael", "")]
        [InlineData(@"CallMe\""Ishmael", @"CallMe""Ishmael", "")]
        [InlineData(@"""CallMe\""Ishmael""", @"CallMe""Ishmael", "")]
        [InlineData(@"""Call Me Ishmael\\""", @"Call Me Ishmael\", "")]
        [InlineData(@"""CallMe\\\""Ishmael""", @"CallMe\""Ishmael", "")]
        [InlineData(@"a\\\b", @"a\\\b", "")]
        [InlineData(@"""a\\\b""", @"a\\\b", "")]
        // From D Deley http://www.daviddeley.com/autohotkey/parameters/parameters.htm 5.5:
        [InlineData(@"""\""Call Me Ishmael\""""", @"""Call Me Ishmael""", "")]
        [InlineData(@"""C:\TEST A\\""", @"C:\TEST A\", "")]
        [InlineData(@"""\""C:\TEST A\\\""""", @"""C:\TEST A\""", "")]
        // From D Deley http://www.daviddeley.com/autohotkey/parameters/parameters.htm 5.7:
        [InlineData(@"""a b c""""", @"a b c""", "")]
        [InlineData(@"""""""CallMeIshmael"""""" b c", @"""CallMeIshmael""", " b c")]
        [InlineData(@"""""""Call Me Ishmael""""""", @"""Call Me Ishmael""", "")]
        [InlineData(@"""""""""Call Me Ishmael"""" b c", @"""Call", @" Me Ishmael"""" b c")]
        // Complete code coverage:
        [InlineData(@"abc""def\\\\"""" ghi"" jkl", @"abcdef\\"" ghi", " jkl")]
        [InlineData(@"abc""def\\\\"" ghi"" jkl", @"abcdef\\", @" ghi"" jkl")]
        public void TakeArgumentTests(string input, string expectedArgument, string expectedRemainingLine)
        {
            var argumentStream = input.ToCharArray().AsEnumerable();
            var argumentValue = new StringBuilder();
            var log = new RemoveArgumentStateLog();
            var remainingLine = input.RemoveArgument(argumentValue, log).Collect();
            output_.WriteLine(log.ToString(true));
            argumentValue.ToString().Should().Be(expectedArgument, "arg failed, input was \"{0}\"", input.ToLiteralFormat());
            remainingLine.Should().Be(expectedRemainingLine, "remaining failed, input was \"{0}\"", input.ToLiteralFormat());
        }
    }

    public class CommandLineSplittingTests
    {
        public readonly Lexer splitter;

        public CommandLineSplittingTests()
        {
            splitter = new Lexer();
        }

        delegate object[] CommandLineSplitTestCase(string input, string exeName, params string[] args);

        public static IEnumerable<object[]> CommandLineToExeAndArgsWin32TestCases
        {
            get
            {
                CommandLineSplitTestCase tc = (input, exeName, results) => new Object[] { input, exeName, results };

                yield return tc("foo", "foo");
                yield return tc("foo bar bear", "foo", "bar", "bear");
                yield return tc("foo    bar     bear    ", "foo", "bar", "bear");
                yield return tc(@"foo ""bar bear""", "foo", "bar bear");

                // 4 examples from Old New Thing:
                yield return tc(@"program.exe ""hello there.txt""", "program.exe", "hello there.txt");
                yield return tc(@"program.exe ""C:\Hello there.txt""", "program.exe", @"C:\Hello there.txt");
                yield return tc(@"program.exe ""hello\""there""", "program.exe", @"hello""there");
                yield return tc(@"program.exe ""hello\\""", "program.exe", @"hello\");

                // 4 examples from comments at Old New Thing showing quotes - but
                // these examples work as described only for arguments, not for the
                // exename which has different rules!:
                yield return tc(@"x foo""bar", "x", "foobar");
                yield return tc(@"x foo""""bar", "x", "foobar");
                yield return tc(@"x foo""""""bar", "x", @"foo""bar");
                yield return tc(@"x foo""x""""bar", "x", @"foox""bar");

                yield return tc(@"foo""bar", @"foo""bar");
                yield return tc(@"foo""""bar", @"foo""""bar");
                yield return tc(@"foo""""""bar", @"foo""""""bar");
                yield return tc(@"foo""x""""bar", @"foo""x""""bar");

                // If line starts with one or more spaces then the exeName is empty
                // and all the tokens are args
                yield return tc(@"   spaces are  here  and there  ", "", "spaces", "are", "here", "and", "there");
            }
        }

        [Theory]
        [MemberData("CommandLineToExeAndArgsWin32TestCases")]
        public void CommandLineToExeAndArgsWin32Test(string input, string exeName, string[] args)
        {
            var actual = splitter.CommandLineToExeAndArgsWin32(input);
            Assert.Equal(exeName, actual.Item1);
            Assert.Equal(args.ToList(), actual.Item2.ToList());
        }

        public static IEnumerable<object[]> CommandLineToExeAndArgsCppTestCases
        {
            get
            {
                CommandLineSplitTestCase tc = (input, exeName, results) => new Object[] { input, exeName, results };

                yield return tc("foo", "foo");
                yield return tc("foo bar bear", "foo", "bar", "bear");
                yield return tc("foo    bar     bear    ", "foo", "bar", "bear");
                yield return tc(@"foo ""bar bear""", "foo", "bar bear");

                // 4 examples from Old New Thing:
                yield return tc(@"program.exe ""hello there.txt""", "program.exe", "hello there.txt");
                yield return tc(@"program.exe ""C:\Hello there.txt""", "program.exe", @"C:\Hello there.txt");
                yield return tc(@"program.exe ""hello\""there""", "program.exe", @"hello""there");
                yield return tc(@"program.exe ""hello\\""", "program.exe", @"hello\");

                // 4 examples from comments at Old New Thing showing quotes - but
                // these examples work as described only for arguments, not for the
                // exename which has different rules!:
                yield return tc(@"x foo""bar", "x", "foobar");
                yield return tc(@"x foo""""bar", "x", "foobar");
                yield return tc(@"x foo""""""bar", "x", @"foo""bar");
                yield return tc(@"x foo""x""""bar", "x", @"foox""bar");

                // If line starts with one or more spaces then the exeName is empty
                // and all the tokens are args
                yield return tc(@"   spaces are  here  and there  ", "", "spaces", "are", "here", "and", "there");
            }
        }

        [Theory]
        [MemberData("CommandLineToExeAndArgsCppTestCases")]
        public void CommandLineToExeAndArgsCppTest(string input, string exename, string[] args)
        {
            var actual = splitter.CommandLineToExeAndArgsCpp(input);
            Assert.Equal(exename, actual.Item1);
            Assert.Equal(args.ToList(), actual.Item2.ToList());
        }

        [Theory]
        [InlineData("a", "a")]
        [InlineData("abc", "abc")]
        [InlineData("a c", @"""a c""")]
        [InlineData("a  c", @"""a  c""")]
        [InlineData("a\tc", "\"a\tc\"")]
        [InlineData(@"\abc", @"""\abc""")]
        [InlineData(@"a\bc", @"""a\bc""")]
        [InlineData(@"abc\", @"""abc\\""")]
        public void QuotifyTest(string input, string expected)
        {
            var actual = input.Quotify();
            Assert.Equal(expected, actual);
        }
    }
}
