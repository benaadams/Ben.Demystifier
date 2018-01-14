using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace Ben.Demystifier.Test
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
            var demystifiedException = new Exception(innerException.Message, innerException).Demystify();

            // Assert
            var stackTrace = demystifiedException.ToString();
            stackTrace = ReplaceLineEndings.Replace(stackTrace, "");
            var trace = stackTrace.Split(new[]{Environment.NewLine}, StringSplitOptions.None);

            Assert.Equal(
                new[] {     
                    "System.Exception: Exception of type 'System.Exception' was thrown. ---> System.Exception: Exception of type 'System.Exception' was thrown.",
                    "   at Task Ben.Demystifier.Test.NonThrownException.DoesNotPreventThrowStackTrace()+() => { }",
                    "   at async Task Ben.Demystifier.Test.NonThrownException.DoesNotPreventThrowStackTrace()",
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
            trace = stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            Assert.Equal(
                new[] {
                    "System.Exception: Exception of type 'System.Exception' was thrown. ---> System.Exception: Exception of type 'System.Exception' was thrown.",
                    "   at Task Ben.Demystifier.Test.NonThrownException.DoesNotPreventThrowStackTrace()+() => { }",
                    "   at async Task Ben.Demystifier.Test.NonThrownException.DoesNotPreventThrowStackTrace()",
                    "   --- End of inner exception stack trace ---",
                    "   at async Task Ben.Demystifier.Test.NonThrownException.DoesNotPreventThrowStackTrace()"
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
            var trace = stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                // Remove Full framework entries
                .Where(s => !s.StartsWith("   at bool System.Threading._ThreadPoolWaitCallbac") &&
                       !s.StartsWith("   at void System.Threading.Tasks.Task.System.Thre"));


            Assert.Equal(
                new[] {
                    "   at bool System.Threading.ThreadPoolWorkQueue.Dispatch()"},
                trace);
        }

        private Regex ReplaceLineEndings = new Regex(" in [^\n\r]+");
    }
}
