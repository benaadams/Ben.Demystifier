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
            var traces = stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Regex.Replace(s, " x [0-9]+", " x N")) 
                .Skip(1)
                .ToArray();

            Assert.Contains("   at async Task<int> Ben.Demystifier.Test.RecursionTests.RecurseAsync(int depth) x N", traces);
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
            var traces = stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Regex.Replace(s, " x [0-9]+", " x N"))
                .Skip(1)
                .ToArray();

            Assert.Contains("   at int Ben.Demystifier.Test.RecursionTests.Recurse(int depth) x N", traces);
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
