using System.Collections.Generic;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal record Trackable<T>
    {
        private T _currentValue;
        private T _originalValue;

        internal T Value
        {
            get => _currentValue;
            set => _currentValue = value;
        }

        internal T OriginalValue => _originalValue;

        internal bool IsModified
        {
            get
            {
                if (IsInitialValue) return true;
                if (_currentValue == null && _originalValue == null) return false;
                else if (_currentValue == null || _originalValue == null) return true;
                return !EqualityComparer<T>.Default.Equals(_currentValue, _originalValue);
            }
        }

        internal bool IsInitialValue { get; private set; }

        internal Trackable(T initialValue, bool initIsModified = false)
        {
            _currentValue = initialValue;
            _originalValue = initialValue;
            IsInitialValue = initIsModified;
        }

        internal void AcceptChanges()
        {
            _originalValue = _currentValue;
            IsInitialValue = false;
        }

        internal void RejectChanges()
        {
            _currentValue = _originalValue;
        }
    }

    internal static class TrackableHelper
    {
        internal static Trackable<T> CreateOrUpdate<T>(this Trackable<T> trackable, T setValue)
        {
            if (trackable == null)
            {
                return new(setValue, true);
            }
            else
            {
                trackable.Value = setValue;
                return trackable;
            }
        }
    }
}
