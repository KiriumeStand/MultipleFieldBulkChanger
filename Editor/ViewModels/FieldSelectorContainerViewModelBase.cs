using System;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    public abstract class FieldSelectorContainerViewModelBase<TModel> : PropertyViewModelBase<TModel>
        where TModel : class
    {
        public Trackable<UnityEngine.Object> m_SelectObject { get; protected set; }
    }
}