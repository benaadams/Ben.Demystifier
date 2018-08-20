using System.IO;
using System.Reflection;

namespace VsixTesting.Demystifier
{
    class Util
    {
        internal static Assembly LoadEmbeddedAssembly(string fileName)
        {
            var resourceName = Assembly.GetExecutingAssembly().GetName().Name + "." + fileName;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    var buffer = new BinaryReader(stream).ReadBytes((int)stream.Length);
                    return Assembly.Load(buffer);
                }
            }

            return null;
        }
    }
}
