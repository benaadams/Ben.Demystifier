// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Internal;
using System.Text;

namespace System.Diagnostics 
{
    public class ValueTupleResolvedParameter : ResolvedParameter
    {
        public IList<string> TupleNames { get; }

        public ValueTupleResolvedParameter(Type resolvedType, IList<string> tupleNames) 
            : base(resolvedType) 
            => TupleNames = tupleNames;

        protected override void AppendTypeName(StringBuilder sb)
        {
            if (ResolvedType is not null)
            {
                if (ResolvedType.IsValueTuple())
                {
                    AppendValueTupleParameterName(sb, ResolvedType);
                }
                else
                {
                    // Need to unwrap the first generic argument first.
                    sb.Append(TypeNameHelper.GetTypeNameForGenericType(ResolvedType));
                    sb.Append("<");
                    AppendValueTupleParameterName(sb, ResolvedType.GetGenericArguments()[0]);
                    sb.Append(">");
                }
            }
        }

        private void AppendValueTupleParameterName(StringBuilder sb, Type parameterType)
        {
            sb.Append("(");
            var args = parameterType.GetGenericArguments();
            for (var i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.AppendTypeDisplayName(args[i], fullName: false, includeGenericParameterNames: true);

                if (i >= TupleNames.Count)
                {
                    continue;
                }

                var argName = TupleNames[i];
                if (argName == null)
                {
                    continue;
                }

                sb.Append(" ");
                sb.Append(argName);
            }

            sb.Append(")");
        }
    }
}
