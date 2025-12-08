using System.Runtime.CompilerServices;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// イベント購読の自動解除機能などを持つPropertyDrawerの基底クラス
    /// </summary>
    public abstract class ExpansionPropertyDrawer : PropertyDrawer, IExpansionInspectorCustomizer
    {
        public string SourceFilePath { get; }

        /// <summary>
        /// 継承先は必ずこのコンストラクターを明示的に呼び出さなくてはならない
        /// </summary>
        /// <param name="sourceFilePath"></param>
        public ExpansionPropertyDrawer([CallerFilePath] string sourceFilePath = "")
        {
            SourceFilePath = new System.IO.FileInfo(sourceFilePath).FullName;
        }

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        /// <summary>
        /// UnityのCreatePropertyGUIをオーバーライドし、自動クリーンアップを追加
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement uxml = ((IExpansionInspectorCustomizer)this).CreateCustomizerGUI(property);
            return uxml;
        }

        public abstract void CreatePropertyGUICore(
            SerializedProperty property,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status);

        public void PostCreatePropertyGUICore(
            SerializedProperty property,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        {
            ((IExpansionInspectorCustomizer)this).Subscribe<OnDisableEventArgs>(this,
                property, status,
                (sender, args) => { OnDisableEventHandler(property, uxml, targetObject, status); },
                e =>
                {
                    if (!EditorUtil.SerializedObjectUtil.IsValid(property)) return false;
                    return e.GetSerializedObject() == property.serializedObject;
                },
                true
            );
        }

        public virtual void DelayCallCore(
            SerializedProperty property,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        private void OnDisableEventHandler(
            SerializedProperty property,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        {
            ((IExpansionInspectorCustomizer)this).DrawerCleanup(property, uxml, targetObject, status);
        }

        // ▲ イベントハンドラー ========================= ▲

        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        public virtual void OnCleanup(
            SerializedProperty property,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        // ▲ メソッド ========================= ▲
    }
}