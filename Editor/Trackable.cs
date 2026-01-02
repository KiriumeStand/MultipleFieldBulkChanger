using System.Collections.Generic;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public record Trackable<T>
    {
        private T _currentValue;
        private T _originalValue;

        public T Value
        {
            get => _currentValue;
            set => _currentValue = value;
        }

        public T OriginalValue => _originalValue;

        public bool IsModified
        {
            get
            {
                if (IsInitialValue) return true;
                if (_currentValue == null && _originalValue == null) return false;
                else if (_currentValue == null || _originalValue == null) return true;
                return !EqualityComparer<T>.Default.Equals(_currentValue, _originalValue);
            }
        }

        public bool IsInitialValue { get; private set; }

        public Trackable(T initialValue, bool initIsModified = false)
        {
            _currentValue = initialValue;
            _originalValue = initialValue;
            IsInitialValue = initIsModified;
        }

        public void AcceptChanges()
        {
            _originalValue = _currentValue;
            IsInitialValue = false;
        }

        public void RejectChanges()
        {
            _currentValue = _originalValue;
        }
    }

    public static class TrackableHelper
    {
        public static Trackable<T> CreateOrUpdate<T>(this Trackable<T> trackable, T setValue)
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
