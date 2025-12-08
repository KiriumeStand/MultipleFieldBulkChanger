namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public record Optional<T>
    {
        public bool HasValue { get; }
        public T Value { get; }

        public Optional(T value, bool hasValue)
        {
            Value = value;
            HasValue = hasValue;
        }

        public Optional(T value)
        {
            Value = value;
            HasValue = true;
        }

        public static Optional<T> None => new(default, false);
    }
}
