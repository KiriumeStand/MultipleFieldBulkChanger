using System;
using System.Collections.Generic;
using System.Linq;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public abstract class FieldSelectorContainerDrawerBase : ExpansionPropertyDrawer
    {
        public FieldSelectorContainerDrawerBase() : base() { }

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
                    if (!EditorUtil.SerializedObjectUtil.IsValid(property)) return false;
                    if (status.CurrentPhase < InspectorCustomizerStatus.Phase.BeforeDelayCall) return false;

                    property.serializedObject.Update();

                    SerializedObject senderSerializedObject = e.GetSerializedObject();

                    bool isSameEditorInstance = EditorUtil.ObjectIdUtil.GetObjectId(senderSerializedObject) == EditorUtil.ObjectIdUtil.GetObjectId(property.serializedObject);

                    //string senderBindingPropertyInstancePath = $"{EditorUtil.SerializedObjectUtil.GetSerializedObjectInstanceId(senderSerializedObject)}.{e.SenderBindingPath}";
                    string senderBindingPropertyInstancePath = EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(e.SenderBindingSerializedProperty);

                    // イベント発行が先祖からかを確認
                    bool isSenderIsAncestorProperty = false;
                    foreach (int index in e.RemovedIndex)
                    {
                        string targetPathPrefix = $"{senderBindingPropertyInstancePath}.Array.data[{index}]";
                        isSenderIsAncestorProperty |= EditorUtil.SerializedObjectUtil.GetPropertyInstancePath(property).StartsWith(targetPathPrefix);
                    }

                    return isSameEditorInstance && isSenderIsAncestorProperty;
                },
                true
            );
        }

        // ▲ 初期化定義 ========================= ▲

        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        /// <summary>
        /// 選択オブジェクトが変更された時のイベント処理
        /// </summary>
        /// <param name="args"></param>
        /// <param name="property"></param>
        /// <param name="propertyInstancePath"></param>
        protected void OnFieldSelectorSelectObjectChangedEventHandler(FieldValueChangedEventArgs<UnityEngine.Object> args, VisualElement uxml, InspectorCustomizerStatus status)
        {
            // ノードツリーのキャッシュを更新
            UpdateSerializedPropertiesCache(args.SenderInspectorCustomizerSerializedProperty, uxml, status, args.NewValue);
        }

        protected void OnListViewAncestorItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, status);
        }

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        /// <summary>
        /// 選択したオブジェクトの <see cref="SerializedProperty"/> リストを更新
        /// </summary>
        /// <param name="property"></param>
        /// <param name="uxml"></param>
        /// <param name="status"></param>
        /// <param name="selectedObject"></param>
        protected void UpdateSerializedPropertiesCache(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, UnityEngine.Object selectedObject)
        {
            // 偽装NullかNullならNullに統一
            selectedObject = RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectedObject) ? null : selectedObject;

            SerializedObject selectedSerializedObject = null;
            SerializedPropertyTreeNode propertyRoot = new(null, null, false, false);
            if (!RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(selectedObject))
            {
                selectedSerializedObject = new(selectedObject);
                HashSet<EditorUtil.SerializedObjectUtil.Filter> addListFilters = new() {
                        new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsHighRisk, true),
                        new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsSafetyUnknown, true),
                };
                HashSet<EditorUtil.SerializedObjectUtil.Filter> enterChildrenFilters = new() {
                        new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsHighRisk, true),
                        new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsSafetyUnknown, true),
                };
                HashSet<EditorUtil.SerializedObjectUtil.Filter> selectableNodeFilters = new() {
                        new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsGenericType, true),
                };
                HashSet<EditorUtil.SerializedObjectUtil.Filter> editableFilters = new() {
                        new(EditorUtil.SerializedObjectUtil.FilterFuncs.IsReadonly, true),
                };
                propertyRoot = EditorUtil.SerializedObjectUtil.GetPropertyTree(selectedSerializedObject, addListFilters, enterChildrenFilters, selectableNodeFilters, editableFilters);
            }

            FieldSelectorContainerBase targetObject = EditorUtil.SerializedObjectUtil.GetTargetObject(property) as FieldSelectorContainerBase;

            UniversalDataManager.targetObjectAllPropertieNodesCache[targetObject] = propertyRoot.GetAllNode().ToHashSet();
            UniversalDataManager.targetObjectRootSerializedObjectCache[targetObject] = selectedSerializedObject;

            UniversalDataManager.targetObjectPropertiyTreeRootCache[targetObject] = propertyRoot;

            OnSelectObjectSerializedPropertiesUpdateEventPublish(property, uxml, status, selectedSerializedObject);
        }

        private void OnSelectObjectSerializedPropertiesUpdateEventPublish(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, SerializedObject serializedObject)
        {
            property.serializedObject.Update();
            SelectObjectSerializedPropertiesUpdateEventArgs args = new(this, property, uxml, status, serializedObject);
            ((IExpansionInspectorCustomizer)this).Publish(args);
        }

        // ▲ メソッド ========================= ▲

    }
}