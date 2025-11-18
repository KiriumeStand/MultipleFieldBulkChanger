using System;

namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    [Serializable]
    public enum FieldNumericType
    {
        Unknown = 0,
        Int8 = 1,
        UInt8 = 2,
        Int16 = 3,
        UInt16 = 4,
        Int32 = 5,
        UInt32 = 6,
        Int64 = 7,
        UInt64 = 8,
        Float = 100,
        Double = 101
    }
}