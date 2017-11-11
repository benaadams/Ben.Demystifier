﻿// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic.Enumerable;
using System.Diagnostics.Internal;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Diagnostics
{
    public partial class EnhancedStackTrace
    {

        private static List<EnhancedStackFrame> GetFrames(Exception exception)
        {
            if (exception == null)
            {
                return new List<EnhancedStackFrame>();
            }

            var needFileInfo = true;
            var stackTrace = new StackTrace(exception, needFileInfo);

            return GetFrames(stackTrace);
        }

        private static List<EnhancedStackFrame> GetFrames(StackTrace stackTrace)
        {
            var frames = new List<EnhancedStackFrame>();
            var stackFrames = stackTrace.GetFrames();

            if (stackFrames == null)
            {
                return frames;
            }

            using (var portablePdbReader = new PortablePdbReader())
            {

                for (var i = 0; i < stackFrames.Length; i++)
                {
                    var frame = stackFrames[i];
                    var method = frame.GetMethod();

                    // Always show last stackFrame
                    if (!ShowInStackTrace(method) && i < stackFrames.Length - 1)
                    {
                        continue;
                    }

                    var fileName = frame.GetFileName();
                    var row = frame.GetFileLineNumber();
                    var column = frame.GetFileColumnNumber();
                    var ilOffset = frame.GetILOffset();
                    if (string.IsNullOrEmpty(fileName) && ilOffset >= 0)
                    {
                        // .NET Framework and older versions of mono don't support portable PDBs
                        // so we read it manually to get file name and line information
                        portablePdbReader.PopulateStackFrame(frame, method, frame.GetILOffset(), out fileName, out row, out column);
                    }

                    var stackFrame = new EnhancedStackFrame(frame, GetMethodDisplayString(method), fileName, row, column);


                    frames.Add(stackFrame);
                }

                return frames;
            }
        }

        private static ResolvedMethod GetMethodDisplayString(MethodBase originMethod)
        {
            // Special case: no method available
            if (originMethod == null)
            {
                return null;
            }

            MethodBase method = originMethod;

            var methodDisplayInfo = new ResolvedMethod();
            methodDisplayInfo.SubMethodBase = method;

            // Type name
            var type = method.DeclaringType;

            var subMethodName = method.Name;
            var methodName = method.Name;

            if (type != null && type.IsDefined(typeof(CompilerGeneratedAttribute)) &&
                (typeof(IAsyncStateMachine).IsAssignableFrom(type) || typeof(IEnumerator).IsAssignableFrom(type)))
            {
                methodDisplayInfo.IsAsync = typeof(IAsyncStateMachine).IsAssignableFrom(type);

                // Convert StateMachine methods to correct overload +MoveNext()
                if (!TryResolveStateMachineMethod(ref method, out type))
                {
                    methodDisplayInfo.SubMethodBase = null;
                    subMethodName = null;
                }

                methodName = method.Name;
            }

            // Method name
            methodDisplayInfo.MethodBase = method;
            methodDisplayInfo.Name = methodName;
            if (method.Name.IndexOf("<") >= 0)
            {
                if (TryResolveGeneratedName(ref method, out type, out methodName, out subMethodName, out var kind, out var ordinal))
                {
                    methodName = method.Name;
                    methodDisplayInfo.MethodBase = method;
                    methodDisplayInfo.Name = methodName;
                    methodDisplayInfo.Ordinal = ordinal;
                }
                else
                {
                    methodDisplayInfo.MethodBase = null;
                }

                methodDisplayInfo.IsLambda = (kind == GeneratedNameKind.LambdaMethod);

                if (methodDisplayInfo.IsLambda && type != null)
                {
                    if (methodName == ".cctor")
                    {
                        var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        foreach (var field in fields)
                        {
                            var value = field.GetValue(field);
                            if (value is Delegate d)
                            {
                                if (ReferenceEquals(d.Method, originMethod) &&
                                    d.Target.ToString() == originMethod.DeclaringType.ToString())
                                {
                                    methodDisplayInfo.Name = field.Name;
                                    methodDisplayInfo.IsLambda = false;
                                    method = originMethod;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (subMethodName != methodName)
            {
                methodDisplayInfo.SubMethod = subMethodName;
            }

            // ResolveStateMachineMethod may have set declaringType to null
            if (type != null)
            {
                var declaringTypeName =  TypeNameHelper.GetTypeDisplayName(type, fullName: true, includeGenericParameterNames: true);
                methodDisplayInfo.DeclaringTypeName = declaringTypeName;
            }

            if (method is System.Reflection.MethodInfo mi)
            {
                var returnParameter = mi.ReturnParameter;
                if (returnParameter != null)
                {
                    methodDisplayInfo.ReturnParameter = GetParameter(mi.ReturnParameter);
                }
                else if (mi.ReturnType != null)
                {
                    methodDisplayInfo.ReturnParameter = new ResolvedParameter
                    {
                        Prefix = "",
                        Name = "",
                        Type = TypeNameHelper.GetTypeDisplayName(mi.ReturnType, fullName: false, includeGenericParameterNames: true).ToString(),
                    };
                }
            }

            if (method.IsGenericMethod)
            {
                var genericArguments = string.Join(", ", method.GetGenericArguments()
                    .Select(arg => TypeNameHelper.GetTypeDisplayName(arg, fullName: false, includeGenericParameterNames: true)));
                methodDisplayInfo.GenericArguments += "<" + genericArguments + ">";
            }

            // Method parameters
            var parameters = method.GetParameters();
            if (parameters.Length > 0)
            {
                var parameterList = new List<ResolvedParameter>(parameters.Length);
                foreach (var parameter in parameters)
                {
                    parameterList.Add(GetParameter(parameter));
                }

                methodDisplayInfo.Parameters = parameterList;
            }

            if (methodDisplayInfo.SubMethodBase == methodDisplayInfo.MethodBase)
            {
                methodDisplayInfo.SubMethodBase = null;
            }
            else if (methodDisplayInfo.SubMethodBase != null)
            {
                parameters = methodDisplayInfo.SubMethodBase.GetParameters();
                if (parameters.Length > 0)
                {
                    var parameterList = new List<ResolvedParameter>(parameters.Length);
                    foreach (var parameter in parameters)
                    {
                        var param = GetParameter(parameter);
                        if (param.Name?.StartsWith("<") ?? true) continue;

                        parameterList.Add(param);
                    }

                    methodDisplayInfo.SubMethodParameters = parameterList;
                }
            }

            return methodDisplayInfo;
        }

        private static bool TryResolveGeneratedName(ref MethodBase method, out Type type, out string methodName, out string subMethodName, out GeneratedNameKind kind, out int? ordinal)
        {
            kind = GeneratedNameKind.None;
            type = method.DeclaringType;
            subMethodName = null;
            ordinal = null;
            methodName = method.Name;

            var generatedName = methodName;

            if (!TryParseGeneratedName(generatedName, out kind, out int openBracketOffset, out int closeBracketOffset))
            {
                return false;
            }

            methodName = generatedName.Substring(openBracketOffset + 1, closeBracketOffset - openBracketOffset - 1);

            switch (kind)
            {
                case GeneratedNameKind.LocalFunction:
                    {
                        var localNameStart = generatedName.IndexOf((char)kind, closeBracketOffset + 1);
                        if (localNameStart < 0) break;
                        localNameStart += 3;

                        if (localNameStart < generatedName.Length)
                        {
                            var localNameEnd = generatedName.IndexOf("|", localNameStart);
                            if (localNameEnd > 0)
                            {
                                subMethodName = generatedName.Substring(localNameStart, localNameEnd - localNameStart);
                            }
                        }
                        break;
                    }
                case GeneratedNameKind.LambdaMethod:
                    subMethodName = "";
                    break;
            }

            var dt = method.DeclaringType;
            if (dt == null)
            {
                return false;
            }

            var matchHint = GetMatchHint(kind, method);

            var matchName = methodName;

            var candidateMethods = dt.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.Name == matchName);
            if (TryResolveSourceMethod(candidateMethods, kind, matchHint, ref method, ref type, out ordinal)) return true;

            var candidateConstructors = dt.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.Name == matchName);
            if (TryResolveSourceMethod(candidateConstructors, kind, matchHint, ref method, ref type, out ordinal)) return true;

            dt = dt.DeclaringType;
            if (dt == null)
            {
                return false;
            }

            candidateMethods = dt.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.Name == matchName);
            if (TryResolveSourceMethod(candidateMethods, kind, matchHint, ref method, ref type, out ordinal)) return true;

            candidateConstructors = dt.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => m.Name == matchName);
            if (TryResolveSourceMethod(candidateConstructors, kind, matchHint, ref method, ref type, out ordinal)) return true;

            return false;
        }

        private static bool TryResolveSourceMethod(IEnumerable<MethodBase> candidateMethods, GeneratedNameKind kind, string matchHint, ref MethodBase method, ref Type type, out int? ordinal)
        {
            ordinal = null;
            foreach (var candidateMethod in candidateMethods)
            {
                var nethodBody = candidateMethod.GetMethodBody();
                if (kind == GeneratedNameKind.LambdaMethod)
                {
                    foreach (var v in EnumerableIList.Create(nethodBody?.LocalVariables))
                    {
                        if (v.LocalType == type)
                        {
                            GetOrdinal(method, ref ordinal);

                        }
                        method = candidateMethod;
                        type = method.DeclaringType;
                        return true;
                    }
                }


                var rawIL = nethodBody?.GetILAsByteArray();
                if (rawIL == null) continue;

                var reader = new ILReader(rawIL);
                while (reader.Read(candidateMethod))
                {
                    if (reader.Operand is MethodBase mb)
                    {
                        if (method == mb || (matchHint != null && method.Name.Contains(matchHint)))
                        {
                            if (kind == GeneratedNameKind.LambdaMethod)
                            {
                                GetOrdinal(method, ref ordinal);
                            }

                            method = candidateMethod;
                            type = method.DeclaringType;
                            return true;
                        }
                    }
                    else if (reader.Operand is Type t)
                    {
                        if (t == type)
                        {
                            method = candidateMethod;
                            type = method.DeclaringType;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static void GetOrdinal(MethodBase method, ref int? ordinal)
        {
            var lamdaStart = method.Name.IndexOf((char) GeneratedNameKind.LambdaMethod + "__") + 3;
            if (lamdaStart > 3)
            {
                var secondStart = method.Name.IndexOf("_", lamdaStart) + 1;
                if (secondStart > 0)
                {
                    lamdaStart = secondStart;
                }

                if (!int.TryParse(method.Name.Substring(lamdaStart), out var foundOrdinal))
                {
                    ordinal = null;
                    return;
                }

                ordinal = foundOrdinal;

                var methods = method.DeclaringType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                var startName = method.Name.Substring(0, lamdaStart);
                var count = 0;
                foreach (var m in methods)
                {
                    if (m.Name.Length > lamdaStart && m.Name.StartsWith(startName))
                    {
                        count++;

                        if (count > 1)
                        {
                            break;
                        }
                    }
                }


                if (count <= 1)
                {
                    ordinal = null;
                }
            }
        }

        static string GetMatchHint(GeneratedNameKind kind, MethodBase method)
        {
            var methodName = method.Name;

            switch (kind)
            {
                case GeneratedNameKind.LocalFunction:
                    var start = methodName.IndexOf("|");
                    if (start < 1) return null;
                    var end = methodName.IndexOf("_", start) + 1;
                    if (end <= start) return null;

                    return methodName.Substring(start, end - start);
            }
            return null;
        }

        // Parse the generated name. Returns true for names of the form
        // [CS$]<[middle]>c[__[suffix]] where [CS$] is included for certain
        // generated names, where [middle] and [__[suffix]] are optional,
        // and where c is a single character in [1-9a-z]
        // (csharp\LanguageAnalysis\LIB\SpecialName.cpp).
        internal static bool TryParseGeneratedName(
            string name,
            out GeneratedNameKind kind,
            out int openBracketOffset,
            out int closeBracketOffset)
        {
            openBracketOffset = -1;
            if (name.StartsWith("CS$<", StringComparison.Ordinal))
            {
                openBracketOffset = 3;
            }
            else if (name.StartsWith("<", StringComparison.Ordinal))
            {
                openBracketOffset = 0;
            }

            if (openBracketOffset >= 0)
            {
                closeBracketOffset = IndexOfBalancedParenthesis(name, openBracketOffset, '>');
                if (closeBracketOffset >= 0 && closeBracketOffset + 1 < name.Length)
                {
                    int c = name[closeBracketOffset + 1];
                    if ((c >= '1' && c <= '9') || (c >= 'a' && c <= 'z')) // Note '0' is not special.
                    {
                        kind = (GeneratedNameKind)c;
                        return true;
                    }
                }
            }

            kind = GeneratedNameKind.None;
            openBracketOffset = -1;
            closeBracketOffset = -1;
            return false;
        }


        private static int IndexOfBalancedParenthesis(string str, int openingOffset, char closing)
        {
            char opening = str[openingOffset];

            int depth = 1;
            for (int i = openingOffset + 1; i < str.Length; i++)
            {
                var c = str[i];
                if (c == opening)
                {
                    depth++;
                }
                else if (c == closing)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static string GetPrefix(ParameterInfo parameter, Type parameterType)
        {
            if (parameter.IsOut)
            {
                return "out";
            }
            else if (parameterType != null && parameterType.IsByRef)
            {
                var attribs = parameter.GetCustomAttributes(inherit: false);
                if (attribs?.Length > 0)
                {
                    foreach (var attrib in attribs)
                    {
                        if (attrib is Attribute att && att.GetType().Namespace == "System.Runtime.CompilerServices" && att.GetType().Name == "IsReadOnlyAttribute")
                        {
                            return "in";
                        }
                    }
                }

                return "ref";
            }

            return string.Empty;
        }

        private static ResolvedParameter GetParameter(ParameterInfo parameter)
        {
            var parameterType = parameter.ParameterType;

            var prefix = GetPrefix(parameter, parameterType);

            var parameterTypeString = "?";
            if (parameterType != null)
            {
                if (parameterType.IsGenericType)
                {
                    var tupleNames = parameter.GetCustomAttributes<TupleElementNamesAttribute>().FirstOrDefault()?.TransformNames;
                    if (tupleNames != null)
                    {
                        var sb = new StringBuilder();
                        sb.Append("(");
                        var args = parameterType.GetGenericArguments();
                        for (var i = 0; i < args.Length; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append(", ");
                            }
                            sb.Append(TypeNameHelper.GetTypeDisplayName(args[i], fullName: false, includeGenericParameterNames: true));

                            if (i >= tupleNames.Count) continue;

                            var argName = tupleNames[i];
                            if (argName != null)
                            {
                                sb.Append(" ");
                                sb.Append(argName);
                            }
                        }

                        sb.Append(")");
                        parameterTypeString = sb.ToString();

                        return new ResolvedParameter
                        {
                            Prefix = prefix,
                            Name = parameter.Name,
                            Type = parameterTypeString,
                        };
                    }
                }

                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }

                parameterTypeString = TypeNameHelper.GetTypeDisplayName(parameterType, fullName: false, includeGenericParameterNames: true);

            }

            return new ResolvedParameter
            {
                Prefix = prefix,
                Name = parameter.Name,
                Type = parameterTypeString,
            };
        }

        private static bool ShowInStackTrace(MethodBase method)
        {
            Debug.Assert(method != null);
            try
            {
                var type = method.DeclaringType;
                if (type == typeof(Task<>) && method.Name == "InnerInvoke")
                {
                    return false;
                }
                if (type == typeof(Task) && method.Name == "ExecuteWithThreadLocal")
                {
                    return false;
                }

                // Don't show any methods marked with the StackTraceHiddenAttribute
                // https://github.com/dotnet/coreclr/pull/14652
                foreach (var attibute in EnumerableIList.Create(method.GetCustomAttributesData()))
                {
                    // internal Attribute, match on name
                    if (attibute.AttributeType.Name == "StackTraceHiddenAttribute")
                    {
                        return false;
                    }
                }

                if (type == null)
                {
                    return true;
                }

                foreach (var attibute in EnumerableIList.Create(type.GetCustomAttributesData()))
                {
                    // internal Attribute, match on name
                    if (attibute.AttributeType.Name == "StackTraceHiddenAttribute")
                    {
                        return false;
                    }
                }

                // Fallbacks for runtime pre-StackTraceHiddenAttribute
                if (type == typeof(ExceptionDispatchInfo) && method.Name == "Throw")
                {
                    return false;
                }
                else if (type == typeof(TaskAwaiter) ||
                    type == typeof(TaskAwaiter<>) ||
                    type == typeof(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter) ||
                    type == typeof(ConfiguredTaskAwaitable<>.ConfiguredTaskAwaiter))
                {
                    switch (method.Name)
                    {
                        case "HandleNonSuccessAndDebuggerNotification":
                        case "ThrowForNonSuccess":
                        case "ValidateEnd":
                        case "GetResult":
                            return false;
                    }
                }
                else if (type.FullName == "System.ThrowHelper")
                {
                    return false;
                }
            }
            catch
            {
                // GetCustomAttributesData can throw
                return true;
            }

            return true;
        }

        private static bool TryResolveStateMachineMethod(ref MethodBase method, out Type declaringType)
        {
            Debug.Assert(method != null);
            Debug.Assert(method.DeclaringType != null);

            declaringType = method.DeclaringType;

            var parentType = declaringType.DeclaringType;
            if (parentType == null)
            {
                return false;
            }

            var methods = parentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (methods == null)
            {
                return false;
            }

            foreach (var candidateMethod in methods)
            {
                var attributes = candidateMethod.GetCustomAttributes<StateMachineAttribute>();
                if (attributes == null)
                {
                    continue;
                }

                foreach (var asma in attributes)
                {
                    if (asma.StateMachineType == declaringType)
                    {
                        method = candidateMethod;
                        declaringType = candidateMethod.DeclaringType;
                        // Mark the iterator as changed; so it gets the + annotation of the original method
                        // async statemachines resolve directly to their builder methods so aren't marked as changed
                        return asma is IteratorStateMachineAttribute;
                    }
                }
            }

            return false;
        }

        internal enum GeneratedNameKind
        {
            None = 0,

            // Used by EE:
            ThisProxyField = '4',
            HoistedLocalField = '5',
            DisplayClassLocalOrField = '8',
            LambdaMethod = 'b',
            LambdaDisplayClass = 'c',
            StateMachineType = 'd',
            LocalFunction = 'g', // note collision with Deprecated_InitializerLocal, however this one is only used for method names

            // Used by EnC:
            AwaiterField = 'u',
            HoistedSynthesizedLocalField = 's',

            // Currently not parsed:
            StateMachineStateField = '1',
            IteratorCurrentBackingField = '2',
            StateMachineParameterProxyField = '3',
            ReusableHoistedLocalField = '7',
            LambdaCacheField = '9',
            FixedBufferField = 'e',
            AnonymousType = 'f',
            TransparentIdentifier = 'h',
            AnonymousTypeField = 'i',
            AutoPropertyBackingField = 'k',
            IteratorCurrentThreadIdField = 'l',
            IteratorFinallyMethod = 'm',
            BaseMethodWrapper = 'n',
            AsyncBuilderField = 't',
            DynamicCallSiteContainerType = 'o',
            DynamicCallSiteField = 'p'
        }
    }
}
