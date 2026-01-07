using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public abstract class ExpansionPropertyDrawer : ScriptableObject
    {
        public StyleSheet uss;
        public VisualTreeAsset uxml;

        protected ExpansionPropertyDrawer() { }
    }

    /// <summary>
    /// イベント購読の自動解除機能などを持つPropertyDrawerの基底クラス
    /// </summary>
    public abstract class ExpansionPropertyDrawerImpl<TDrawer> : PropertyDrawer, IExpansionInspectorCustomizer where TDrawer : ExpansionPropertyDrawer
    {
        private TDrawer _drawer;

        public TDrawer Drawer
        {
            get
            {
                if (_drawer == null)
                {
                    _drawer = ScriptableObject.CreateInstance<TDrawer>();
                }
                return _drawer;
            }
        }

        public StyleSheet USS => Drawer.uss;
        public VisualTreeAsset UXML => Drawer.uxml;

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement uxml = ((IExpansionInspectorCustomizer)this).CreateCustomizerGUI(property);
            return uxml;
        }

        public abstract void CreatePropertyGUICore(
            SerializedProperty property,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject);

        public virtual void DelayCallCore(
            SerializedProperty property,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject)
        { }

        // ▲ 初期化定義 ========================= ▲
    }
}