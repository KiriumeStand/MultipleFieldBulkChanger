using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class FieldChangeSettingDrawer : ExpansionPropertyDrawer { }

    [CustomPropertyDrawer(typeof(FieldChangeSetting))]
    public class FieldChangeSettingDrawerImpl : ExpansionPropertyDrawerImpl<FieldChangeSettingDrawer>
    {
        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject)
        {
            MultipleFieldBulkChangerVM viewModel = MultipleFieldBulkChangerVM.GetInstance(property.serializedObject);
            SerializedProperty vmSP = ViewModelManager.GetViewModelSerializedProperty<MultipleFieldBulkChangerVM, MultipleFieldBulkChanger>(property);


            Toggle u_Enable = BindHelper.BindRelative<Toggle>(uxml, UxmlNames.Enable, property, nameof(FieldChangeSetting._Enable));
            TextField u_Expression = BindHelper.BindRelative<TextField>(uxml, UxmlNames.Expression, property, nameof(FieldChangeSetting._Expression));
            ListView u_TargetFields = BindHelper.BindRelative<ListView>(uxml, UxmlNames.TargetFields, property, nameof(FieldChangeSetting._TargetFields));

            Label u_ValuePreview = BindHelper.BindRelative<Label>(uxml, UxmlNames.ValuePreview, vmSP, nameof(FieldChangeSettingVM.vm_ValuePreview));

            // イベント発行の登録
            u_TargetFields.itemsAdded += (e) =>
            {
                IExpansionInspectorCustomizer.AddListElementWithClone(((FieldChangeSetting)targetObject)._TargetFields, e);

                viewModel.Recalculate();
            };
            u_TargetFields.itemsRemoved += (e) =>
            {
                viewModel.Recalculate();
            };
        }

        // ▲ 初期化定義 ========================= ▲


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
