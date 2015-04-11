using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BakinsBits.CommandLineLexing.Demo
{
    public partial class Report
    {
        public Report()
        {

        }

        public string ExecutableName { get; set; }
        public string CommandLine { get; set; }
        public string[] CSharpArguments { get; set; }
        public string Win32Executable { get; set; }
        public string[] Win32Arguments { get; set; }
        public string CppExecutable { get; set; }
        public string[] CppArguments { get; set; }
    }
}
