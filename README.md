# command-line-lexing
Splitting command lines into arguments (and back) on Windows

(This is not *command line parsing* - that's interpreting filenames and options and stuff on the command line, and there are
plenty of packages for that.  This is *lexing* - splitting the command line into the executable file name and arguments.)

On Windows, programs are responsible for their own command line lexing.  C and C++ programs usually do it by letting the C/C++ runtime do it - command lines are already lexed by the time `main(int argc, char *argv[])` is called.  C# (and VB) programs
get their arguments lexed by the .NET runtime before `Main(string[] args)` (or `Main(ByVal cmdArgs() As String)`) is called.  Otherwise, programs generally do it by calling the Windows API function [`CommandLineToArgvW`](https://msdn.microsoft.com/en-us/library/windows/desktop/bb776391(v=vs.85).aspx).

But sometimes you have to get dirty and break down the command line yourself - or build it up.  If you're working in a
.NET managed language this is the library for it.

### News
2014-04-10: Setting up the repository today.  Code is ready to be put into it.
