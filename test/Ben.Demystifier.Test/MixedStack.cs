using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class MixedStack
    {
        [Fact]
        public void ProducesReadableFrames()
        {
            // Arrange
            var exception = GetMixedStackException();

            // Act
            var methodNames = new EnhancedStackTrace(exception)
                .Select(
                    stackFrame => stackFrame.MethodInfo.ToString()
                )
                // Remove Framework method that can be optimized out (inlined)
                .Where(methodName => !methodName.StartsWith("bool System.Collections.Generic.List<T>+"));

            var count = methodNames.Count();
            methodNames = methodNames.Take(count - 1);

            // Assert
            var expected = ExpectedCallStack.ToArray();
            var trace = methodNames.ToArray();

            Assert.Equal(expected.Length, trace.Length);

            for (var i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], trace[i]);
            }
        }

        Exception GetMixedStackException()
        {
            Exception exception = null;
            try
            {
                Start((val: "", true));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            return exception;
        }

        static List<string> ExpectedCallStack = new List<string>()
        {
            "IEnumerable<string> Ben.Demystifier.Test.MixedStack.Iterator()+MoveNext()",
            "string string.Join(string separator, IEnumerable<string> values)",
            "string Ben.Demystifier.Test.MixedStack+GenericClass<T>.GenericMethod<V>(ref V value)",
            "async Task<string> Ben.Demystifier.Test.MixedStack.MethodAsync(int value)",
            "async ValueTask<string> Ben.Demystifier.Test.MixedStack.MethodAsync<TValue>(TValue value)",
            "(string val, bool) Ben.Demystifier.Test.MixedStack.Method(string value)",
            "ref string Ben.Demystifier.Test.MixedStack.RefMethod(string value)",
            "(string val, bool) Ben.Demystifier.Test.MixedStack.s_func(string s, bool b)",
            "void Ben.Demystifier.Test.MixedStack.s_action(string s, bool b)",
            "void Ben.Demystifier.Test.MixedStack.Start((string val, bool) param)"

        };

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static IEnumerable<string> Iterator()
        {
            var list = new List<int>() { 1, 2, 3, 4 };
            foreach (var item in list)
            {
                // Throws the exception
                list.Add(item);

                yield return item.ToString();
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static async Task<string> MethodAsync(int value)
        {
            await Task.Delay(0);
            return GenericClass<byte>.GenericMethod(ref value);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static async ValueTask<string> MethodAsync<TValue>(TValue value) => await MethodAsync(1);

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static (string val, bool) Method(string value) => (MethodAsync(value).GetAwaiter().GetResult(), true);

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static ref string RefMethod(string value)
        {
            Method(value);
            return ref s;
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static void Start((string val, bool) param) => s_action.Invoke(param.val, param.Item2);

        static Action<string, bool> s_action = (string s, bool b) => s_func(s, b);
        static Func<string, bool, (string val, bool)> s_func = (string s, bool b) => (RefMethod(s), b);
        static string s = "";

        static class GenericClass<T>
        {
            [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
            public static string GenericMethod<V>(ref V value)
            {
                var returnVal = "";
                for (var i = 0; i < 10; i++)
                {
                    returnVal += string.Join(", ", Iterator());
                }
                return returnVal;
            }
        }
    }
}
