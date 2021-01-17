using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class ResolvedMethodTests
    {
        [Fact]
        public void AppendWithFullNameTrueTest()
        {
            var resolvedMethod = EnhancedStackTrace.GetMethodDisplayString(GetType().GetMethods().First(m => m.Name == nameof(AppendWithFullNameTrueTest)));
            var sb = new StringBuilder();
            Assert.Equal($"void {GetType().Namespace}.{GetType().Name}.{nameof(AppendWithFullNameTrueTest)}()", resolvedMethod.Append(sb).ToString());
        }

        [Fact]
        public void AppendWithFullNameFalseTest()
        {
            var resolvedMethod = EnhancedStackTrace.GetMethodDisplayString(GetType().GetMethods().First(m => m.Name == nameof(AppendWithFullNameFalseTest)));
            var sb = new StringBuilder();
            Assert.Equal($"void {GetType().Name}.{nameof(AppendWithFullNameFalseTest)}()", resolvedMethod.Append(sb, false).ToString());
        }
    }
}
