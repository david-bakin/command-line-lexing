using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BakinsBits.CommandLineLexing
{
    namespace Details
    {
        /// <summary>
        /// Encapsulates native Win32 API calls (and related marshaling needs) used
        /// by CommandLineLexing.
        /// </summary>
        public static class Native
        {
            /// <summary>
            /// Parses a Unicode command line string and returns an array of pointers
            /// to the command line arguments, along with a count of such arguments,
            /// in a way that is similar to the standard C run-time argv and argc
            /// values.  (Includes strange handling of backslashes and quotes.)
            /// </summary>
            /// <param name="lpCmdLine">Pointer to a null-terminated Unicode string that
            /// contains the full command line.  Full means: includes program executable
            /// at the beginning.</param>
            /// <param name="pNumArgs">Pointer to an int that receives the number of
            /// array elements returned, similar to argc.</param>
            /// <returns>A pointer to an array of LPWSTR values, similar to argv.
            /// Unlike argv/argc there is no terminating NULL pointer in the argv-ish
            /// array of pointers to arguments.  This returned block of memory must
            /// be freed with LocalFree. Returns NULL for failure in which case call
            /// GetLastError.</returns>
            /// <remarks>
            /// See https://msdn.microsoft.com/en-us/library/windows/desktop/bb776391(v=vs.85).aspx
            /// especially the discussion of backslashes and quotes.  See also
            /// http://blogs.msdn.com/b/oldnewthing/archive/2010/09/17/10063629.aspx
            /// </remarks>
            [DllImport("shell32.dll", SetLastError = true)]
            public static extern IntPtr CommandLineToArgvW(
                [MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine,
                out int pNumArgs
            );

            /// <summary>
            /// Clean up after CommandLineToArgvW call.
            /// </summary>
            public static void CommandLineToArgvWCleanup(IntPtr argv)
            {
                Marshal.FreeHGlobal(argv);
            }

            /// <summary>
            /// Return a string that is pointed to by a native buffer containing an
            /// array of pointers to native strings.
            /// </summary>
            /// <param name="ptrArray">Pointer to a native buffer containing an array
            /// of pointers to native strings.</param>
            /// <param name="offset">Offset into pointer array (index, not byte
            /// offset).</param>
            /// <returns>Pointed-to native string, marshaled to .NET.</returns>
            public static string MarshalToStringFromPtrArray(this IntPtr ptrArray, int offset)
            {
                var byteOffset = offset * IntPtr.Size;
                IntPtr pStr = Marshal.ReadIntPtr(ptrArray, byteOffset);
                return Marshal.PtrToStringUni(pStr);
            }

            /// <summary>
            /// Throw the last Win32 error as an exception.
            /// </summary>
            public static void ThrowWin32Error()
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }

       }
    }
}
