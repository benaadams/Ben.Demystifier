using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class TuplesTests
    {
        [Fact]
        public void DemistifiesAsyncMethodWithTuples()
        {
            Exception demystifiedException = null;

            try
            {
                AsyncThatReturnsTuple().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                demystifiedException = ex.Demystify();
            }

            // Assert
            var stackTrace = demystifiedException.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = string.Join("", stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            var expected = string.Join("", new[] {
                    "System.ArgumentException: Value does not fall within the expected range.",
                    "   at async Task<(int left, int right)> Ben.Demystifier.Test.TuplesTests.AsyncThatReturnsTuple()",
                    "   at void Ben.Demystifier.Test.TuplesTests.DemistifiesAsyncMethodWithTuples()"});

            Assert.Equal(expected, trace);
        }

        [Fact]
        public void DemistifiesListOfTuples()
        {
            Exception demystifiedException = null;

            try
            {
                ListOfTuples();
            }
            catch (Exception ex)
            {
                demystifiedException = ex.Demystify();
            }

            // Assert
            var stackTrace = demystifiedException.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = string.Join("", stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            var expected = string.Join("", new[] {
                    "System.ArgumentException: Value does not fall within the expected range.",
                    "   at List<(int left, int right)> Ben.Demystifier.Test.TuplesTests.ListOfTuples()",
                    "   at void Ben.Demystifier.Test.TuplesTests.DemistifiesListOfTuples()"});

            Assert.Equal(expected, trace);
        }

        async Task<(int left, int right)> AsyncThatReturnsTuple()
        {
            await Task.Delay(1).ConfigureAwait(false);
            throw new ArgumentException();
        }

        List<(int left, int right)> ListOfTuples()
        {
            throw new ArgumentException();
        }
    }
}
