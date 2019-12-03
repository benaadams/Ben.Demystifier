namespace Ben.Demystifier.Test
{
    using System;
    using System.Diagnostics;
    using Xunit;

    public class MethodTests
    {
        [Fact]
        public void DemistifiesMethodWithNullableInt()
        {
            Exception dex = null;
            try
            {
                MethodWithNullableInt(1);
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
                "   at bool Ben.Demystifier.Test.MethodTests.MethodWithNullableInt(int? number)",
                "   at void Ben.Demystifier.Test.MethodTests.DemistifiesMethodWithNullableInt()"});

            Assert.Equal(expected, trace);
        }
        
        [Fact]
        public void DemistifiesMethodWithDynamic()
        {
            Exception dex = null;
            try
            {
                MethodWithDynamic(1);
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
                "   at bool Ben.Demystifier.Test.MethodTests.MethodWithDynamic(dynamic value)",
                "   at void Ben.Demystifier.Test.MethodTests.DemistifiesMethodWithDynamic()"});

            Assert.Equal(expected, trace);
        }

        private bool MethodWithNullableInt(int? number) => throw new ArgumentException();

        private bool MethodWithDynamic(dynamic value) => throw new ArgumentException();
    }
}
