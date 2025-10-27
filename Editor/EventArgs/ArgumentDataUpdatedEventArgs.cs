using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// 引数データの更新がされたことを通知するイベント
    /// </summary>
    public class ArgumentDataUpdatedEventArgs : VisualElementEventArgs<VisualElement>
    {
        public ArgumentData NewArgumentData { get; }

        public ArgumentDataUpdatedEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, VisualElement senderElement, InspectorCustomizerStatus status, ArgumentData newArgumentData
            ) : base(inspectorCustomizer, drawerProperty, senderElement, status)
        {
            NewArgumentData = newArgumentData;
        }
    }


}