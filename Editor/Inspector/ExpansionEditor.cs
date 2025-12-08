using System.Runtime.CompilerServices;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// イベント購読の自動解除機能などを持つEditorの基底クラス
    /// </summary>
    public abstract class ExpansionEditor : Editor, IExpansionInspectorCustomizer
    {
        public string SourceFilePath { get; }

        private VisualElement _uxml;

        private InspectorCustomizerStatus _status;

        /// <summary>
        /// 継承先は必ずこのコンストラクターを明示的に呼び出さなくてはならない
        /// </summary>
        /// <param name="sourceFilePath"></param>
        public ExpansionEditor([CallerFilePath] string sourceFilePath = "")
        {
            SourceFilePath = new System.IO.FileInfo(sourceFilePath).FullName;
        }

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        /// <summary>
        /// UnityのCreateInspectorGUIをオーバーライドし、自動クリーンアップを追加
        /// </summary>
        /// <returns></returns>
        public sealed override VisualElement CreateInspectorGUI()
        {
            VisualElement uxml = ((IExpansionInspectorCustomizer)this).CreateCustomizerGUI(serializedObject);
            return uxml;
        }

        public abstract void CreateInspectorGUICore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status);

        public void PostCreateInspectorGUICore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        {
            _uxml = uxml;
            _status = status;
        }

        public virtual void DelayCallCore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        public void OnDisable()
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = EditorUtil.SerializedObjectUtil.GetTargetObject(serializedObject);
            VisualElement uxml = _uxml;
            InspectorCustomizerStatus status = _status;

            ((IExpansionInspectorCustomizer)this).DrawerCleanup(serializedObject, uxml, targetObject, status);

            if (uxml == null || status == null) return;

            OnDisableEventArgs args = new(this, serializedObject, uxml, status);

            UniversalEventManager.Publish(args);
        }

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        public virtual void OnCleanup(
            SerializedObject serializedObject,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        // ▲ メソッド ========================= ▲

    }
}