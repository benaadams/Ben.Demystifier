using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

        private static class ThrowsInCtor
        {
#pragma warning disable CS0649
            public static readonly string Field;
#pragma warning restore CS0649

            static ThrowsInCtor()
            {
                var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                    .Select(x => new RegionInfo("qwerty")) //this will throw
                    .Where(x => x.DisplayName != "World").ToList();
            }
        }

        [Fact]
        public void ThrowsInCtorMethodDisplayString()
        {
            var type = typeof(ThrowsInCtor).GetNestedTypes(BindingFlags.NonPublic).Single();
            var method = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(x => x.ReturnType == typeof(bool));
            var displayString = EnhancedStackTrace.GetMethodDisplayString(method);
            Assert.StartsWith(".cctor", displayString.Name!);
        }
    }
}
