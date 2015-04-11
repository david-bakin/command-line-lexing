using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BakinsBits.Utilities
{
    /// <summary>
    /// Extension methods to validate values of things in order to satisfy
    /// invariants.  They all throw an exception when validation fails.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Syntax sugar for checking that an argument is not null.
        /// </summary>
        public static void MustNotBeNull(this object o, string argumentName)
        {
            if (null == o)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Syntax sugar for checking that an argument is not null. Delayed
        /// evaluation of its message argument.
        /// </summary>
        public static void MustNotBeNull(this object o, Func<string> fArgumentName)
        {
            if (null == o)
            {
                throw new ArgumentNullException(fArgumentName());
            }
        }

        /// <summary>
        /// Syntax sugar for checking that a value is not null, throwing a specific
        /// kind of exception (given as generic type argument).  Also, with built-in
        /// fancy message formatting.
        /// </summary>
        /// <typeparam name="E">Type of exception to throw.</typeparam>
        public static void MustNotBeNull<E>(
            this object o,
            string message,
            params object[] messageArgs) where E : System.Exception
        {
            if (null == o)
            {
                string interpolatedMessage = (0 == messageArgs.Length) ? message : message.FormatWith(messageArgs);
                throw (System.Exception)Activator.CreateInstance(typeof(E), interpolatedMessage);
            }
        }

        /// <summary>
        /// Syntax sugar for checking that a string argument is not null or empty.
        /// </summary>
        public static void MustNotBeNullOrEmpty(this string s, string argumentName)
        {
            if (s.IsNullOrEmpty())
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Syntax sugar for checking that a string argument is not null or empty.
        /// Delayed evaluation of its message argument.
        /// </summary>
        public static void MustNotBeNullOrEmpty(this string s, Func<string> fArgumentName)
        {
            if (s.IsNullOrEmpty())
            {
                throw new ArgumentNullException(fArgumentName());
            }
        }

        /// Syntax sugar for checking that a value is not null or empty, throwing a
        /// specific kind of exception (given as generic type argument).  Also, with
        /// built-in fancy message formatting.
        /// </summary>
        /// <typeparam name="E">Type of exception to throw.</typeparam>
        public static void MustNotBeNullOrEmpty<E>(
            this string s,
            string message,
            params object[] messageArgs) where E : System.Exception
        {
            if (s.IsNullOrEmpty())
            {
                string interpolatedMessage = (0 == messageArgs.Length) ? message : message.FormatWith(messageArgs);
                throw (System.Exception)Activator.CreateInstance(typeof(E), interpolatedMessage);
            }
        }

        /// <summary>
        /// Syntax sugar for checking that the argument passes a validation function.
        /// </summary>
        public static void Validate<T>(this T subject, Func<T, bool> validation, string argName)
        {
            validation.MustNotBeNull("validation");
            if (!validation(subject))
            {
                throw new ArgumentException(String.Format("{0} ({1}) fails to validate",
                                                          argName, subject));
            }
        }

        /// <summary>
        /// Returns a predicate that given a subject object of some type T returns
        /// whether that object.ToString() matches a given regular expression.
        /// </summary>
        public static Func<T, bool> Matches<T>(string regex)
        {
            return (T subject) =>
            {
                var s = subject != null ? subject.ToString() : String.Empty;
                return System.Text.RegularExpressions.Regex.IsMatch(s, regex);
            };
        }
    }
}
