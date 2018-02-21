using System.Text.RegularExpressions;

namespace Ben.Demystifier.Test
{
    internal static class LineEndingsHelper
    {
        private static readonly Regex ReplaceLineEndings = new Regex(" in [^\n\r]+");

        public static string RemoveLineEndings(string original)
        {
            return ReplaceLineEndings.Replace(original, "");
        }
    }
}
