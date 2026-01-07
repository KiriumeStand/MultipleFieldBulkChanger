using System;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    [Serializable]
    /// <summary>
    /// イベント購読の自動解除機能などを持つEditorの基底クラス
    /// </summary>
    public abstract class ExpansionEditor : Editor, IExpansionInspectorCustomizer
    {
        [SerializeField]
        private StyleSheet uss;

        [SerializeField]
        private VisualTreeAsset uxml;

        public VisualTreeAsset UXML => uxml;
        public StyleSheet USS => uss;

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public sealed override VisualElement CreateInspectorGUI()
        {
            VisualElement uxml = ((IExpansionInspectorCustomizer)this).CreateCustomizerGUI(serializedObject);
            return uxml;
        }

        public abstract void CreateInspectorGUICore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject);

        public virtual void DelayCallCore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject)
        { }

        // ▲ 初期化定義 ========================= ▲
    }
}