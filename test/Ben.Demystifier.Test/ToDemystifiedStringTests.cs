using System;
using System.Diagnostics;
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
                SimpleMethodThatThrows(null).Wait();
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

            async Task SimpleMethodThatThrows(string value)
            {
                if (value == null)
                {
                    throw new InvalidOperationException("message");
                }

                await Task.Yield();
            }
        }


        [Fact]
        public void DemystifyKeepsMessage()
        {
            Exception ex = null;
            try
            {
                throw new InvalidOperationException("aaa")
                {
                    Data =
                    {
                        ["bbb"] = "ccc",
                        ["ddd"] = "eee",
                    }
                };
            }
            catch (Exception e)
            {
                ex = e;
            }

            var original = ex.ToString();
            var endLine = (int)Math.Min((uint)original.IndexOf('\n'), original.Length);

            original = original.Substring(0, endLine);

            var stringDemystified = ex.ToStringDemystified();
            endLine = (int)Math.Min((uint)stringDemystified.IndexOf('\n'), stringDemystified.Length);

            stringDemystified = stringDemystified.Substring(0, endLine);

            Assert.Equal(original, stringDemystified);
        }
    }
}
