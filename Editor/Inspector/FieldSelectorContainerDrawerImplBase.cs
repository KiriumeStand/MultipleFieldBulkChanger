using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public abstract class FieldSelectorContainerDrawerImplBase<TDrawer> : ExpansionPropertyDrawerImpl<TDrawer> where TDrawer : ExpansionPropertyDrawer
    {
        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        protected void SubscribeListViewItemsRemovedEvent(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            // イベント購読の登録
            ((IExpansionInspectorCustomizer)this).Subscribe<ListViewItemsRemovedEventArgs>(this,
                property, status,
                (sender, args) => { OnListViewAncestorItemRemovedEventHandler(args, property, uxml, status); },
                e =>
                {
                    if (!SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    property.serializedObject.Update();

                    SerializedObject senderSerializedObject = e.GetSerializedObject();

                    bool isSameEditorInstance = EditorUtil.ObjectIdUtil.GetObjectId(senderSerializedObject) == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    string senderBindingSPInstancePath = SerializedObjectUtil.GetSerializedPropertyInstancePath(e.SenderBindingSerializedProperty);

                    // イベント発行が先祖からかを確認
                    bool isSenderIsAncestorProperty = false;
                    foreach (int index in e.RemovedIndex)
                    {
                        string targetPathPrefix = $"{senderBindingSPInstancePath}.Array.data[{index}]";
                        isSenderIsAncestorProperty |= SerializedObjectUtil.GetSerializedPropertyInstancePath(property).StartsWith(targetPathPrefix);
                    }

                    return isSameEditorInstance && isSenderIsAncestorProperty;
                },
                true
            );
        }

        // ▲ 初期化定義 ========================= ▲

        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        protected void OnListViewAncestorItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = MFBCHelper.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, status);
        }

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        // ▲ メソッド ========================= ▲

    }
}