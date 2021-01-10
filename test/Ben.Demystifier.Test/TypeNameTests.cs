using System;
using System.Diagnostics;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class TypeNameTests
    {
        [Fact]
        public void NestedGenericTypes()
        {
            try
            {
                Throw(new Generic<(int, string)>.Nested());
            }
            catch (Exception ex)
            {
                var text = ex.ToStringDemystified();
            }
        }

        private void Throw(Generic<(int a, string b)>.Nested nested) 
        {
            throw null;
        }
    }

    public static class Generic<T> { public struct Nested { } }
}
