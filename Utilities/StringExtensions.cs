using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;

namespace BakinsBits.Utilities
{
    /// <summary>
    /// Extension methods on string to make some common things more convenient.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Postfix form for string.Format()
        /// </summary>
        public static string FormatWith(this string format, params object[] args)
        {
            format.MustNotBeNull("format");
            return string.Format(format, args);
        }

        /// <summary>
        /// Alternative (and postfix form) to string.Format() which allows formatting by
        /// naming properties or fields of a single object.
        /// </summary>
        /// <remarks>
        /// From http://james.newtonking.com/archive/2008/03/29/formatwith-2-0-string-formatting-with-named-variables
        /// modified slightly to modernize it.  Uses System.Web.Ui.DataBinder.
        /// (Unnecessary in C# 6.0+ (which has string interpolation).)
        /// </remarks>
        public static string FormatWithBindings(this string format, object source)
        {
            return FormatWithBindings(format, null, source);
        }

        /// <summary>
        /// Alternative (and postfix form) to string.Format() which allows formatting by
        /// naming properties or fields of a single object.
        /// </summary>
        /// <remarks>
        /// From http://james.newtonking.com/archive/2008/03/29/formatwith-2-0-string-formatting-with-named-variables
        /// modified slightly to modernize it.  Uses System.Web.Ui.DataBinder.
        /// (Unnecessary in C# 6.0+ (which has string interpolation).)
        /// </remarks>
        public static string FormatWithBindings(this string format, IFormatProvider provider, object source)
        {
            format.MustNotBeNull("format");

            var rx = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
                               RegexOptions.Compiled|RegexOptions.CultureInvariant|RegexOptions.IgnoreCase);

            List<object> values = new List<object>();
            string rformat = rx.Replace(format, m => {
                var startG = m.Groups["start"];
                var propG = m.Groups["property"];
                var formatG = m.Groups["format"];
                var endG = m.Groups["end"];

                values.Add(("0" == propG.Value) ? source : DataBinder.Eval(source, propG.Value));

                return new string('{', startG.Captures.Count)
                         + (values.Count-1)
                         + formatG.Value
                     + new string('}', endG.Captures.Count);
            });

            return string.Format(provider, rformat, values.ToArray());
        }

        /// <summary>
        /// LINQ-style extension method to return all strings after a given string
        /// in a string sequence.
        /// </summary>
        public static IEnumerable<string> SkipUntilAfter(
            this IEnumerable<string> seq,
            string element,
            StringComparison comparer = StringComparison.Ordinal)
        {
            return seq.SkipWhile(s => !(s.Equals(element, comparer))).Skip(1);
        }

        /// <summary>
        /// LINQ-style extension method to return all strings up to (but not including)
        /// a given string in a string sequence.
        /// </summary>
        public static IEnumerable<string> TakeUntil(
            this IEnumerable<string> seq,
            string element,
            StringComparison comparer = StringComparison.Ordinal)
        {
            return seq.TakeWhile(s => !(s.Equals(element, comparer)));
        }

        /// <summary>
        /// LINQ-style extension method to return all strings between (but not including)
        /// two delimiter strings in a string sequence.
        /// </summary>
        public static IEnumerable<string> TakeBetween(
            this IEnumerable<string> seq,
            string beforeFirstWantedElement,
            string afterLastWantedElement,
            StringComparison comparer = StringComparison.Ordinal)
        {
            return seq.SkipUntilAfter(beforeFirstWantedElement, comparer)
                      .TakeUntil(afterLastWantedElement, comparer);
        }

        //splits a string on the specified separator, then trims the array elements
        public static string[] SplitAndTrim(this string str, params char[] separator)
        {
            string[] array = str.Split(separator);
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = array[i].Trim();
            }
            return array;
        }

        /// <summary>
        /// Return a string with a given prefix chopped off.  If the string doesn't
        /// begin with the prefix, just returns the string unchanged.
        /// </summary>
        public static string TrimLeading(
            this string str,
            string prefix,
            StringComparison comparer = StringComparison.Ordinal)
        {
            str.MustNotBeNull("str");
            prefix.MustNotBeNull("prefix");

            var result = str;
            if (str.StartsWith(prefix, comparer))
            {
                result = str.Substring(prefix.Length);
            }
            return result;
        }

        /// <summary>
        /// Given a filename (with or without extension) and an old and a new
        /// extension, return a filename with the new extension - replacing the
        /// filename's old extension iff it was the <i>given</i> old extension.
        /// If new extension is null then the fileName's extension is removed
        /// if it matches the old extension.
        /// </summary>
        public static string AddOrChangeFileExtension(
            this string fileName,
            string oldExtension,
            string newExtension)
        {
            fileName.MustNotBeNullOrEmpty("fileName");

            if (!oldExtension.IsNullOrEmpty() && !oldExtension.StartsWith("."))
            {
                oldExtension = '.' + oldExtension;
            }

            if (!newExtension.IsNullOrEmpty() && !newExtension.StartsWith("."))
            {
                newExtension = '.' + newExtension;
            }

            // If new extension is null then remove extension iff it is the same
            // as oldExtension (or if oldExtension is null or empty)
            if (null == newExtension)
            {
                if (Path.HasExtension(fileName))
                {
                    if (oldExtension.IsNullOrEmpty())
                        return Path.GetFileNameWithoutExtension(fileName);
                    if (String.Equals(Path.GetExtension(fileName), oldExtension, StringComparison.OrdinalIgnoreCase))
                        return Path.GetFileNameWithoutExtension(fileName);
                }
                return fileName;
            }

            // Otherwise, add the new extension as the filename's extension,
            // unless the filename's extension was the old extension, in which
            // case, replace it.
            if (Path.HasExtension(fileName) &&
                !oldExtension.IsNullOrEmpty() &&
                !String.Equals(Path.GetExtension(fileName), oldExtension, StringComparison.OrdinalIgnoreCase))
            {
                // If the current extension isn't the old extension we want to
                // add the new extension, not replace the old extension
                var result = fileName + newExtension;
                return result;
            }
            return Path.ChangeExtension(fileName, newExtension);
        }

        /// <summary>
        /// Indicates whether the specified string is null or a System.String.Empty string.
        /// </summary>
        /// <remarks>
        /// Why didn't they add this extension method when they first introduced
        /// extension methods?  It's too late now: Everyone has defined their own.
        /// </remarks>
        public static Boolean IsNullOrEmpty(this string str)
        {
            return String.IsNullOrEmpty(str);
        }

        /// <summary>
        /// Given a string, and a second string which is to be a suffix, append
        /// the second string to the first if the first doesn't already end
        /// with the second.
        /// </summary>
        public static string AppendIfMissing(
            this string str,
            string suffix,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (suffix.IsNullOrEmpty())
                return str;
            if (str.IsNullOrEmpty())
                return suffix;
            if (str.EndsWith(suffix, comparison))
                return str;
            return str + suffix;
        }

        /// <summary>
        /// Given a string, and a char which is to be a suffix, append
        /// the char to the string if the string doesn't already end
        /// with the char.
        /// </summary>
        public static string AppendIfMissing(
            this string str,
            char suffix,
            StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return str.AppendIfMissing(suffix.ToString(), comparison);
        }

        /// <summary>
        /// Split a string into tokens by whitespace, except respecting quotes.
        /// (Similar to the way command line arguments are split.)
        /// </summary>
        /// <remarks>
        /// See http://stackoverflow.com/a/4780801/751579 for an explanation of
        /// this regex (modified to allow multiple whitespace between args - but
        /// it isn't exactly a command line argv/argc parser, so also see WinAPI
        /// CommandLineToArgvW
        /// https://msdn.microsoft.com/en-us/library/bb776391.aspx
        /// and here at SO http://stackoverflow.com/a/749653/751579 and also
        /// various C# command line parsing libraries.
        /// </remarks>
        public static string[] SplitRespectingQuotes(this string arg)
        {
            arg.MustNotBeNull("arg");

            var r = Regex.Split(arg, "(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*)\\s+(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            // Strip empty (all whitespace) arguments that are left (which
            // happens at the beginning and end of the array)
            if (r.Any(s => s.IsNullOrEmpty()))
            {
                r = r.Where(s => !s.IsNullOrEmpty()).ToArray();
            }

            return r;
        }

        /// <summary>
        /// Collect an entire stream of characters into a single string.
        /// </summary>
        public static string Collect(this IEnumerable<char> source)
        {
            var sb = new StringBuilder();
            foreach (char c in source)
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static IEnumerable<char> ToLiteralFormat(this IEnumerable<char> source)
        {
            const string singleCharEscapes = "'\"\\\0\a\b\f\n\r\t\v";
            const string singleCharCodes = "\'\"\\0abfnrtv";

            foreach (char c in source)
            {
                var index = singleCharEscapes.IndexOf(c);
                if (-1 != index)
                {
                    yield return '\\';
                    yield return singleCharCodes[index];
                    continue;
                }

                var category = Char.GetUnicodeCategory(c);
                switch (category)
                {
                        // A bunch of Unicode categories that I hope hold most of
                        // the non-printable Unicode characters - emit as hex
                        // escape instead:
                    case System.Globalization.UnicodeCategory.Control:
                    case System.Globalization.UnicodeCategory.Format:
                    case System.Globalization.UnicodeCategory.LineSeparator:
                    case System.Globalization.UnicodeCategory.OtherNotAssigned:
                    case System.Globalization.UnicodeCategory.ParagraphSeparator:
                    case System.Globalization.UnicodeCategory.PrivateUse:
                    case System.Globalization.UnicodeCategory.Surrogate:
                        var lit = ((int)c).ToString("X4");
                        yield return '\\';
                        yield return 'x';
                        foreach (char d in lit) yield return d;
                        continue;
                    default:
                        yield return c;
                        continue;
                }
            }
        }

        public static string ToLiteralFormat(this string source)
        {
            string result = source.ToCharArray().AsEnumerable().ToLiteralFormat().Collect();
            return result;
        }

        public static string ToLiteralFormat(this char source)
        {
            return new String(source, 1).ToLiteralFormat();
        }
    }
}
