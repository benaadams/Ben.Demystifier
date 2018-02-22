using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Ben.Demystifier.Test
{
    public sealed class ToDemystifiedStringTests
    {
        private readonly ITestOutputHelper _output;

        public ToDemystifiedStringTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DemystifyShouldNotAffectTheOriginalStackTrace()
        {
            try
            {
                SimpleMethodThatThrows().Wait();
            }
            catch (Exception e)
            {
                var original = e.ToString();
                var stringDemystified = e.ToStringDemystified();

                _output.WriteLine("Demystified: ");
                _output.WriteLine(stringDemystified);

                _output.WriteLine("Original: ");
                var afterDemystified = e.ToString();
                _output.WriteLine(afterDemystified);

                Assert.Equal(original, afterDemystified);
            }

            async Task SimpleMethodThatThrows()
            {
                throw new InvalidOperationException("message");
            }
        }
    }
}
