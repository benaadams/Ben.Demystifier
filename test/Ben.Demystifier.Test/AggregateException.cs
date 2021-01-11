using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class AggregateException
    {
        [Fact]
        public void DemystifiesAggregateExceptions()
        {
            Exception demystifiedException = null;

            try
            {
                var tasks = new List<Task>
                {
                    Task.Run(async () => await Throw1()),
                    Task.Run(async () => await Throw2()),
                    Task.Run(async () => await Throw3())
                };

                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex)
            {
                demystifiedException = ex.Demystify();
            }

            // Assert
            var stackTrace = demystifiedException.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = string.Join("", stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                // Remove items that vary between test runners
                .Where(s =>
                    (s != "   at Task Ben.Demystifier.Test.DynamicCompilation.DoesNotPreventStackTrace()+() => { }" &&
                    !s.Contains("System.Threading.Tasks.Task.WaitAll"))
                )
                .Skip(1)
                .ToArray())
                // Remove Full framework back arrow
                .Replace("<---", "");
#if NET5_0 || NETCOREAPP3_1 || NETCOREAPP3_0
            var expected = string.Join("", new[] {
                " ---> System.ArgumentException: Value does not fall within the expected range.",
                "   at async Task Ben.Demystifier.Test.AggregateException.Throw1()",
                "   at async void Ben.Demystifier.Test.AggregateException.DemystifiesAggregateExceptions()+(?) => { }",
                "   --- End of inner exception stack trace ---",
                "   at void Ben.Demystifier.Test.AggregateException.DemystifiesAggregateExceptions()",
                " ---> (Inner Exception #1) System.NullReferenceException: Object reference not set to an instance of an object.",
                "   at async Task Ben.Demystifier.Test.AggregateException.Throw2()",
                "   at async void Ben.Demystifier.Test.AggregateException.DemystifiesAggregateExceptions()+(?) => { }",
                " ---> (Inner Exception #2) System.InvalidOperationException: Operation is not valid due to the current state of the object.",
                "   at async Task Ben.Demystifier.Test.AggregateException.Throw3()",
                "   at async void Ben.Demystifier.Test.AggregateException.DemystifiesAggregateExceptions()+(?) => { }"
            });
#else
            var expected = string.Join("", new[] {
                    "   at async Task Ben.Demystifier.Test.AggregateException.Throw1()",
                    "   at async void Ben.Demystifier.Test.AggregateException.DemystifiesAggregateExceptions()+(?) => { }",
                    "   --- End of inner exception stack trace ---",
                    "   at void Ben.Demystifier.Test.AggregateException.DemystifiesAggregateExceptions()",
                    "---> (Inner Exception #0) System.ArgumentException: Value does not fall within the expected range.",
                    "   at async Task Ben.Demystifier.Test.AggregateException.Throw1()",
                    "   at async void Ben.Demystifier.Test.AggregateException.DemystifiesAggregateExceptions()+(?) => { }",
                    "---> (Inner Exception #1) System.NullReferenceException: Object reference not set to an instance of an object.",
                    "   at async Task Ben.Demystifier.Test.AggregateException.Throw2()",
                    "   at async void Ben.Demystifier.Test.AggregateException.DemystifiesAggregateExceptions()+(?) => { }",
                    "---> (Inner Exception #2) System.InvalidOperationException: Operation is not valid due to the current state of the object.",
                    "   at async Task Ben.Demystifier.Test.AggregateException.Throw3()",
                    "   at async void Ben.Demystifier.Test.AggregateException.DemystifiesAggregateExceptions()+(?) => { }"});
#endif
            Assert.Equal(expected, trace);
        }

        async Task Throw1()
        {
            await Task.Delay(1).ConfigureAwait(false);
            throw new ArgumentException();
        }

        async Task Throw2()
        {
            await Task.Delay(1).ConfigureAwait(false);
            throw new NullReferenceException();
        }

        async Task Throw3()
        {
            await Task.Delay(1).ConfigureAwait(false);
            throw new InvalidOperationException();
        }
    }
}
