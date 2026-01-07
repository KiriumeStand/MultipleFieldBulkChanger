using System;
using System.Collections.Generic;
using System.Linq;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class FieldSelectorDrawer : ExpansionPropertyDrawer { }

    [CustomPropertyDrawer(typeof(FieldSelector))]
    public class FieldSelectorDrawerImpl : ExpansionPropertyDrawerImpl<FieldSelectorDrawer>
    {
        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreatePropertyGUICore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            MultipleFieldBulkChangerVM viewModel = MultipleFieldBulkChangerVM.GetInstance(property.serializedObject);
            SerializedObject vmRootSO = new(viewModel);
            string vmSPPath = ViewModelManager.GetVMSPPath(property);
            SerializedProperty vmSP = vmRootSO.FindProperty(vmSPPath);

            TextField u_SelectFieldPath = BindHelper.BindRelative<TextField>(uxml, UxmlNames.SelectFieldPath, property, nameof(FieldSelector._SelectFieldPath));
            Label u_LogLabel = BindHelper.BindRelative<Label>(uxml, UxmlNames.LogLabel, vmSP, nameof(FieldSelectorVM.vm_LogLabel));

            // イベント発行の登録
            u_LogLabel.RegisterValueChangedCallback(e => OnTextElementValueChangedEventHandler(e, property, uxml, status, vmSP));

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

        public override void DelayCallCore(SerializedProperty property, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            MultipleFieldBulkChangerVM viewModel = MultipleFieldBulkChangerVM.GetInstance(property.serializedObject);
            SerializedObject vmRootSO = new(viewModel);
            string vmSPPath = ViewModelManager.GetVMSPPath(property);
            SerializedProperty vmSP = vmRootSO.FindProperty(vmSPPath);

            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);
            Button u_SelectFieldButton = UIQuery.Q<Button>(uxml, UxmlNames.SelectFieldButton);

            Label u_LogLabel = UIQuery.Q<Label>(uxml, UxmlNames.LogLabel);

            UpdateLogLabel(uxml, vmSP);

            // イベント購読の登録
            u_SelectFieldButton.clicked += () =>
            {
                OnSelectFieldButtonClickedEventHandler(property, uxml, status, vmSP);
            };
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        private void OnListViewAncestorItemRemovedEventHandler(ListViewItemsRemovedEventArgs args, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status)
        {
            IExpansionInspectorCustomizerTargetMarker targetObject = MFBCHelper.GetTargetObject(property);
            ((IExpansionInspectorCustomizer)this).OnDetachFromPanelEvent(property, uxml, targetObject, status);
        }

        private static void OnTextElementValueChangedEventHandler(ChangeEvent<string> e, SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, SerializedProperty vmProperty)
        {
            UpdateLogLabel(uxml, vmProperty);
        }

        private static void OnSelectFieldButtonClickedEventHandler(SerializedProperty property, VisualElement uxml, InspectorCustomizerStatus status, SerializedProperty vmSP)
        {
            ShowFieldSelectorAdvancedDropdown(property, uxml, vmSP);
        }

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        private static void UpdateLogLabel(VisualElement uxml, SerializedProperty vmSP)
        {
            Label u_LogLabel = UIQuery.Q<Label>(uxml, UxmlNames.LogLabel);

            SerializedProperty logStyleSP = vmSP.FindPropertyRelative(nameof(FieldSelectorVM.vm_LogStyle));
            logStyleSP.serializedObject.Update();
            string logStyle = logStyleSP.stringValue;
            switch (logStyle)
            {
                case "Error":
                    u_LogLabel.style.color = new StyleColor(Color.red);
                    u_LogLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    u_LogLabel.style.fontSize = 12;
                    break;
                case "Normal":
                default:
                    u_LogLabel.style.color = new StyleColor(Color.white);
                    u_LogLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
                    u_LogLabel.style.fontSize = 12;
                    break;
            }

            if (!string.IsNullOrWhiteSpace(u_LogLabel.text))
                VisualElementUtil.SetDisplay(u_LogLabel, true);
            else VisualElementUtil.SetDisplay(u_LogLabel, false);
        }

        private static void ShowFieldSelectorAdvancedDropdown(SerializedProperty property, VisualElement uxml, SerializedProperty vmSP)
        {
            TextField u_SelectFieldPath = UIQuery.Q<TextField>(uxml, UxmlNames.SelectFieldPath);
            Button u_SelectFieldButton = UIQuery.Q<Button>(uxml, UxmlNames.SelectFieldButton);

            FieldSelectorVM vm = (FieldSelectorVM)vmSP.managedReferenceValue;
            SerializedPropertyTreeNode rootNode = vm.GetParentSelectObjectSerializedPropertyTree();

            SerializedProperty selectFieldPathSP = property.FindPropertyRelative(nameof(FieldSelector._SelectFieldPath));

            SerializedProperty fieldSelectorContainerSP = SerializedObjectUtil.GetParentSerializedProperty(property);
            IExpansionInspectorCustomizerTargetMarker grandparentObject = MFBCHelper.GetTargetObject(fieldSelectorContainerSP);
            bool ddItemEditableOnly = grandparentObject is FieldChangeSetting;

            FieldSelectorAdvancedDropdown dropdown = new(
                new List<string>() { u_SelectFieldPath.value }, new AdvancedDropdownState(),
                rootNode, selectFieldPathSP, ddItemEditableOnly
            );
            dropdown.Show(u_SelectFieldButton.parent.worldBound);
        }

        // ▲ メソッド ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public record UxmlNames
        {
            public static readonly string SelectFieldPath = "FS_SelectFieldPath";
            public static readonly string SelectFieldButton = "FS_SelectFieldButton";
            public static readonly string LogLabel = "FS_LogLabel";
        }

        // ▲ 名前辞書 ========================= ▲


        // ▼ 拡張AdvancedDropdown ========================= ▼
        // MARK: ==拡張AdvancedDropdown==

        private class FieldSelectorAdvancedDropdown : ExpantionAdvancedDropdown<FieldSelectorAdvancedDropdownItem>
        {
            protected override FieldSelectorAdvancedDropdownItem GetNewSearchTreeRoot() => new("Search Results", false, null);

            private readonly SerializedPropertyTreeNode _rootNode;

            private readonly SerializedProperty _bindingProperty;

            private readonly bool _editableOnly;

            private readonly HashSet<SerializedPropertyTreeNode.Filter> _selectableFilters = new();
            private readonly HashSet<SerializedPropertyTreeNode.Filter> _innerNodeFilters = new();

            public FieldSelectorAdvancedDropdown(
                List<string> selectedItemPaths, AdvancedDropdownState state, SerializedPropertyTreeNode root,
                 SerializedProperty bindingProperty, bool editableOnly
            ) : base(selectedItemPaths.Select(x => FieldSelectorAdvancedDropdownItem.GetHashCode(x, true)), state)
            {
                CurrentFolderContextualSearch = true;
                Vector2 minSize = minimumSize;
                minSize.y = 200;
                minimumSize = minSize;

                _rootNode = root;
                _bindingProperty = bindingProperty;
                _editableOnly = editableOnly;

                _selectableFilters.Add(new(SerializedPropertyTreeNode.FilterFuncs.IsGenericType, true));
                if (Settings.Instance._Limitter)
                {
                    _selectableFilters.Add(new(SerializedPropertyTreeNode.FilterFuncs.IsHighRisk, true));
                    _selectableFilters.Add(new(SerializedPropertyTreeNode.FilterFuncs.IsChange2Crash, true));

                    _innerNodeFilters.Add(new(SerializedPropertyTreeNode.FilterFuncs.IsHighRisk, true));
                    _innerNodeFilters.Add(new(SerializedPropertyTreeNode.FilterFuncs.IsChange2Crash, true));
                }
            }

            protected override FieldSelectorAdvancedDropdownItem GenericBuildRoot()
            {
                if (_rootNode == null)
                    return new(_rootNode?.Name, false, _rootNode);

                Dictionary<SerializedPropertyTreeNode, FieldSelectorAdvancedDropdownItem> nodeADItemPairs = new() { { _rootNode, new(_rootNode.Name, false, _rootNode) } };
                foreach (SerializedPropertyTreeNode child in _rootNode.Children)
                {
                    BuildRootInternal(child, nodeADItemPairs);
                }
                return nodeADItemPairs[_rootNode];
            }

            private void BuildRootInternal(
                SerializedPropertyTreeNode curNode, Dictionary<SerializedPropertyTreeNode, FieldSelectorAdvancedDropdownItem> nodeADItemPairs
            )
            {
                if (curNode.SerializedProperty == null)
                {
                    FieldSelectorAdvancedDropdownItem innerItem = new($"{curNode.Name} ->", false, curNode);
                    nodeADItemPairs[curNode.Parent].AddChild(innerItem);

                    FieldSelectorAdvancedDropdownItem dictRegisterItem = innerItem;
                    nodeADItemPairs.Add(curNode, dictRegisterItem);

                    foreach (SerializedPropertyTreeNode child in curNode.Children)
                    {
                        BuildRootInternal(child, nodeADItemPairs);
                    }
                }
                else
                {
                    SerializedPropertyTreeNode[] spNodeStack = curNode.GetNodeStackWithoutRoot();
                    SerializedProperty[] spStack = spNodeStack.Select(x => x.SerializedProperty).ToArray();


                    bool isSelectable = true;
                    bool isInnerNode = true;

                    isInnerNode &= curNode.Children.Any();
                    if (Settings.Instance._Limitter)
                    {
                        isSelectable &= curNode.Tags.Contains("Selectable");
                        if (_editableOnly)
                        {
                            isSelectable &= curNode.Tags.Contains("Editable");
                        }
                    }

                    isSelectable &= _selectableFilters.All(x => x.Calc(curNode.SerializedObject, spStack));
                    isInnerNode &= _innerNodeFilters.All(x => x.Calc(curNode.SerializedObject, spStack));

                    if (isSelectable || isInnerNode)
                    {
                        FieldSelectorAdvancedDropdownItem selectableItem = null;
                        if (isSelectable)
                        {
                            selectableItem = new(curNode.Name, true, curNode);
                            nodeADItemPairs[curNode.Parent].AddChild(selectableItem);
                        }

                        FieldSelectorAdvancedDropdownItem innerItem = null;
                        if (isInnerNode)
                        {
                            innerItem = new($"{curNode.Name} ->", false, curNode);
                            nodeADItemPairs[curNode.Parent].AddChild(innerItem);
                        }

                        FieldSelectorAdvancedDropdownItem dictRegisterItem = innerItem ?? selectableItem;
                        nodeADItemPairs.Add(curNode, dictRegisterItem);

                        if (isInnerNode)
                        {
                            foreach (SerializedPropertyTreeNode child in curNode.Children)
                            {
                                BuildRootInternal(child, nodeADItemPairs);
                            }
                        }
                    }
                }
            }

            protected override (string itemName, string description, string tooltip) BuildDisplayTexts(
                    FieldSelectorAdvancedDropdownItem item, string name, Texture2D icon, bool enabled, bool drawArrow, bool selected, bool hasSearch)
            {
                string itemName = "";
                string description = "";
                string tooltip = "";

                itemName = item.name;
                SerializedProperty sp = item.Node.SerializedProperty;
                if (sp != null)
                {
                    string path = sp.propertyPath;
                    tooltip = path;
                    if (hasSearch)
                    {
                        description = $"({path})";
                    }
                    else
                    {
                        if (item.name.StartsWith("data["))
                        {
                            string[] nameFieldNames = new[] { "m_name", "_name", "name" };
                            IEnumerable<SerializedPropertyTreeNode> nameNodes = item.Node.Children.Where(x => nameFieldNames.Contains(x.SerializedProperty.name.ToLower()));

                            foreach (SerializedPropertyTreeNode node in nameNodes)
                            {
                                if (node.SerializedProperty.propertyType == SerializedPropertyType.String)
                                {
                                    // マテリアルのプロパティ名を description に表示
                                    description = $"({node.SerializedProperty.stringValue})";
                                    break;
                                }
                            }
                        }

                        {
                            UnityEngine.Object targetObject = sp.serializedObject.targetObject;
                            bool isNull = EditorUtil.FakeNullUtil.IsNullOrFakeNull(targetObject);
                            if (!isNull)
                            {
                                string[] pathStack;
                                switch (targetObject)
                                {
                                    case Material:
                                        // マテリアルの m_SavedPropertiesの要素か
                                        if (
                                            sp.name == "data" &&
                                            (item.Node.Parent.SerializedProperty?.isArray ?? false) &&
                                            item.Node.Parent.Parent.SerializedProperty.arrayElementType == "pair" &&
                                            (pathStack = path.Split('.')).Length == 4 &&
                                            pathStack[0] == "m_SavedProperties" &&
                                            new[] { "m_TexEnvs", "m_Ints", "m_Floats", "m_Colors" }.Contains(pathStack[1])
                                        )
                                        {
                                            // マテリアルのプロパティ名を description に表示
                                            description = $"({sp.displayName})";
                                        }
                                        break;
                                    case AnimationClip:
                                        if (
                                            sp.name == "data" &&
                                            (item.Node.Parent.SerializedProperty?.isArray ?? false) &&
                                            item.Node.Parent.Parent.SerializedProperty.arrayElementType.EndsWith("Curve") &&
                                            (pathStack = path.Split('.')).Length == 3 &&
                                            pathStack[0].EndsWith("Curves") &&
                                            true
                                        )
                                        {
                                            string pathString = "";
                                            string attributeString = "";

                                            SerializedPropertyTreeNode pathSPNode = item.Node.Children.FirstOrDefault(x => x.SerializedProperty.name == "path");
                                            if (pathSPNode != null && pathSPNode.SerializedProperty.propertyType == SerializedPropertyType.String)
                                            {
                                                pathString = pathSPNode.SerializedProperty.stringValue;
                                            }

                                            switch (item.Node.Parent.Parent.SerializedProperty.arrayElementType)
                                            {
                                                case "FloatCurve":
                                                case "PPtrCurve":
                                                    SerializedPropertyTreeNode attributeSPNode = item.Node.Children.FirstOrDefault(x => x.SerializedProperty.name == "attribute");

                                                    if (attributeSPNode != null && attributeSPNode.SerializedProperty.propertyType == SerializedPropertyType.String)
                                                    {
                                                        attributeString = attributeSPNode.SerializedProperty.stringValue;
                                                    }
                                                    break;
                                                case "Vector3Curve":
                                                    switch (pathStack[0])
                                                    {
                                                        case "m_PositionCurves":
                                                            attributeString = "Position";
                                                            break;
                                                        case "m_EulerCurves":
                                                            attributeString = "Rotation";
                                                            break;
                                                        case "m_ScaleCurves":
                                                            attributeString = "Scale";
                                                            break;
                                                    }
                                                    break;
                                                case "QuaternionCurve":
                                                    switch (pathStack[0])
                                                    {
                                                        case "m_RotationCurves":
                                                            attributeString = "Quaternion";
                                                            break;
                                                    }
                                                    break;
                                                case "CompressedAnimationCurve":
                                                    switch (pathStack[0])
                                                    {
                                                        case "m_CompressedRotationCurves":
                                                            attributeString = "CompressedAnimation";
                                                            break;
                                                    }
                                                    break;
                                            }
                                            description = $"{pathString} : {attributeString}";
                                        }
                                        if (
                                            sp.name == "size" &&
                                            (item.Node.Parent.SerializedProperty?.isArray ?? false) &&
                                            item.Node.Parent.Parent.SerializedProperty.arrayElementType.EndsWith("Curve") &&
                                            (pathStack = path.Split('.')).Length == 3 &&
                                            pathStack[0].EndsWith("Curves")
                                        )
                                        {
                                            description = $"({item.Node.Parent.Parent.SerializedProperty.arrayElementType})";
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }

                return (itemName, description, tooltip);
            }

            protected override void GenericItemSelected(FieldSelectorAdvancedDropdownItem item)
            {
                _bindingProperty.stringValue = item.FullPath;
                _bindingProperty.serializedObject.ApplyModifiedProperties();
            }
        }

        private class FieldSelectorAdvancedDropdownItem : ExpantionAdvancedDropdownItem
        {
            public bool IsSelectable { get; }
            public SerializedPropertyTreeNode Node { get; }

            public override int Id { get => GetHashCode(); }

            public override string FullPath => Node?.FullPath ?? "";

            public FieldSelectorAdvancedDropdownItem(string displayName, bool isSelectable, SerializedPropertyTreeNode node) : base(displayName)
            {
                IsSelectable = isSelectable;
                Node = node;
            }

            public override int GetHashCode()
            {
                return GetHashCode(FullPath, IsSelectable);
            }

            public static int GetHashCode(string fullPath, bool isSelectable)
            {
                return (fullPath + (isSelectable ? "@value" : "")).GetHashCode();
            }
        }

        // ▲ 拡張AdvancedDropdown ========================= ▲
    }
}