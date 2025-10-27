
using System;
using System.Runtime.CompilerServices;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class ObjectEqualityWeakReference<T> : WeakReference
    {
        private readonly int _hashCode;

        public ObjectEqualityWeakReference(T obj) :
            base(obj, true)
        {
            _hashCode = RuntimeHelpers.GetHashCode(obj);
        }

        public static implicit operator ObjectEqualityWeakReference<T>(T value) => new(value);
        public static implicit operator T(ObjectEqualityWeakReference<T> wrapper) => (T)wrapper.Target;

        public override bool Equals(object obj)
        {
            if (obj is not WeakReference other) return ReferenceEquals(obj, Target);

            return ReferenceEquals(other, this) ||
                   ReferenceEquals(other.Target, Target);
        }

        public override int GetHashCode() { return _hashCode; }
    }
}