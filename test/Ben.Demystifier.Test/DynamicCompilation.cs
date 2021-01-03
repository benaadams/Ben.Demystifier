using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class DynamicCompilation
    {
        [Fact]
        public async Task DoesNotPreventStackTrace()
        {
            // Arrange
           var expression = Expression.Throw(
                Expression.New(
                    typeof(ArgumentException).GetConstructor(
                        new Type[] {typeof(string)}),
                    Expression.Constant( "Message")));

            var lambda = Expression.Lambda<Action>(expression);

            var action = lambda.Compile();

            // Act
            Exception demystifiedException = null;
            try
            {
                await Task.Run(() => action()).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                demystifiedException = ex.Demystify();
            }

            // Assert
            var stackTrace = demystifiedException.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                // Remove items that vary between test runners
                .Where(s =>
                    s != "   at void System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, object state)" &&
                    s != "   at Task Ben.Demystifier.Test.DynamicCompilation.DoesNotPreventStackTrace()+() => { }"
                )
                .Select(s => Regex.Replace(s, "lambda_method[0-9]+\\(", "lambda_method("))
                .ToArray();

            Assert.Equal(
                new[] {
                    "System.ArgumentException: Message",
                    "   at void lambda_method(Closure)",
                    "   at async Task Ben.Demystifier.Test.DynamicCompilation.DoesNotPreventStackTrace()"}, 
                trace);
        }
    }
}
