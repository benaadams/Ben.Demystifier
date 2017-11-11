using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Demystify
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
            stackTrace = ReplaceLineEndings.Replace(stackTrace, "");
            var trace = stackTrace.Split(Environment.NewLine);

            Assert.Equal(
                new[] {
                    "System.ArgumentException: Message",
                    "   at void lambda_method(Closure)",
                    "   at Task Demystify.DynamicCompilation.DoesNotPreventThrowStackTrace()+()=>{}",
                    "   at void System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, object state)",
                    "   at async Task Demystify.DynamicCompilation.DoesNotPreventThrowStackTrace()"}, 
                trace);
        }

        private Regex ReplaceLineEndings = new Regex(" in [^\n\r]+");
    }
}
