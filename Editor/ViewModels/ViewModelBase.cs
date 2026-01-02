using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    public abstract class ViewModelBase<TViewModel, TTarget> : ScriptableObject, IViewModel where TViewModel : ViewModelBase<TViewModel, TTarget> where TTarget : Component
    {
        private static readonly Dictionary<TTarget, TViewModel> _instances = new();

        public string vm_DebugLabelText;

        public TTarget Model { get; private set; }

        private SerializedObject _modelSO;
        public SerializedObject ModelSO
        {
            get
            {
                _modelSO ??= new(Model);
                return _modelSO;
            }
        }

        protected ViewModelBase() { }

        public static TViewModel GetInstance(SerializedObject modelSO)
        {
            TTarget castedTargetObject = modelSO.targetObject as TTarget;
            if (_instances.TryGetValue(castedTargetObject, out TViewModel vm))
            {
                return vm;
            }
            else
            {
                vm = CreateInstance<TViewModel>();
                vm.Model = castedTargetObject;
                vm.Recalculate();

                _instances.Add(castedTargetObject, vm);
                return vm;
            }
        }

        public abstract void Recalculate();
    }
}