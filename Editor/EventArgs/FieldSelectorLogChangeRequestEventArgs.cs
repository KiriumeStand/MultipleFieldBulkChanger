using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// <see cref="FieldSelector"/> のログの更新をリクエストするイベント
    /// </summary>
    public class FieldSelectorLogChangeRequestEventArgs : VisualElementEventArgs<VisualElement>
    {
        public string TargetFieldSelectorPropertyInstancePath { get; }
        public string LogMessage { get; }
        public StyleColor LogColor { get; }
        public FontStyle FontStyle { get; }
        public StyleLength FontSize { get; }

        public FieldSelectorLogChangeRequestEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, VisualElement senderElement, InspectorCustomizerStatus status,
            string targetFieldSelectorPropertyInstancePath, string logMessage, StyleColor? logColor = null, FontStyle? fontStyle = null, StyleLength? fontSize = null
            ) : base(inspectorCustomizer, drawerProperty, senderElement, status)
        {
            TargetFieldSelectorPropertyInstancePath = targetFieldSelectorPropertyInstancePath;
            LogMessage = logMessage;
            LogColor = logColor ?? new StyleColor();
            FontStyle = fontStyle ?? FontStyle.Normal;
            FontSize = fontSize ?? 12;
        }
    }
}