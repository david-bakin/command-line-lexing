using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BakinsBits.CommandLineLexing.Details;
using BakinsBits.Utilities;

namespace BakinsBits.CommandLineLexing
{
    /// <summary>
    /// This class handles two different kinds of Windows command line lexing: Using
    /// the Win32 API (CommandLineToArgvW) and using the way that the Visual C++
    /// runtime splits up the command line to provide main() with argv and argc.
    /// </summary>
    /// <remarks>
    /// On Windows each program is responsible for splitting the command line into
    /// arguments.  Most let something else do it for them.  The three main providers
    /// of command line splitting services are:
    ///
    ///    a) The Visual C++ runtimes which creates argv and argc for main().
    ///    b) The .NET framework/engine which creates the args array for Main().
    ///    c) The Win32 API CommandLineToArgvW.
    ///
    /// It is interesting (though unfortunate) that each of the three methods splits
    /// the command line into arguments slightly differently (mainly in the handling
    /// of double-quotes, backslashes, and quoting double-quotes and backslashes).
    ///
    /// Usually one of those three methods suffices.  But sometimes you need to split
    /// the command line yourself.  In that case, this library contains methods for
    /// (a) and (c) above.
    /// </remarks>
    public class Lexer
    {
        /// <summary>
        /// Parses a command line into a bunch of arguments (throwing away the
        /// executable name at the start of the command line), using the Windows
        /// API CommandLineToArgvW, which handles quoting quotes specially.
        /// </summary>
        /// <param name="commandLine">Full command line including executable name.</param>
        /// <returns>Array of arguments from the given command line.</returns>
        public virtual string[] CommandLineToArgsWin32(string commandLine)
        {
            return CommandLineToExeAndArgsWin32(commandLine).Item2;
        }

        /// <summary>
        /// Parses a command line into an executable name and a bunch of arguments,
        /// using the Windows API CommandLineToArgvW, which handles quoting quotes
        /// specially.
        /// </summary>
        /// <param name="commandLine">Full command line including executable name.</param>
        /// <returns>A tuple where the first element (Item1) is the executable
        /// name and the second element (Item2) is the array of arguments.</returns>
        /// <remarks>
        /// See blog post http://intellitect.com/converting-command-line-string-to-args-using-commandlinetoargvw-api/
        /// which has code for how to do the tricky marshaling required by the
        /// CommandLineToArgvW API.
        /// </remarks>
        public virtual Tuple<string,string[]> CommandLineToExeAndArgsWin32(string commandLine)
        {
            int argc;
            IntPtr argv = Native.CommandLineToArgvW(commandLine, out argc);
            if (IntPtr.Zero == argv)
            {
                Native.ThrowWin32Error();
            }

            try
            {
                string executableName;
                var args = new string[argc - 1];

                // Get executable name first
                executableName = argv.MarshalToStringFromPtrArray(0);

                // Get arguments
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = argv.MarshalToStringFromPtrArray(i + 1);
                    args[i] = arg;
                }

                return Tuple.Create(executableName, args);
            }
            finally
            {
                Native.CommandLineToArgvWCleanup(argv);
            }
        }

        /// <summary>
        /// Parses a command line into a bunch of arguments (throwing away the
        /// executable name at the start of the command line), using the Visual C++
        /// algorithm, which handles quoting quotes specially.
        /// </summary>
        /// <param name="commandLine">Full command line including executable name.</param>
        /// <returns>Array of arguments from the given command line.</returns>
        public virtual string[] CommandLineToArgsCpp(string commandLine)
        {
            return CommandLineToExeAndArgsCpp(commandLine).Item2;
        }


        /// <summary>
        /// Parses a command line into an executable name and a bunch of arguments,
        /// executable name at the start of the command line), using the Visual C++
        /// algorithm, which handles quoting quotes specially.
        /// </summary>
        /// <param name="commandLine">Full command line including executable name.</param>
        /// <returns>A tuple where the first element (Item1) is the executable
        /// name and the second element (Item2) is the array of arguments.</returns>
        /// <remarks>
        /// See web page http://www.daviddeley.com/autohotkey/parameters/parameters.htm#WINCALGORITHM
        /// which has a reconstruction of the algorithm in the internal VC++ CRT
        /// routine parse_cmdline.  See also MSDN documentation at
        /// https://msdn.microsoft.com/en-us/library/17w5ykft(v=vs.120).aspx .
        /// </remarks>
        public virtual Tuple<string, string[]> CommandLineToExeAndArgsCpp(string commandLine)
        {
            // Handle first argument, which is a (possibly quoted) program name.
            // Since it must be a valid file name (NTFS or other file system) and
            // therefore can't have double-quotes, the handling of this argument
            // is simpler than other arguments.  (Whether this is a good idea or not,
            // though, ...)  Basically, if the first argument is quoted then whatever
            // is between the leading double-quote and the next double-quote (or end
            // of string) is just copied, except for the double-quote characters
            // themselves.

            var firstArgument = new StringBuilder();
            var arguments = new List<string>();

            commandLine.AsEnumerable()
                       .TakeUntilNul()
                       .RemoveFirstArgument(firstArgument)
                       .RemoveRemainingArguments(arguments);

            return Tuple.Create(firstArgument.ToString(), arguments.ToArray());
        }

        public virtual string ExeAndArgsToCommandLine(string exeName, IEnumerable<string> args)
        {
            return ArgsToCommandLine(exeName.Yield().Concat(args));
        }

        public virtual string ArgsToCommandLine(IEnumerable<string> args)
        {
            return String.Join(" ", args.Select(a => a.Quotify()));
        }
    }
}
