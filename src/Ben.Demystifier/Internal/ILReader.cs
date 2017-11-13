using System.Reflection;
using System.Reflection.Emit;

namespace System.Diagnostics.Internal
{
    internal class ILReader
    {
        private static OpCode[] singleByteOpCode;
        private static OpCode[] doubleByteOpCode;

        private readonly byte[] _cil;
        private int ptr;


        public ILReader(byte[] cil)
        {
            _cil = cil;
        }

        public OpCode OpCode { get; private set; }
        public int MetadataToken { get; private set; }
        public MemberInfo Operand { get; private set; }

        public bool Read(MethodBase methodInfo)
        {
            if (ptr < _cil.Length)
            {
                OpCode = ReadOpCode();
                Operand = ReadOperand(OpCode, methodInfo);
                return true;
            }
            return false;
        }

        OpCode ReadOpCode()
        {
            byte instruction = ReadByte();
            if (instruction < 254)
                return singleByteOpCode[instruction];
            else
                return doubleByteOpCode[ReadByte()];
        }

        MemberInfo ReadOperand(OpCode code, MethodBase methodInfo)
        {
            MetadataToken = 0;
            switch (code.OperandType)
            {
                case OperandType.InlineMethod:
                    MetadataToken = ReadInt();
                    Type[] methodArgs = null;
                    if (methodInfo.GetType() != typeof(ConstructorInfo) && !methodInfo.GetType().IsSubclassOf(typeof(ConstructorInfo)))
                    {
                        methodArgs = methodInfo.GetGenericArguments();
                    }
                    Type[] typeArgs = null;
                    if (methodInfo.DeclaringType != null)
                    {
                        typeArgs = methodInfo.DeclaringType.GetGenericArguments();
                    }
                    try
                    {
                        return methodInfo.Module.ResolveMember(MetadataToken, typeArgs, methodArgs);
                    }
                    catch
                    {
                        // Can return System.ArgumentException : Token xxx is not a valid MemberInfo token in the scope of module xxx.dll
                        return null;
                    }
            }
            return null;
        }

        byte ReadByte()
        {
            return _cil[ptr++];
        }

        int ReadInt()
        {
            byte b1 = ReadByte();
            byte b2 = ReadByte();
            byte b3 = ReadByte();
            byte b4 = ReadByte();
            return (int)b1 | (((int)b2) << 8) | (((int)b3) << 16) | (((int)b4) << 24);
        }

        static ILReader()
        {
            singleByteOpCode = new OpCode[225];
            doubleByteOpCode = new OpCode[31];

            FieldInfo[] fields = GetOpCodeFields();

            for (int i = 0; i < fields.Length; i++)
            {
                OpCode code = (OpCode)fields[i].GetValue(null);
                if (code.OpCodeType == OpCodeType.Nternal)
                    continue;

                if (code.Size == 1)
                    singleByteOpCode[code.Value] = code;
                else
                    doubleByteOpCode[code.Value & 0xff] = code;
            }
        }

        static FieldInfo[] GetOpCodeFields()
        {
            return typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static);
        }
    }
}
