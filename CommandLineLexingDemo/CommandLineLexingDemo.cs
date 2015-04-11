using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BakinsBits.CommandLineLexing
{
    namespace Demo
    {
        class CommandLineLexingDemo
        {
            static void Main(string[] args)
            {
                var main = new CommandLineLexingDemo(args);
                main.DoIt();
            }

            public CommandLineLexingDemo(string[] args)
            {
                Args = args;
            }

            public string[] Args { get; set; }

            public void DoIt()
            {
                var lexer = new Lexer();
                var report = new Report();

                // C#
                report.CommandLine = Environment.CommandLine;
                report.CSharpArguments = Args;

                // Win32
                var win32 = lexer.CommandLineToExeAndArgsWin32(report.CommandLine);
                report.Win32Executable = win32.Item1;
                report.Win32Arguments = win32.Item2;

                // C/C++
                var ccpp = lexer.CommandLineToExeAndArgsCpp(report.CommandLine);
                report.CppExecutable = ccpp.Item1;
                report.CppArguments = ccpp.Item2;

                // Generate the report and output it to the console
                var output = report.TransformText();
                Console.Write(output);
            }

            public static string GetExecutableName()
            {
                return System.Reflection.Assembly.GetEntryAssembly().Location;
            }
        }
    }
}
