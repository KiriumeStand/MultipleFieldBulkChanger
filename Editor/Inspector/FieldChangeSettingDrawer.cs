using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class FieldChangeSettingDrawer : ExpansionPropertyDrawer { }

    [CustomPropertyDrawer(typeof(FieldChangeSetting))]
    public class FieldChangeSettingDrawerImpl : ExpansionPropertyDrawerImpl<FieldChangeSettingDrawer>
    {

        private static string GetMultiFieldSelectorContainerPath(int index1) => $"{nameof(FieldChangeSetting._TargetFields)}.Array.data[{index1}]";
        private static string GetFieldSelectorPath(int index1, int index2) => $"{GetMultiFieldSelectorContainerPath(index1)}.{nameof(MultipleFieldSelectorContainer._FieldSelectors)}.Array.data[{index2}]";

        public FieldChangeSettingDrawerImpl() : base() { }

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            MultipleFieldBulkChangerVM viewModel = MultipleFieldBulkChangerVM.GetInstance(property.serializedObject);
            SerializedObject vmRootSO = new(viewModel);
            string vmPropPath = ViewModelManager.GetVMPropPath(property);
            SerializedProperty vmProperty = vmRootSO.FindProperty(vmPropPath);


            Toggle u_Enable = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.Enable, property, nameof(FieldChangeSetting._Enable));
            TextField u_Expression = BindHelper.BindRelative<TextField>(uxml, UxmlNames.Expression, property, nameof(FieldChangeSetting._Expression));
            ListView u_TargetFields = BindHelper.BindRelative<ListView>(uxml, UxmlNames.TargetFields, property, nameof(FieldChangeSetting._TargetFields));

            Label u_ValuePreview = BindHelper.BindRelative<Label>(uxml, UxmlNames.ValuePreview, vmProperty, nameof(FieldChangeSettingVM.vm_ValuePreview));

            // イベント発行の登録
            u_TargetFields.itemsAdded += (e) =>
            {
                IExpansionInspectorCustomizer.AddListElementWithClone(((FieldChangeSetting)targetObject)._TargetFields, e);

                viewModel.Recalculate();
            };
            u_TargetFields.itemsRemoved += (e) =>
            {
                ListViewItemsRemovedEventArgs args = new(this, property, u_TargetFields, status, e);
                ((IExpansionInspectorCustomizer)this).Publish(args);

                viewModel.Recalculate();
            };

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

                    string senderBindingPropertyInstancePath = SerializedObjectUtil.GetPropertyInstancePath(e.SenderBindingSerializedProperty);

                    // イベント発行が先祖からかを確認
                    bool isSenderIsAncestorProperty = false;
                    foreach (int index in e.RemovedIndex)
                    {
                        string targetPathPrefix = $"{senderBindingPropertyInstancePath}.Array.data[{index}]";
                        isSenderIsAncestorProperty |= SerializedObjectUtil.GetPropertyInstancePath(property).StartsWith(targetPathPrefix);
                    }

                    return isSameEditorInstance && isSenderIsAncestorProperty;
                },
                true
            );
        }

        public override void DelayCallCore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        private void OnListViewAncestorItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = MFBCHelper.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, status);
        }

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        // ▲ メソッド ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public record UxmlNames
        {
            public static readonly string Enable = "FCS_Enable";
            public static readonly string Expression = "FCS_Expression";
            public static readonly string TargetFields = "FCS_TargetFields";
            public static readonly string ValuePreview = "FCS_ValuePreview";
        }

        // ▲ 名前辞書 ========================= ▲
    }
}
