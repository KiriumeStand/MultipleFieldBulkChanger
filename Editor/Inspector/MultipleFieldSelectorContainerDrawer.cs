using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class MultipleFieldSelectorContainerDrawer : ExpansionPropertyDrawer { }

    [CustomPropertyDrawer(typeof(MultipleFieldSelectorContainer))]
    public class MultipleFieldSelectorContainerDrawerImpl : FieldSelectorContainerDrawerImplBase<MultipleFieldSelectorContainerDrawer>
    {
        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            MultipleFieldBulkChangerVM viewModel = MultipleFieldBulkChangerVM.GetInstance(property.serializedObject);

            ObjectField u_SelectObject = BindHelper.BindRelative<ObjectField>(uxml, UxmlNames.SelectObject, property, nameof(MultipleFieldSelectorContainer._SelectObject));
            ListView u_SelectFields = BindHelper.BindRelative<ListView>(uxml, UxmlNames.FieldsSelector, property, nameof(MultipleFieldSelectorContainer._FieldSelectors));

            // イベント発行の登録
            u_SelectFields.itemsAdded += (e) =>
            {
                IExpansionInspectorCustomizer.AddListElementWithClone(((MultipleFieldSelectorContainer)targetObject)._FieldSelectors, e);

                viewModel.Recalculate();
            };
            u_SelectFields.itemsRemoved += (e) =>
            {
                ListViewItemsRemovedEventArgs args = new(this, property, u_SelectFields, status, e);
                ((IExpansionInspectorCustomizer)this).Publish(args);

                viewModel.Recalculate();
            };

            // イベント購読の登録
            SubscribeListViewItemsRemovedEvent(property, uxml, status);
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public record UxmlNames
        {
            public static readonly string SelectObject = "MFSC_SelectObject";
            public static readonly string FieldsSelector = "MFSC_FieldsSelector";
        }

        // ▲ 名前辞書 ========================= ▲
    }
}