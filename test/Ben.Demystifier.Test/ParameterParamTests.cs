namespace Ben.Demystifier.Test
{
    using System;
    using System.Diagnostics;
    using Xunit;

    public class ParameterParamTests
    {
        [Fact]
        public void DemistifiesMethodWithParams()
        {
            Exception dex = null;
            try
            {
                MethodWithParams(1, 2, 3);
            }
            catch (Exception e)
            {
                dex = e.Demystify();
            }

            // Assert
            var stackTrace = dex.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = string.Join(string.Empty, stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            var expected = string.Join(string.Empty, new[] {
                "System.ArgumentException: Value does not fall within the expected range.",
                "   at bool Ben.Demystifier.Test.ParameterParamTests.MethodWithParams(params int[] numbers)",
                "   at void Ben.Demystifier.Test.ParameterParamTests.DemistifiesMethodWithParams()"});

            Assert.Equal(expected, trace);
        }

        private bool MethodWithParams(params int[] numbers)
        {
            throw new ArgumentException();
        }
    }
}
