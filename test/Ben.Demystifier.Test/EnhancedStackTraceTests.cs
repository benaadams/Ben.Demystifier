using System.Diagnostics;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class EnhancedStackTraceTests
    {
        [Theory]
        [InlineData(@"file://Sources\MySolution\Foo.cs", @"/MySolution/Foo.cs")]
        [InlineData(@"d:\Public\Src\Foo.cs", @"d:/Public/Src/Foo.cs")]
        // To be deterministic, the C# compiler can take a /pathmap command line option.
        // This option force the compiler to emit the same bits even when their built from the
        // differrent locations.
        // The binaries built with the pathmap usually don't have an absolute path,
        // but have some prefix like \.\.
        // This test case makes sure that EhancedStackTrace can deal with such kind of paths.
        [InlineData(@"\.\Public\Src\Foo.cs", @"/./Public/Src/Foo.cs")]
        public void RelativePathIsConvertedToAnAbsolutePath(string original, string expected)
        {
            var converted = EnhancedStackTrace.TryGetFullPath(original);
            Assert.Equal(expected, NormalizePath(converted));
        }

        [Theory]
        [InlineData(@"file://Sources\My 100%.Done+Solution\Foo`1.cs", @"/My 100%.Done+Solution/Foo`1.cs", false)]
        [InlineData(@"d:\Public Files+50%.Done\Src\Foo`1.cs", @"d:/Public Files+50%.Done/Src/Foo`1.cs", false)]
        [InlineData(@"\.\Public Files+50%.Done\Src\Foo`1.cs", @"/./Public Files+50%.Done/Src/Foo`1.cs", true)]
        public void SpecialPathCharactersAreHandledCorrectly(string original, string expected, bool normalize)
        {
            var converted = EnhancedStackTrace.TryGetFullPath(original);
            if (normalize)
            {
                converted = NormalizePath(converted);
            }

            Assert.Equal(expected, converted);
        }

        // Used in tests to avoid platform-specific issues.
        private static string NormalizePath(string path)
            => path.Replace("\\", "/");
    }
}
