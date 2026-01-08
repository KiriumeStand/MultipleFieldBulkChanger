using System;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    internal abstract class FieldSelectorContainerViewModelBase<TModel> : ViewModelPropertyBase<TModel>
        where TModel : class
    {
        public Trackable<UnityEngine.Object> m_SelectObject { get; protected set; }
    }
}