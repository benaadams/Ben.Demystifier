using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class GenericMethodDisplayStringTests
    {
        private static class Example<T>
        {
            // ReSharper disable once StaticMemberInGenericType
            public static readonly StackFrame StackFrame; 
            
            static Example()
            {
                var fun = new Func<StackFrame>(() => new StackFrame(0, true));
                
                StackFrame = fun();
                
            }

        }

        [Fact]
        public void DiagnosesGenericMethodDisplayString()
        {
            var sf = Example<Type>.StackFrame;

            try
            {
                var s = EnhancedStackTrace.GetMethodDisplayString(sf.GetMethod());
                Assert.True(true, "Does not throw exception when diagnosing generic method display string.");
            }
            catch (Exception)
            {
                Assert.True(false, "Must not throw an exception when diagnosing generic method display string.");
            }

        }
    }
}
