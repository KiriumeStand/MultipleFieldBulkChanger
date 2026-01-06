using System;
using System.Collections.Generic;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal record Optional<T> : IEquatable<Optional<T>>
    {
        internal bool HasValue { get; }
        private readonly T _value;
        internal T Value { get => HasValue ? _value : default; }

        internal Optional(T value, bool hasValue)
        {
            _value = value;
            HasValue = hasValue;
        }

        internal Optional(T value)
        {
            _value = value;
            HasValue = true;
        }

        internal static Optional<T> None => new(default, false);

        public virtual bool Equals(Optional<T> other)
        {
            if (HasValue == false && other.HasValue == false) return true;
            if (HasValue != other.HasValue) return false;
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((Value, HasValue));
        }
    }
}
