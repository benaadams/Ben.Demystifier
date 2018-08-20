namespace System.Diagnostics.Internal
{
    public interface IPortablePdbReader : IDisposable
    {
        void PopulateStackFrame(bool isMethodDynamic, string location, int methodToken, int IlOffset, out string fileName, out int row, out int column);
    }
}
