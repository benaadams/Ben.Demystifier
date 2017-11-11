using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Demystify
{
    public class NonThrownException
    {
        [Fact]
        public async Task DoesNotPreventThrowStackTrace()
        {
            // Arrange
            Exception innerException = null;
            try
            {
                await Task.Run(() => throw new Exception()).ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                innerException = ex;
            }

            // Act
            Exception demystifiedException = new Exception(innerException.Message, innerException).Demystify();

            // Assert
            var stackTrace = demystifiedException.ToString();
            stackTrace = ReplaceLineEndings.Replace(stackTrace, "");
            var trace = stackTrace.Split(Environment.NewLine);

            Assert.Equal(
                new[] {     
                    "System.Exception: Exception of type 'System.Exception' was thrown. ---> System.Exception: Exception of type 'System.Exception' was thrown.",
                    "   at Task Demystify.NonThrownException.DoesNotPreventThrowStackTrace()+()=>{}",
                    "   at void System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, object state)",
                    "   at async Task Demystify.NonThrownException.DoesNotPreventThrowStackTrace()",
                    "   --- End of inner exception stack trace ---"}, 
                trace);

            // Act
            try
            {
                throw demystifiedException;
            }
            catch (Exception ex)
            {
                demystifiedException = ex.Demystify();
            }

            // Assert
            stackTrace = demystifiedException.ToString();
            stackTrace = ReplaceLineEndings.Replace(stackTrace, "");
            trace = stackTrace.Split(Environment.NewLine);

            Assert.Equal(
                new[] {
                    "System.Exception: Exception of type 'System.Exception' was thrown. ---> System.Exception: Exception of type 'System.Exception' was thrown.",
                    "   at Task Demystify.NonThrownException.DoesNotPreventThrowStackTrace()+()=>{}",
                    "   at void System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, object state)",
                    "   at async Task Demystify.NonThrownException.DoesNotPreventThrowStackTrace()",
                    "   --- End of inner exception stack trace ---",
                    "   at async Task Demystify.NonThrownException.DoesNotPreventThrowStackTrace()"
                },
                trace);
        }

        [Fact]
        public async Task Current()
        {
            // Arrange
            EnhancedStackTrace est = null;

            // Act
            await Task.Run(() => est = EnhancedStackTrace.Current()).ConfigureAwait(false);

            // Assert
            var stackTrace = est.ToString();
            stackTrace = ReplaceLineEndings.Replace(stackTrace, "");
            var trace = stackTrace.Split(Environment.NewLine);

            Assert.Equal(
                new[] {
                    "   at void System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, object state)",
                    "   at bool System.Threading.ThreadPoolWorkQueue.Dispatch()"},
                trace);
        }

        private Regex ReplaceLineEndings = new Regex(" in [^\n\r]+");
    }
}
