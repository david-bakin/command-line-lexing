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
        // This file contains the logging mechanism for the state machine in RemoveArgument().

        /// <summary>
        /// The state machine embedded in RemoveArgument() has its own logging
        /// mechanism, for visibility into its mechanism and for debugging purposes;
        /// this is the log entry.
        /// </summary>
        public struct RemoveArgumentStateLogEntry
        {
            public readonly string Message;
            public readonly CppArgumentLexing.ArgStates State;
            public readonly char C;
            public readonly int Backslashes;
            public readonly string Argument;

            public RemoveArgumentStateLogEntry(
                string message,
                CppArgumentLexing.ArgStates state,
                char c,
                int nBackslashes,
                String argument)
            {
                Message = message;
                State = state;
                C = c;
                Backslashes = nBackslashes;
                Argument = argument;
            }

            public override string ToString()
            {
                return String.Format("({0}, {1}, '{2}', {3}, \"{4}\")",
                                     Message, State, C.ToLiteralFormat(), Backslashes, Argument.ToLiteralFormat());
            }
        }

        /// <summary>
        /// The state machine embedded in RemoveArgument() has its own logging
        /// mechanism, for visibility into its mechanism and for debugging purposes;
        /// this is the log collector.
        /// </summary>
        public class RemoveArgumentStateLog : List<RemoveArgumentStateLogEntry>
        {
            public void Add(
                string message,
                CppArgumentLexing.ArgStates state,
                char c,
                int nBackslashes,
                StringBuilder argument)
            {
                this.Add(new RemoveArgumentStateLogEntry(message, state, c, nBackslashes, argument.ToString()));
            }

            public SortedDictionary<CppArgumentLexing.ArgStates, int> HistogramOfLog()
            {
                // (This sorted dictionary will sort the enum keys in order of enum
                // value, not enum name!)
                var histogram = new SortedDictionary<CppArgumentLexing.ArgStates, int>();

                foreach (var e in EnumerateArgStates())
                {
                    histogram[e] = 0;
                }

                foreach (var entry in this)
                {
                    histogram[entry.State]++;
                }

                return histogram;
            }

            public static SortedDictionary<CppArgumentLexing.ArgStates, int>
                MergeHistograms(params SortedDictionary<CppArgumentLexing.ArgStates, int>[] histograms)
            {
                var result = new SortedDictionary<CppArgumentLexing.ArgStates, int>();

                foreach (var e in EnumerateArgStates())
                {
                    result[e] = 0;
                }

                foreach (var histogram in histograms)
                {
                    foreach (var kvp in histogram)
                    {
                        result[kvp.Key] += kvp.Value;
                    }
                }
                return result;
            }

            public override string ToString()
            {
                return ToString(false);
            }

            public string ToString(bool withHistogram)
            {
                var sb = new StringBuilder();
                foreach (var entry in this)
                {
                    sb.AppendLine(entry.ToString());
                }

                if (withHistogram)
                {
                    var histogram = HistogramOfLog();

                    int width = (from e in EnumerateArgStates()
                                 let name = e.ToString()
                                 select name.Length).Max();

                    var format = "{0," + width.ToString() + ":G}: {1,4:D}";

                    sb.AppendLine();
                    foreach (var entry in histogram)
                    {
                        sb.AppendFormat(format, entry.Key, entry.Value);
                        sb.AppendLine();
                    }
                }

                return sb.ToString();
            }

            protected static IEnumerable<CppArgumentLexing.ArgStates> EnumerateArgStates()
            {
                return Enum.GetValues(typeof(CppArgumentLexing.ArgStates))
                           .Cast<CppArgumentLexing.ArgStates>();
            }
        }
    }
}
