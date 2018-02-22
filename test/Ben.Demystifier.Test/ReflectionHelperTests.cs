using System;
using System.Diagnostics.Internal;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class ReflectionHelperTest
    {
        [Fact]
        public void IsValueTupleReturnsTrueForTupleWith1Element()
        {
            Assert.True(typeof(ValueTuple<int>).IsValueTuple());
        }

        [Fact]
        public void IsValueTupleReturnsTrueForTupleWith1ElementWithOpenedType()
        {
            Assert.True(typeof(ValueTuple<>).IsValueTuple());
        }

        [Fact]
        public void IsValueTupleReturnsTrueForTupleWith6ElementsWithOpenedType()
        {
            Assert.True(typeof(ValueTuple<,,,,,>).IsValueTuple());
        }
    }
}
