// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic.Enumerable;
using System.Text;

namespace System.Diagnostics 
{
    public static class StringBuilderExtentions
    {
        public static StringBuilder AppendDemystified(this StringBuilder builder, Exception exception)
        {
            try
            {
                var stackTrace = new EnhancedStackTrace(exception);

                builder.Append(exception.GetType());
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    builder.Append(": ").Append(exception.Message);
                }
                builder.Append(Environment.NewLine);

                if (stackTrace.FrameCount > 0)
                {
                    stackTrace.Append(builder);
                }

                if (exception is AggregateException aggEx)
                {
                    foreach (var ex in EnumerableIList.Create(aggEx.InnerExceptions))
                    {
                        builder.AppendInnerException(ex);
                    }
                }

                if (exception.InnerException != null)
                {
                    builder.AppendInnerException(exception.InnerException);
                }
            }
            catch
            {
                // Processing exceptions shouldn't throw exceptions; if it fails
            }

            return builder;
        }

        private static void AppendInnerException(this StringBuilder builder, Exception exception) 
            => builder.Append(" ---> ")
                .AppendDemystified(exception)
                .AppendLine()
                .Append("   --- End of inner exception stack trace ---");
    }
}
