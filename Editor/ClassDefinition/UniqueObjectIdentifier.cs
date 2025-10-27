using System;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    using UniqueObjectIdentifierTuple = ValueTuple<Type, string>;

    public record UniqueObjectIdentifier
    {
        public readonly Type ObjectType;
        public readonly string DataTypeIdentifier;

        public UniqueObjectIdentifier(UniqueObjectIdentifierTuple tuple) : this(tuple.Item1, tuple.Item2) { }
        public UniqueObjectIdentifier(Type objectType, string dataTypeIdentifier)
        {
            ObjectType = objectType;
            DataTypeIdentifier = dataTypeIdentifier;
        }

        public static implicit operator UniqueObjectIdentifier(UniqueObjectIdentifierTuple value) => new(value);
        public static implicit operator UniqueObjectIdentifierTuple(UniqueObjectIdentifier record) => (record.ObjectType, record.DataTypeIdentifier);
    }
}