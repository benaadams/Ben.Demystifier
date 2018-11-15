// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;

namespace System.Diagnostics
{
    public class ResolvedParameter
    {
        public string Name { get; set; }

        public Type ResolvedType { get; set; }

        public string Prefix { get; set; }

        public override string ToString() => Append(new StringBuilder()).ToString();

        internal StringBuilder Append(StringBuilder sb)
        {
            if (!string.IsNullOrEmpty(Prefix))
            {
                sb.Append(Prefix)
                  .Append(" ");
            }

            if (ResolvedType != null)
            {
                AppendTypeName(sb);
            }
            else
            {
                sb.Append("?");
            }

            if (!string.IsNullOrEmpty(Name))
            {
                sb.Append(" ")
                  .Append(Name);
            }

            return sb;
        }

        protected virtual void AppendTypeName(StringBuilder sb) 
        {
            sb.AppendTypeDisplayName(ResolvedType, fullName: false, includeGenericParameterNames: true);
        }
    }
}
