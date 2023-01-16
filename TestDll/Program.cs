using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TestDll
{
    class Emit_Test
    {
        public static int AA = 10000;

        public int BB { get; set; }

        [IndexerName("It")]
        public int this[int i]
        {
            get => i;
            set => i = value;
        }
    }

    class Emit_Test<T>
    {
        public int AA = 10000;

        public int BB { get; set; }

        [IndexerName("It")]
        public int this[int i]
        {
            get => i;
            set => i = value;
        }
    }

    public class Program
    {
        public static void Main()
        {
            Info.OfConstructor<object>();

            Info.OfField<Emit_Test<string>>("AA");

            Info.OfPropertyGet<Emit_Test<string>>("BB");

            Info.OfPropertySet<Emit_Test<string>>("BB");

            Info.OfIndexerGet<Emit_Test<string>>("Int32", "It");

            Info.OfIndexerSet<Emit_Test<string>>("Int32", "It");

            var m = Info.OfMethod<Action<string>>(nameof(Action<string>.Invoke));

            var m2 = Info.OfMethod<string>(nameof(string.Clone));

            Info.OfMethod<Type>(nameof(Type.GetTypeFromHandle));

            var c = Info.OfConstructor<Action<string>>("Object, IntPtr");

            var mt = Info.OfMethod<int>("TryParse", $"{nameof(String)}, {nameof(Int32)}&");

            var mmm = Info.OfMethod<Program>("Main");

            var m_GetTypeFromHandle = Info.OfMethod<Type>(nameof(Type.GetTypeFromHandle));

            var m_GetMethodFromHandle = Info.OfMethod<MethodBase>(
                nameof(MethodBase.GetMethodFromHandle),
                $"{nameof(RuntimeMethodHandle)}, {nameof(RuntimeTypeHandle)}");

            var fi__ = Info.OfField<string>(nameof(string.Empty));

            var fi__1 = Info.OfField<Emit_Test>("AA");

            Info.OfPropertyGet<Emit_Test>("BB");

            Info.OfPropertySet<Emit_Test>("BB");

            Info.OfIndexerGet<Emit_Test>("Int32", "It");

            Info.OfIndexerSet<Emit_Test>("Int32", "It");

            var writeline = Info.OfMethod("mscorlib", "System.Console", "WriteLine", "String");

            Console.ReadKey();
        }
    }
}
