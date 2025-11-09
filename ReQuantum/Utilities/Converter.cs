using System;
using System.Buffers.Binary;

namespace ReQuantum.Utilities;

public static class Converter
{
    public static Guid LongToGuid(long value)
    {
        Span<byte> buffer = stackalloc byte[16];
        BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
        return new Guid(buffer.ToArray());
    }
}
