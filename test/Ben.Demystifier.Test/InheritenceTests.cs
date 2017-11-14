using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class InheritenceTests
    {
        private abstract class BaseClass
        {
            public abstract Task<object> Method();
        }

        private class ImplClass : BaseClass
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public override Task<object> Method()
            {
                throw new Exception();
            }
        }

        [Fact]
        public async Task ImplementedAbstractMethodDoesNotThrow()
        {
            // Arrange
            var instance = new ImplClass();

            // Act
            Exception exception = null;
            try
            {
                await instance.Method();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Act
            var est = new EnhancedStackTrace(exception);
        }
    }
}
