#if HAS_ASYNC_ENUMERATOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Ben.Demystifier.Test
{
    public class AsyncEnumerableTests
    {
        [Fact]
        public async Task DemystifiesAsyncEnumerable()
        {
            Exception demystifiedException = null;

            try
            {
                await foreach (var val in Start(CancellationToken.None))
                {
                    _ = val;
                }
            }
            catch (Exception ex)
            {
                demystifiedException = ex.Demystify();
            }

            // Assert
            var stackTrace = demystifiedException.ToString();
            stackTrace = LineEndingsHelper.RemoveLineEndings(stackTrace);
            var trace = string.Join("", stackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Regex.Replace(s, " x [0-9]+", " x N"))
                .Skip(1)
                .ToArray());
            var expected = string.Join("", new[] {
                "   at async IAsyncEnumerable<int> Ben.Demystifier.Test.AsyncEnumerableTests.Throw(CancellationToken cancellationToken)+MoveNext()",
                "   at async IAsyncEnumerable<long> Ben.Demystifier.Test.AsyncEnumerableTests.Start(CancellationToken cancellationToken)+MoveNext() x N",
                "   at async Task Ben.Demystifier.Test.AsyncEnumerableTests.DemystifiesAsyncEnumerable() x N"
            });
            Assert.Equal(expected, trace);
        }

        async IAsyncEnumerable<long> Start([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            yield return 1;
            await foreach (var @throw in Throw(cancellationToken))
            {
                yield return @throw;
            }
        }

        async IAsyncEnumerable<int> Throw([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield return 2;
            await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException();
        }
    }
}
#endif
