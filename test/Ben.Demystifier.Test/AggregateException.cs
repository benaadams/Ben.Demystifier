using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Demystify
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
            stackTrace = ReplaceLineEndings.Replace(stackTrace, "");
            var trace = string.Join("", stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                // Remove items that vary between test runners
                .Where(s =>
                    s != "   at Task Demystify.DynamicCompilation.DoesNotPreventStackTrace()+()=>{}" &&
                    !s.Contains("System.Threading.Tasks.Task.WaitAll")
                )
                .Skip(1)
                .ToArray())
                // Remove Full framework back arrow
                .Replace("<---", "");

            var expected = string.Join("", new[] {
                    "   at async Task Demystify.AggregateException.Throw1()",
                    "   at async void Demystify.AggregateException.DemystifiesAggregateExceptions()+(?)=>{}",
                    "   --- End of inner exception stack trace ---",
                    "   at void Demystify.AggregateException.DemystifiesAggregateExceptions()",
                    "---> (Inner Exception #0) System.ArgumentException: Value does not fall within the expected range.",
                    "   at async Task Demystify.AggregateException.Throw1()",
                    "   at async void Demystify.AggregateException.DemystifiesAggregateExceptions()+(?)=>{}",
                    "---> (Inner Exception #1) System.NullReferenceException: Object reference not set to an instance of an object.",
                    "   at async Task Demystify.AggregateException.Throw2()",
                    "   at async void Demystify.AggregateException.DemystifiesAggregateExceptions()+(?)=>{}",
                    "---> (Inner Exception #2) System.InvalidOperationException: Operation is not valid due to the current state of the object.",
                    "   at async Task Demystify.AggregateException.Throw3()",
                    "   at async void Demystify.AggregateException.DemystifiesAggregateExceptions()+(?)=>{}"});

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


        private Regex ReplaceLineEndings = new Regex(" in [^\n\r]+");
    }
}
