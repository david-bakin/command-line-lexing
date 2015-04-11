using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BakinsBits.Utilities;

namespace BakinsBits.CommandLineLexing
{
    namespace Details
    {
        public static class CppArgumentLexing
        {
            enum FirstArgStates { OutsideQuotes, InsideQuotes, CopyToEnd };

            /// <summary>
            /// Grab first parameter off of incoming stream, then pass all the rest
            /// of the characters unchanged.  First parameter has a simple rule:
            /// double-quotes are not copied but change the state from outside-quote
            /// to inside-quote and back.  The first parameter ends at a space or tab
            /// outside of a quote, or at a nul char ('\0') or end of stream.
            /// </summary>
            /// <remarks>
            /// By the above rule there is no way to get a double-quote into the
            /// first argument.  This is ok because the first argument is always
            /// either an executable filename or full path, and double-quote isn't
            /// allowed in a a filename or path in NTFS.
            ///
            /// The /not/-out parameter argument is sort of ugly.  (Also at RemoveArgument
            /// and even worse at RemoveRemainingArguments.)  It was meant (here) to
            /// be an out string, but unfortunately, C# iterators (blocks with yield
            /// statements that the compiler turns into state machines) can't have
            /// ref or out parameters.
            /// </remarks>
            public static IEnumerable<char> RemoveFirstArgument(
                this IEnumerable<char> source,
                /*out*/ StringBuilder argument)
            {
                FirstArgStates state = FirstArgStates.OutsideQuotes;
                foreach (char c in source)
                {
                    switch (state)
                    {
                        case FirstArgStates.OutsideQuotes:
                        case FirstArgStates.InsideQuotes:
                            if ('"' == c)
                            {
                                state = FirstArgStates.InsideQuotes == state
                                            ? FirstArgStates.OutsideQuotes
                                            : FirstArgStates.InsideQuotes;
                            }
                            else
                            {
                                argument.Append(c);
                            }

                            if (FirstArgStates.InsideQuotes == state)
                            {
                                continue;
                            }

                            if (' ' == c || '\t' == c)
                            {
                                state = FirstArgStates.CopyToEnd;
                                argument.Length -= 1;
                                yield return c;
                            }
                            break;

                        case FirstArgStates.CopyToEnd:
                            yield return c;
                            break;
                    }
                }
            }

            public enum ArgStates {
                Start,
                LeadingWhitespace,
                ScanningBackslashesOutsideQuotes,
                QuoteAfterBackslashesOutsideQuotes,
                EmittingBackslashesOutsideQuotes,
                CheckForArgumentEndOutsideQuotes,
                EmittingBackslashesOutsideQuotesGoingInsideQuotes,
                ScanningBackslashesInsideQuotes,
                QuoteAfterBackslashesInsideQuotes,
                EmittingBackslashesInsideQuotes,
                CheckForTwoQuotesAfterBackslashesInsideQuotes,
                EmittingBackslashesInsideQuotesGoingOutsideQuotes,
                CheckForArgumentEndInsideQuotesGoingOutsideQuotes,
                EmittingBackslashesInsideQuotesStayingInsideQuotes,
                EmitFinalBackslashesInsideQuotes,
                CopyToEnd,
                End
            };

            /// <summary>
            /// Grab a parameter (_not_ the first) off of incoming stream, then pass
            /// all the rest of the characters unchanged.  Quoting rules are
            /// complicated, see http://www.daviddeley.com/autohotkey/parameters/parameters.htm#WINCRULES
            /// and https://msdn.microsoft.com/en-us/library/17w5ykft(v=vs.120).aspx
            /// for details.
            /// </summary>
            /// <remarks>
            /// Engineered from the D.Deley article above into a state machine.
            /// </remarks>
            public static IEnumerable<char> RemoveArgument(
                this IEnumerable<char> source,
                StringBuilder argument,
                /*out*/ RemoveArgumentStateLog log = null)
            {
                ArgStates state = ArgStates.Start;
                int nBackslashes = 0;

                foreach (char c in source)
                {
                    if (null != log) log.Add("fetchChar", state, c, nBackslashes, argument);
                    goto stateAction;

                reinspectChar:
                    if (null != log) log.Add("reinspectChar", state, c, nBackslashes, argument);
                    goto stateAction;

                stateAction:
                    switch (state)
                    {
                        case ArgStates.Start:
                            state = ArgStates.LeadingWhitespace;
                            goto reinspectChar;

                        case ArgStates.LeadingWhitespace:
                            if (' ' == c || '\t' == c)
                            {
                                continue;
                            }
                            else
                            {
                                state = ArgStates.ScanningBackslashesOutsideQuotes;
                                nBackslashes = 0;
                                goto reinspectChar;
                            }

                        case ArgStates.ScanningBackslashesOutsideQuotes:
                            if ('\\' == c)
                            {
                                nBackslashes++;
                                continue;
                            }
                            else if ('\"' == c)
                            {
                                state = ArgStates.QuoteAfterBackslashesOutsideQuotes;
                                goto reinspectChar;
                            }
                            else
                            {
                                state = ArgStates.EmittingBackslashesOutsideQuotes;
                                goto reinspectChar;
                            }

                        case ArgStates.QuoteAfterBackslashesOutsideQuotes:
                            {
                                bool oddBackslashes = 1 == nBackslashes % 2;
                                nBackslashes /= 2;
                                if (oddBackslashes)
                                {
                                    state = ArgStates.EmittingBackslashesOutsideQuotes;
                                    goto reinspectChar;
                                }
                                else
                                {
                                    state = ArgStates.EmittingBackslashesOutsideQuotesGoingInsideQuotes;
                                    continue;
                                }
                            }

                        case ArgStates.EmittingBackslashesOutsideQuotes:
                            argument.Append('\\', nBackslashes);
                            nBackslashes = 0;
                            state = ArgStates.CheckForArgumentEndOutsideQuotes;
                            goto reinspectChar;

                        case ArgStates.CheckForArgumentEndOutsideQuotes:
                            if (' ' == c || '\t' == c)
                            {
                                state = ArgStates.CopyToEnd;
                                goto reinspectChar;
                            }
                            else
                            {
                                argument.Append(c);
                                nBackslashes = 0;
                                state = ArgStates.ScanningBackslashesOutsideQuotes;
                                continue;
                            }

                        case ArgStates.EmittingBackslashesOutsideQuotesGoingInsideQuotes:
                            argument.Append('\\', nBackslashes);
                            nBackslashes = 0;
                            state = ArgStates.ScanningBackslashesInsideQuotes;
                            goto reinspectChar;

                        case ArgStates.ScanningBackslashesInsideQuotes:
                            if ('\\' == c)
                            {
                                nBackslashes++;
                                continue;
                            }
                            else if ('\"' == c)
                            {
                                state = ArgStates.QuoteAfterBackslashesInsideQuotes;
                                goto reinspectChar;
                            }
                            else
                            {
                                state = ArgStates.EmittingBackslashesInsideQuotes;
                                goto reinspectChar;
                            }

                        case ArgStates.QuoteAfterBackslashesInsideQuotes:
                            {
                                bool oddBackslashes = 1 == nBackslashes % 2;
                                nBackslashes /= 2;
                                if (oddBackslashes)
                                {
                                    state = ArgStates.EmittingBackslashesInsideQuotes;
                                    goto reinspectChar;
                                }
                                else
                                {
                                    state = ArgStates.CheckForTwoQuotesAfterBackslashesInsideQuotes;
                                    continue;
                                }
                            }

                        case ArgStates.EmittingBackslashesInsideQuotes:
                            argument.Append('\\', nBackslashes).Append(c);
                            nBackslashes = 0;
                            state = ArgStates.ScanningBackslashesInsideQuotes;
                            continue;

                        case ArgStates.CheckForTwoQuotesAfterBackslashesInsideQuotes:
                            if ('\"' == c)
                            {
                                state = ArgStates.EmittingBackslashesInsideQuotesStayingInsideQuotes;
                                goto reinspectChar;
                            }
                            else
                            {
                                state = ArgStates.EmittingBackslashesInsideQuotesGoingOutsideQuotes;
                                goto reinspectChar;
                            }

                        case ArgStates.EmittingBackslashesInsideQuotesGoingOutsideQuotes:
                            argument.Append('\\', nBackslashes);
                            nBackslashes = 0;
                            state = ArgStates.CheckForArgumentEndInsideQuotesGoingOutsideQuotes;
                            goto reinspectChar;

                        case ArgStates.EmittingBackslashesInsideQuotesStayingInsideQuotes:
                            argument.Append('\\', nBackslashes).Append('\"');
                            nBackslashes = 0;
                            state = ArgStates.ScanningBackslashesInsideQuotes;
                            continue;

                        case ArgStates.CheckForArgumentEndInsideQuotesGoingOutsideQuotes:
                            if (' ' == c || '\t' == c)
                            {
                                state = ArgStates.CopyToEnd;
                                goto reinspectChar;
                            }
                            else
                            {
                                argument.Append(c);
                                nBackslashes = 0;
                                state = ArgStates.ScanningBackslashesOutsideQuotes;
                                continue;
                            }

                        case ArgStates.CopyToEnd:
                            yield return c;
                            state = ArgStates.CopyToEnd;
                            continue;
                    }
                }

                // Flush final backslashes if any (there's only one state where this is necessary)
                if (ArgStates.CheckForTwoQuotesAfterBackslashesInsideQuotes == state)
                {
                    if (null != log) log.Add("flushing final backslashes", state, '¶', nBackslashes, argument);
                    argument.Append('\\', nBackslashes);
                }

                if (null != log) log.Add("final result", ArgStates.End, '¶', 0, argument);
            }

            /// <summary>
            /// Given a stream lex out all of the remaining command line parameters,
            /// VC++ style.  User provides an IList instance to hold the arguments
            /// found.
            /// </summary>
            /// <param name="max">Maximum number of command line parameters allowed
            /// (not counting exename at start of line)</param>
            /// <remarks>
            /// See above at RemoveFirstArgument for explanation of the /not/-out
            /// parameter arguments.
            ///
            /// Unfortunately I couldn't figure out how to dynamically extend the
            /// chain of iterators without restarting the iterator.  So instead I
            /// create a chain of fixed, large, length.  Most are never used, but it
            /// does mean extra instances are created (and then are garbage as
            /// soon as this method returns). A possible solution would be to change
            /// RemoveArgument() so that instead of a StringBuilder it takes an
            /// IList<string> and an index, and assigns its parameter into the
            /// list slot it is given.
            /// </remarks>
            static public void RemoveRemainingArguments(
                this IEnumerable<char> commandLine,
                /*out*/ IList<string> arguments,
                int max = 250)
            {
                var fullCommandLineStream = commandLine.AsEnumerable().TakeUntilNul();

                var firstArgument = new StringBuilder();
                var argumentBuilders = Enumerable.Range(0, max).Select(_ => new StringBuilder()).ToList();

                var remainingCommandLineStream =
                    argumentBuilders.Aggregate(fullCommandLineStream, (stream, ab) => stream.RemoveArgument(ab));
                char c = remainingCommandLineStream.FirstOrDefault();
                if ('\0' != c)
                {
                    throw new InvalidOperationException("more than 250 arguments");
                }

                // Remove trailing empty strings from the fixed-length list of StringBuilders.
                var args = argumentBuilders.Select(ab => ab.ToString())
                                           .Reverse()
                                           .SkipWhile(s => s.IsNullOrEmpty())
                                           .Reverse();
                foreach (var arg in args)
                {
                    arguments.Add(arg);
                }
            }

            /// <summary>
            /// Take characters from a stream until the first nul ('\0') character,
            /// then stop.
            /// </summary>
            public static IEnumerable<char> TakeUntilNul(this IEnumerable<char> source)
            {
                return source.TakeWhile(c => '\0' != c);
            }
        }

    }
}
