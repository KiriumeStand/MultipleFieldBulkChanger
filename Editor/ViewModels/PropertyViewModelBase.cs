using System;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    internal abstract class ViewModelPropertyBase<TModel> : IViewModel
        where TModel : class
    {
        [SerializeField]
        internal string vm_DebugLabelText;

        protected SerializedProperty TargetProperty { get; private set; }

        internal TModel Model => (TModel)MFBCHelper.GetTargetObject(TargetProperty);

        internal void InitializeSetting(SerializedProperty targetProperty)
        {
            TargetProperty = targetProperty;
        }

        public abstract void Recalculate();
    }
}