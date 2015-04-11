using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BakinsBits.Utilities
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Add a sentinel element to the end of a sequence.
        /// </summary>
        public static IEnumerable<T> AddSentinelToEnd<T>(this IEnumerable<T> seq, T sentinal = default(T))
        {
            foreach (T element in seq)
            {
                yield return element;
            }
            yield return sentinal;
        }

        /// <summary>
        /// "Spy" on a LINQ pipeline by inserting an Action into the middle of
        /// it.  The Action will be called for all elements passing through the
        /// pipeline at the Spy, and the elements will be passed on.
        /// </summary>
        /// <remarks>
        /// If the elements being enumerated are reference types then this Spy
        /// could actually alter the elements in place ... if that's what you
        /// wanted.
        /// </remarks>
        public static IEnumerable<T> Spy<T>(this IEnumerable<T> seq, Action<T> spy)
        {
            foreach (T element in seq)
            {
                spy(element);
                yield return element;
            }
        }

        /// <summary>
        /// "Spy on a LINQ pipeline by inserting an Action into the middle of
        /// it.  The Action will be called for all elements passing through the
        /// pipeline at the Spy, and the elements will be passed on.  At the end
        /// of the stream, a second (different) Action will be called (which can
        /// be used to produce a "summary" or "report" or "end-of-stream
        /// computation".
        /// </summary>
        /// <remarks>
        /// Presumably the Spy action and the Summarize action are linked via some
        /// shared state (e.g., an accumulator).
        /// If the elements being enumerated are reference types then this Spy
        /// could actually alter the elements in place ... if that's what you
        /// wanted.
        /// </remarks>
        public static IEnumerable<T> Spy<T>(this IEnumerable<T> seq, Action<T> spy, Action summarize)
        {
            foreach (T element in seq)
            {
                spy(element);
                yield return element;
            }
            summarize();
        }

        /// <summary>
        /// Convert an enumerator to an enumerable so you can use it in a foreach statement.
        /// </summary>
        /// <remarks>
        /// The returned enumerable isn't a true enumerable since it can't go through the original
        /// enumerator more than once.  In fact, it consumes the enumerator.  For a true enumerable
        /// you'd have to memoize all the elements returned by the enumerator.
        /// </remarks>
        public static IEnumerable<T> ToOnceOnlyEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        /// <summary>
        /// Turn a single object into a sequence (of one element).
        /// </summary>
        /// <remarks>
        /// See this SO question: http://stackoverflow.com/questions/1577822/passing-a-single-item-as-ienumerablet
        /// </remarks>
        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }
    }
}
