using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        Exception exception = null;
        try
        {
            new Program();
        }
        catch (Exception ex)
        {
            exception = ex.Demystify();
        }

        Console.WriteLine(exception);
    }

    static Action<string, bool> s_action = (string s, bool b) => s_func(s, b);
    static Func<string, bool, (string val, bool)> s_func = (string s, bool b) => (RefMethod(s), b);

    Action<Action<object>, object> _action = (Action<object> lambda, object state) => lambda(state);

    static string s = "";

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    Program() : this(() => Start())
    {

    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    Program(Action action)
    {
        RunAction((state) => _action((s) => action(), state), null);
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static IEnumerable<string> Iterator(int startAt)
    {
        var list = new List<int>() { 1, 2, 3, 4 };
        foreach (var item in list)
        {
            // Throws the exception
            list.Add(item);

            yield return item.ToString();
        }
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static async Task<string> MethodAsync(int value)
    {
        await Task.Delay(0);
        return GenericClass<byte>.GenericMethod(ref value);
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static async Task<string> MethodAsync<TValue>(TValue value)
    {
        return await MethodAsync(1);
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static void RunAction(Action<object> lambda, object state)
    {
        lambda(state);
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static string RunLambda(Func<string> lambda)
    {

        return lambda();
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static (string val, bool) Method(string value)
    {
        Func<string> func = () => MethodAsync(value).GetAwaiter().GetResult();
        var anonType = new { func };
        return (RunLambda(() => anonType.func()), true);
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static ref string RefMethod(int value)
    {
        return ref s;
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static string RefMethod(in string value)
    {
        var val = value;
        return LocalFuncParam(value).ToString();

        int LocalFuncParam(string s)
        {
            return int.Parse(LocalFuncRefReturn());
        }

        ref string LocalFuncRefReturn()
        {
            Method(val);
            return ref s;
        }
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static string Start()
    {
        return LocalFunc2(true, false).ToString();

        void LocalFunc1(long l)
        {
            Start((val: "", true));
        }

        bool LocalFunc2(bool b1, bool b2)
        {
            LocalFunc1(1);
            return true;
        }
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static ref string RefMethod(bool value)
    {
        return ref s;
    }

    [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
    static void Start((string val, bool) param)
    {
        s_action.Invoke(param.val, param.Item2);
    }


    class GenericClass<TSuperType>
    {
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static string GenericMethod<TSubType>(ref TSubType value)
        {
            var returnVal = "";
            for (var i = 0; i < 10; i++)
            {
                try
                {
                    returnVal += string.Join(", ", Iterator(5).Select(s => s));
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex);
                }
            }

            return returnVal;
        }
    }
}
