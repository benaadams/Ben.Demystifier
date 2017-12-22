// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic.Enumerable;
using System.Reflection;
using System.Text;

namespace System.Diagnostics
{
    public class ResolvedMethod
    {
        public MethodBase MethodBase { get; set; }

        public string DeclaringTypeName { get; set; }

        public bool IsAsync { get; set; }

        public bool IsLambda { get; set; }

        public ResolvedParameter ReturnParameter { get; set; }

        public string Name { get; set; }

        public int? Ordinal { get; set; }

        public string GenericArguments { get; set; }

        public Type[] ResolvedGenericArguments { get; set; }

        public MethodBase SubMethodBase { get; set; }

        public string SubMethod { get; set; }

        public EnumerableIList<ResolvedParameter> Parameters { get; set; }

        public EnumerableIList<ResolvedParameter> SubMethodParameters { get; set; }

        public override string ToString() => Append(new StringBuilder()).ToString();

        internal StringBuilder Append(StringBuilder builder)
        {

            if (IsAsync)
            {
                builder
                    .Append("async ");
            }

            if (ReturnParameter != null)
            {
                ReturnParameter.Append(builder);
                builder.Append(" ");
            }

            if (!string.IsNullOrEmpty(DeclaringTypeName))
            {

                if (Name == ".ctor")
                {
                    if (string.IsNullOrEmpty(SubMethod) && !IsLambda)
                        builder.Append("new ");

                    builder.Append(DeclaringTypeName);
                }
                else if (Name == ".cctor")
                {
                    builder.Append("static ");
                    builder.Append(DeclaringTypeName);
                }
                else
                {
                    builder
                        .Append(DeclaringTypeName)
                        .Append(".")
                        .Append(Name);
                }
            }
            else
            {
                builder.Append(Name);
            }
            builder.Append(GenericArguments);

            builder.Append("(");
            if (MethodBase != null)
            {
                var isFirst = true;
                foreach(var param in Parameters)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        builder.Append(", ");
                    }
                    param.Append(builder);
                }
            }
            else
            {
                builder.Append("?");
            }
            builder.Append(")");

            if (!string.IsNullOrEmpty(SubMethod) || IsLambda)
            {
                builder.Append("+");
                builder.Append(SubMethod);
                builder.Append("(");
                if (SubMethodBase != null)
                {
                    var isFirst = true;
                    foreach (var param in SubMethodParameters)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            builder.Append(", ");
                        }
                        param.Append(builder);
                    }
                }
                else
                {
                    builder.Append("?");
                }
                builder.Append(")");
                if (IsLambda)
                {
                    builder.Append(" => { }");

                    if (Ordinal.HasValue)
                    {
                        builder.Append(" [");
                        builder.Append(Ordinal);
                        builder.Append("]");
                    }
                }
            }

            return builder;
        }
    }
}
