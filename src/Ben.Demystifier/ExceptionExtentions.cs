// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;

namespace System.Diagnostics
{
    public static class ExceptionExtentions
    {
        private static readonly FieldInfo stackTraceString = typeof(Exception).GetField("_stackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

        public static T Demystify<T>(this T exception) where T : Exception
        {
            try
            {
                var stackTrace = new EnhancedStackTrace(exception);

                if (stackTrace.FrameCount > 0)
                {
                    stackTraceString.SetValue(exception, stackTrace.ToString());
                }

                exception.InnerException?.Demystify();
            }
            catch
            {
                // Processing exceptions shouldn't throw exceptions; if it fails
            }

            return exception;
        }
    }
}
