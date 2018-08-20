using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace VsixTesting.Demystifier
{
    class AssemblyResolver
    {
        private readonly static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        static public void Install()
        {
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object _, ResolveEventArgs args)
            {
                var name = new AssemblyName(args.Name).Name;
                var asm = assemblies.ContainsKey(name) ? assemblies[name] : null;
                return asm;
            };
        }

        internal static void Load(string fileName)
        {
            var assembly = Util.LoadEmbeddedAssembly(fileName);
            assemblies[assembly.GetName().Name] = assembly;
        }
    }
}
