using System;
using System.Collections.Generic;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public record Optional<T> : IEquatable<Optional<T>>
    {
        public bool HasValue { get; }
        private T _value;
        public T Value { get => HasValue ? _value : default; }

        public Optional(T value, bool hasValue)
        {
            _value = value;
            HasValue = hasValue;
        }

        public Optional(T value)
        {
            _value = value;
            HasValue = true;
        }

        public static Optional<T> None => new(default, false);

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
