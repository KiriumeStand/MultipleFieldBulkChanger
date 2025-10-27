using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    /// <summary>
    /// <see cref="ListView"/> の要素が削除されたことを通知するイベント
    /// </summary>
    public class ListViewItemsRemovedEventArgs : BindableElementEventArgs<ListView>
    {
        public IEnumerable<int> RemovedIndex { get; }

        public ListViewItemsRemovedEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedObject editorSerializedObject, ListView senderListView, InspectorCustomizerStatus status, IEnumerable<int> removedIndex
            ) : base(inspectorCustomizer, editorSerializedObject, senderListView, status)
        {
            RemovedIndex = removedIndex;
        }

        public ListViewItemsRemovedEventArgs(
            IExpansionInspectorCustomizer inspectorCustomizer, SerializedProperty drawerProperty, ListView senderListView, InspectorCustomizerStatus status, IEnumerable<int> removedIndex
            ) : base(inspectorCustomizer, drawerProperty, senderListView, status)
        {
            RemovedIndex = removedIndex;
        }
    }
}