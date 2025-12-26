using System;
using System.Collections.Generic;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class ArgumentData : IEquatable<ArgumentData>
    {
        public string Name { get; set; } = "";

        public Optional<object> Value { get; set; } = Optional<object>.None;

        public Type Type { get; set; } = null;

        public SerializedPropertyNumericType SPNumericType { get; set; } = SerializedPropertyNumericType.Unknown;

        public FieldSPType FieldSPType => FieldSPTypeHelper.Parse2FieldSPType(Type);

        public virtual bool Equals(ArgumentData other)
        {
            return Name == other.Name &&
                Type == other.Type &&
                EqualityComparer<Optional<object>>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((Name, Value, Type));
        }
    }
}