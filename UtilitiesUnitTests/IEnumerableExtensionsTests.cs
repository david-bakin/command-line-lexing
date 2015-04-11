using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using FakeItEasy;
using FluentAssertions;

using BakinsBits.Utilities;

namespace BakinsBits.UtilitiesTests
{
    public class IEnumerableExtensionsTests
    {
        [Fact]
        public void AddSentinalToEndOfEmpty()
        {
            var emptySeq = (new string[0]).AsEnumerable();
            var resultSeq = emptySeq.AddSentinelToEnd("xyzzy").ToList();
            resultSeq.Should().ContainInOrder(new List<string> { "xyzzy" });
        }

        [Fact]
        public void AddSentinalToEndOfNonEmpty()
        {
            var aSeq = (new string[] { "abc", "def", "ghi", "jkl" }).AsEnumerable();
            var resultSeq = aSeq.AddSentinelToEnd("xyzzy").ToList();
            resultSeq.Should().ContainInOrder(new List<string> { "abc", "def", "ghi", "jkl", "xyzzy" });
        }
    }
}
