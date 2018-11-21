// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Generic.Enumerable;
using System.Reflection;

namespace System.Diagnostics
{
    /// <nodoc />
    public static class ExceptionExtentions
    {
        private static readonly FieldInfo stackTraceString = typeof(Exception).GetField("_stackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

        public static T Demystify<T>(this T exception) where T : Exception
            => Demystify(exception, originalStacksTracker: null);

        private static string GetStackTracesString(this Exception exception)
            => (string)stackTraceString.GetValue(exception);

        private static void SetStackTracesString(this Exception exception, string value)
            => stackTraceString.SetValue(exception, value);

        /// <summary>
        /// Demystifies the given <paramref name="exception"/> and tracks the original stack traces for the whole exception tree.
        /// </summary>
        private static T Demystify<T>(this T exception, Dictionary<Exception, string> originalStacksTracker) where T : Exception
        {
            try
            {
                if (originalStacksTracker?.ContainsKey(exception) == false)
                {
                    originalStacksTracker[exception] = exception.GetStackTracesString();
                }

                var stackTrace = new EnhancedStackTrace(exception);

                if (stackTrace.FrameCount > 0)
                {
                    exception.SetStackTracesString(stackTrace.ToString());
                }

                if (exception is AggregateException aggEx)
                {
                    foreach (var ex in EnumerableIList.Create(aggEx.InnerExceptions))
                    {
                        ex.Demystify(originalStacksTracker);
                    }
                }

                exception.InnerException?.Demystify(originalStacksTracker);
            }
            catch
            {
                // Processing exceptions shouldn't throw exceptions; if it fails
            }

            return exception;
        }

        /// <summary>
        /// Gets demystified string representation of the <paramref name="exception"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="Demystify{T}"/> method mutates the exception instance that can cause
        /// issues if a system relies on the stack trace be in the specific form.
        /// Unlike <see cref="Demystify{T}"/> this method is pure. It calls <see cref="Demystify{T}"/> first,
        /// computes a demystified string representation and then restores the original state of the exception back.
        /// </remarks>
        [Contracts.Pure]
        public static string ToStringDemystified(this Exception exception)
        {
            try
            {
                var originalStacks = new Dictionary<Exception, string>();
                exception.Demystify(originalStacks);

                var result = exception.ToString();

                foreach (var kvp in originalStacks)
                {
                    kvp.Key.SetStackTracesString(kvp.Value);
                }

                return result;
            }
            catch
            {
                // Processing exceptions shouldn't throw exceptions; if it fails
            }

            return exception.ToString();
        }
    }
}
