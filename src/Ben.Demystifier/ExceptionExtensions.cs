// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Generic.Enumerable;
using System.Reflection;
using System.Text;

namespace System.Diagnostics
{
    public static class ExceptionExtensions
    {
        private static readonly FieldInfo? stackTraceString = typeof(Exception).GetField("_stackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

        private static void SetStackTracesString(this Exception exception, string value)
            => stackTraceString?.SetValue(exception, value);

        /// <summary>
        /// Demystifies the given <paramref name="exception"/> and tracks the original stack traces for the whole exception tree.
        /// </summary>
        public static T Demystify<T>(this T exception) where T : Exception
        {
            try
            {
                var stackTrace = new EnhancedStackTrace(exception);

                if (stackTrace.FrameCount > 0)
                {
                    exception.SetStackTracesString(stackTrace.ToString());
                }

                if (exception is AggregateException aggEx)
                {
                    foreach (var ex in EnumerableIList.Create(aggEx.InnerExceptions))
                    {
                        ex.Demystify();
                    }
                }

                exception.InnerException?.Demystify();
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
            => new StringBuilder().AppendDemystified(exception).ToString();
    }
}
