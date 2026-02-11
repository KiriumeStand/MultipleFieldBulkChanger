using System;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    internal abstract class ViewModelRootBase<TViewModel, TTarget> : ScriptableObject, IViewModel where TViewModel : ViewModelRootBase<TViewModel, TTarget> where TTarget : Component
    {
        protected ViewModelRootBase() { }

        private static readonly ConditionalWeakTable<TTarget, TViewModel> _instances = new();

        [SerializeField]
        internal string vm_DebugLabelText;

        internal TTarget Model { get; private set; }

        private SerializedObject _modelSO;
        internal SerializedObject ModelSO
        {
            get
            {
                _modelSO ??= new(Model);
                return _modelSO;
            }
        }

        internal static TViewModel GetInstance(SerializedObject modelSO)
        {
            TTarget castedTargetObject = modelSO.targetObject as TTarget;
            if (_instances.TryGetValue(castedTargetObject, out TViewModel vm) && !EditorUtil.FakeNullUtil.IsNullOrFakeNull(vm))
            {
                return vm;
            }
            else
            {
                vm = CreateInstance<TViewModel>();
                vm.Model = castedTargetObject;
                vm.Recalculate();

                _instances.AddOrUpdate(castedTargetObject, vm);
                return vm;
            }
        }

        public abstract void Recalculate();
    }
}