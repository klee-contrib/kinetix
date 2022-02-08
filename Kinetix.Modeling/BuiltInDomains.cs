using Kinetix.Modeling.Annotations;

namespace Kinetix.Modeling;

public enum BuiltInDomains
{
    [DomainType(typeof(bool))]
    Boolean,

    [DomainType(typeof(byte))]
    Byte,

    [DomainType(typeof(DateTime))]
    DateTime,

    [DomainType(typeof(decimal))]
    Decimal,

    [DomainType(typeof(double))]
    Double,

    [DomainType(typeof(float))]
    Float,

    [DomainType(typeof(int))]
    Int,

    [DomainType(typeof(long))]
    Long,

    [DomainType(typeof(sbyte))]
    Sbyte,

    [DomainType(typeof(short))]
    Short,

    [DomainType(typeof(string))]
    String,

    [DomainType(typeof(uint))]
    Uint,

    [DomainType(typeof(ushort))]
    Ushort,

    [DomainType(typeof(ulong))]
    Ulong,

    [DomainType(typeof(byte[]))]
    ByteArray,

    [DomainType(typeof(Guid))]
    Guid,

    [DomainType(typeof(char))]
    Char,

    [DomainType(typeof(TimeSpan))]
    TimeSpan,

    [DomainType(typeof(string[]))]
    StringArray,

    [DomainType(typeof(int[]))]
    IntArray
}
