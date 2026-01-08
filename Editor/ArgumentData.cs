using System;
using System.Collections.Generic;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal class ArgumentData : IEquatable<ArgumentData>
    {
        internal string Name { get; set; } = "";

        internal Optional<object> Value { get; set; } = Optional<object>.None;

        internal Type Type { get; set; } = null;

        internal FieldSPType FieldSPType => FieldSPTypeHelper.Parse2FieldSPType(Type);

        public virtual bool Equals(ArgumentData other)
        {
            return Name == other.Name && Type == other.Type &&
                EqualityComparer<Optional<object>>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((Name, Value, Type));
        }
    }
}