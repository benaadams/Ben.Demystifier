namespace Ben.Demystifier.Test
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
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

            var expected = string.Join(string.Empty,
                "System.ArgumentException: Value does not fall within the expected range.",
                "   at bool Ben.Demystifier.Test.MethodTests.MethodWithNullableInt(int? number)",
                "   at void Ben.Demystifier.Test.MethodTests.DemistifiesMethodWithNullableInt()");

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

            var expected = string.Join(string.Empty,
                "System.ArgumentException: Value does not fall within the expected range.",
                "   at bool Ben.Demystifier.Test.MethodTests.MethodWithDynamic(dynamic value)",
                "   at void Ben.Demystifier.Test.MethodTests.DemistifiesMethodWithDynamic()");

            Assert.Equal(expected, trace);
        }

        [Fact]
        public void DemistifiesMethodWithLambda()
        {
            Exception dex = null;
            try
            {
                MethodWithLambda();
            }
            catch (Exception e)
            {
                dex = e.Demystify();
            }

            // Assert
            var stackTrace = dex.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = string.Join(string.Empty, stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            var expected = string.Join(string.Empty,
                "System.ArgumentException: Value does not fall within the expected range.",
                "   at void Ben.Demystifier.Test.MethodTests.MethodWithLambda()+() => { }",
                "   at void Ben.Demystifier.Test.MethodTests.MethodWithLambda()",
                "   at void Ben.Demystifier.Test.MethodTests.DemistifiesMethodWithLambda()");

            Assert.Equal(expected, trace);
        }


        [Fact]
        public void DemistifiesMethodWithAsyncLambda()
        {
            Exception dex = null;
            try
            {
                MethodWithAsyncLambda();
            }
            catch (Exception e)
            {
                dex = e.Demystify();
            }

            // Assert
            var stackTrace = dex.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = string.Join(string.Empty, stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));

            var expected = string.Join(string.Empty,
                "System.ArgumentException: Value does not fall within the expected range.",
                "   at async void Ben.Demystifier.Test.MethodTests.MethodWithAsyncLambda()+(?) => { }",
                "   at void Ben.Demystifier.Test.MethodTests.MethodWithAsyncLambda()",
                "   at void Ben.Demystifier.Test.MethodTests.DemistifiesMethodWithAsyncLambda()");

            Assert.Equal(expected, trace);
        }

        private bool MethodWithNullableInt(int? number) => throw new ArgumentException();

        private bool MethodWithDynamic(dynamic value) => throw new ArgumentException();

        private void MethodWithLambda()
        {
            Action action = () => throw new ArgumentException();
            action();
        }

        private void MethodWithAsyncLambda()
        {
            Func<Task> action = async () => throw new ArgumentException();
            action().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
