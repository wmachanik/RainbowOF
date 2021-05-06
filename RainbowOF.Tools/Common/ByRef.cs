using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainbowOF.Tools.Common
{
    public class ByRef<T>
    {
            public ByRef() { }
            public ByRef(T value) { Value = value; }
            public T Value { get; set; }
            public override string ToString()
            {
                T value = Value;
                return value == null ? "" : value.ToString();
            }
            public static implicit operator T(ByRef<T> r) { return r.Value; }
            public static implicit operator ByRef<T>(T value) { return new ByRef<T>(value); }
    }
}
