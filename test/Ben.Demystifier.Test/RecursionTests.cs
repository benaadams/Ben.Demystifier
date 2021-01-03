using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class RecursionTests
    {
        [Fact]
        public async Task DemystifiesAsyncRecursion()
        {
            Exception demystifiedException = null;

            try
            {
                await RecurseAsync(10);
            }
            catch (Exception ex)
            {
                demystifiedException = ex.Demystify();
            }

            // Assert
            var stackTrace = demystifiedException.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = string.Join("", stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Regex.Replace(s, " x [0-9]+", " x N")) 
                .Skip(1)
                .ToArray());
            var expected = string.Join("", new[] {
                "   at async Task<int> Ben.Demystifier.Test.RecursionTests.RecurseAsync(int depth)",
                "   at async Task<int> Ben.Demystifier.Test.RecursionTests.RecurseAsync(int depth) x N",
                "   at async Task Ben.Demystifier.Test.RecursionTests.DemystifiesAsyncRecursion()"
            });
            Assert.Equal(expected, trace);
        }

        [Fact]
        public void DemystifiesRecursion()
        {
            Exception demystifiedException = null;

            try
            {
                Recurse(10);
            }
            catch (Exception ex)
            {
                demystifiedException = ex.Demystify();
            }

            // Assert
            var stackTrace = demystifiedException.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = string.Join("", stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Regex.Replace(s, " x [0-9]+", " x N"))
                .Skip(1)
                .ToArray());
            var expected = string.Join("", new[] {
                "   at int Ben.Demystifier.Test.RecursionTests.Recurse(int depth)",
                "   at int Ben.Demystifier.Test.RecursionTests.Recurse(int depth) x N",
                "   at void Ben.Demystifier.Test.RecursionTests.DemystifiesRecursion()"
            });
            Assert.Equal(expected, trace);
        }

        async Task<int> RecurseAsync(int depth)
        {
            if (depth > 0) await RecurseAsync(depth - 1);
            throw new InvalidOperationException();
        }

        int Recurse(int depth)
        {
            if (depth > 0) Recurse(depth - 1);
            throw new InvalidOperationException();
        }
    }
}
