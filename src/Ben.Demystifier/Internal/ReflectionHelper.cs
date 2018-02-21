// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using System.Reflection.Emit;

namespace System.Diagnostics.Internal
{
    /// <summary>
    /// A helper class that contains utilities methods for dealing with reflection.
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Returns true if <paramref name="type"/> is <code>System.Runtime.CompilerServices.IsReadOnlyAttribute</code>.
        /// </summary>
        public static bool IsReadOnlyAttribute(this Type type)
        {
            return type.Namespace == "System.Runtime.CompilerServices" && type.Name == "IsReadOnlyAttribute";
        }

        /// <summary>
        /// Returns true if the <paramref name="type"/> is a value tuple type.
        /// </summary>
        public static bool IsValueTuple(this Type type)
        {
            return type.Namespace == "System" && type.Name.Contains("ValueTuple`");
        }
    }
}
