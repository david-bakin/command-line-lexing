using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using FakeItEasy;
using FluentAssertions;

using BakinsBits.Utilities;

namespace Utilities_UnitTests
{
    public class ValidationExtensionTests
    {
        [Theory]
        [MemberData("MatchesTestData")]
        public void MatchesTests(string subject, string regex, bool expected)
        {
            bool actual = ValidationExtensions.Matches<string>(regex)(subject);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData("MatchesTestData")]
        public void ValidationMatchesTests(string subject, string regex, bool expected)
        {
            if (expected)
            {
                subject.Validate(ValidationExtensions.Matches<string>(regex), "foo");
            }
            else
            {
                Assert.Throws(typeof(System.ArgumentException), () => subject.Validate(ValidationExtensions.Matches<string>(regex), "foo"));
            }
        }

        public static IEnumerable<object[]> MatchesTestData
        {
            get
            {
                return new[]
                {
                    //              subj  regex  expect
                    new object [] { "", "abc", false },
                    new object [] { null, "abc", false },
                    new object [] { "abc", "abc", true },
                    new object [] { "abc", "^abc$", true },
                    new object [] { "123", "^[0-9]+$", true },
                    new object [] { "123.4", "^[0-9]+$", false },
                    new object [] { "123", @"^[0-9]+(\.[0-9]+)*$", true },
                    new object [] { "123.", @"^[0-9]+(\.[0-9]+)*$", false },
                    new object [] { ".123", @"^[0-9]+(\.[0-9]+)*$", false },
                    new object [] { "123.123", @"^[0-9]+(\.[0-9]+)*$", true },
                    new object [] { "123.123.", @"^[0-9]+(\.[0-9]+)*$", false },
                    new object [] { "123.456.789", @"^[0-9]+(\.[0-9]+)*$", true },
                    new object [] { "123A", "^[0-9]+$", false },
                    new object [] { "123.A", "^[0-9]+$", false },
                    new object [] { "123.456A", "^[0-9]+$", false },
                };
            }
        }
    }
}
