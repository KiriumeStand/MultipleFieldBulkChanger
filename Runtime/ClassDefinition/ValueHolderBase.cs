namespace io.github.kiriumestand.multiplefieldbulkchanger.runtime
{
    public abstract class ValueHolderBase<T> where T : ValueHolderBase<T>
    {
        public abstract string ValueTypeFieldName { get; }
        public abstract string BoolValueFieldName { get; }
        public abstract string NumberValueFieldName { get; }
        public abstract string StringValueFieldName { get; }
        public abstract string ColorValueFieldName { get; }
        public abstract string ObjectValueFieldName { get; }
        public abstract string Vector2ValueFieldName { get; }
        public abstract string Vector3ValueFieldName { get; }
        public abstract string Vector4ValueFieldName { get; }
        public abstract string BoundsValueFieldName { get; }
        public abstract string CurveValueFieldName { get; }
        public abstract string GradientValueFieldName { get; }

        public abstract string GetCurrentValueFieldName();
    }
}
