using System;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    public abstract class PropertyViewModelBase<TModel> : IViewModel
        where TModel : class
    {
        public PropertyViewModelBase() { }

        protected SerializedProperty TargetProperty { get; private set; }

        public TModel Model => (TModel)MFBCHelper.GetTargetObject(TargetProperty);

        public string vm_DebugLabelText;

        public void InitializeSetting(SerializedProperty targetProperty)
        {
            TargetProperty = targetProperty;
        }

        public abstract void Recalculate();
    }
}