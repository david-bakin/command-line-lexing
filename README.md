# command-line-lexing
Splitting command lines into arguments (and back) on Windows

(This is not *command line parsing* - that's interpreting filenames and options and stuff on the command line, and there are
plenty of packages for that.  This is *lexing* - splitting the command line into the executable file name and arguments.)

On Windows, programs are responsible for their own command line lexing.  C and C++ programs usually do it by letting the C/C++ runtime do it - command lines are already lexed by the time `main(int argc, char *argv[])` is called.  C# (and VB) programs
get their arguments lexed by the .NET runtime before `Main(string[] args)` (or `Main(ByVal cmdArgs() As String)`) is called.  Otherwise, programs generally do it by calling the Windows API function [`CommandLineToArgvW`](https://msdn.microsoft.com/en-us/library/windows/desktop/bb776391(v=vs.85).aspx).

But sometimes you have to get dirty and break down the command line yourself - or build it up.  If you're working in a
.NET managed language this is the library for it.

### News
2014-04-10: Setting up the repository today.  First version of code - working through unit tests and sample
command line application.

2014-04-11: And now quotifying arguments and building a command line is in and tested.

2014-04-14: Used ILMerge to move Utilities assembly into CommandLineLexer assembly and make all Utilities types internal so that Utilities assembly isn't separately deployed.  Reason is:  It's only a subset of my larger (not yet on github) utilities library and having it deployed might interfere with other copies of it.

2014-04-14: Having some issues with Nuget packaging and symbolsource.org.  First, having run ILMerge to eliminate the Utilities assembly I get a binary package that has only the CommandLineLexer.dll as desired, but the sources package has no sources for Utilities.  I ended up running two different "nuget packs" - one without including dependent projects, and one with, and uploaded a mixed set so that Nuget got one without Utilities but symbolsource.org got one with all the source (and the Utilities assembly too).  But then, testing on a second machine, I couldn't step into even the CommandLineLexer code (I didn't even get to Utilities code).  So I uploaded a matched set that included the Utilities.dll to Nuget and I couldn't step into that either. Finally, I just added a dependency to a random package I found on symbolsource.org (FluentPath) and couldn't step into that.  In all cases symbolsource.org was returning 404 for all pdbs (according to Fiddler).  So I put a message on Google Groups for Symbolsource asking what I was doing wrong.
