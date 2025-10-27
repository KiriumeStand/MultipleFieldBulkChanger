namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    public abstract class ValueHolderBase
    {
        public abstract string ValueTypeFieldName { get; }
        public abstract string FieldTypeFullNameFieldName { get; }
        public abstract string BoolValueFieldName { get; }
        public abstract string NumberValueFieldName { get; }
        public abstract string StringValueFieldName { get; }
        public abstract string ObjectValueFieldName { get; }
    }
}
