using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{


    [CustomEditor(typeof(MultipleFieldBulkChanger))]
    public class MultipleFieldBulkChangerEditor : ExpansionEditor
    {
        private static readonly Regex XIndexRegex = new(@"^(.+?)(\d+)$", RegexOptions.Compiled);

        public MultipleFieldBulkChangerEditor() : base() { }

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreateInspectorGUICore(VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            MultipleFieldBulkChanger castedTargetObject = targetObject as MultipleFieldBulkChanger;

            MultipleFieldBulkChangerVM viewModel = MultipleFieldBulkChangerVM.GetInstance(serializedObject);

            // MARK: デバッグ用
            Button u_DebugButton = UIQuery.Q<Button>(uxml, "MFBC_DebugButton");
            u_DebugButton.clicked += () =>
            {
                (int count, int validCount) = UniversalEventManager.ManagedEventCount;
                DebugUtil.DebugLog(
                    $"drawerId:{EditorUtil.ObjectIdUtil.GetObjectId(this)}/targetId:{EditorUtil.ObjectIdUtil.GetObjectId(serializedObject.targetObject)}/propertyId:{EditorUtil.ObjectIdUtil.GetObjectId(serializedObject)}/Unsubscriptions/ManagedEventCount:{count}/valid:{validCount}/\r\n" +
                    $"\r\n" +
                    $"{UniversalDataManager.Debugger.UnsubscribeActionsInfoList}"
                    , LogType.Log);
            };
            VisualElementUtil.SetDisplay(u_DebugButton, Settings.Instance._DebugMode);

            Toggle u_Enable = BindHelper.Bind<Toggle>(uxml, UxmlNames.Enable, serializedObject, nameof(MultipleFieldBulkChanger._Enable));
            ListView u_Arguments = BindHelper.Bind<ListView>(uxml, UxmlNames.Arguments, serializedObject, nameof(MultipleFieldBulkChanger._ArgumentSettings));
            ListView u_ChangeSettings = BindHelper.Bind<ListView>(uxml, UxmlNames.ChangeSettings, serializedObject, nameof(MultipleFieldBulkChanger._FieldChangeSettings));

            // イベント発行の登録
            u_Arguments.itemsAdded += (e) =>
            {
                IExpansionInspectorCustomizer.AddListElementWithClone(castedTargetObject._ArgumentSettings, e);

                int[] addedIndexes = e.ToArray();

                // 新しい要素に使う引数名を取得
                HashSet<string> names = castedTargetObject._ArgumentSettings.Where(x => x != null).Select(x => x._ArgumentName).ToHashSet();
                string[] nextNames = GetNextIndexedName(names, "x", addedIndexes.Count());

                for (int i = 0; i < addedIndexes.Count(); i++)
                {
                    int curIndex = addedIndexes[i];
                    ArgumentSetting curElement = castedTargetObject._ArgumentSettings[curIndex];

                    curElement._ArgumentName = nextNames[i];
                }
            };
            u_Arguments.itemsRemoved += (e) =>
            {
                ListViewItemsRemovedEventArgs args = new(this, serializedObject, u_Arguments, status, e);
                ((IExpansionInspectorCustomizer)this).Publish(args);
            };

            u_ChangeSettings.itemsAdded += (e) =>
            {
                bool existQuoteTarget = IExpansionInspectorCustomizer.AddListElementWithClone(castedTargetObject._FieldChangeSettings, e);

                int[] addedIndexes = e.ToArray();

                if (!existQuoteTarget)
                {
                    // 一番上の有効な引数名を取得
                    string firstArgName = "x1";
                    foreach (ArgumentSetting item in castedTargetObject._ArgumentSettings)
                    {
                        if (!string.IsNullOrWhiteSpace(item._ArgumentName))
                        {
                            firstArgName = item._ArgumentName;
                            break;
                        }
                    }

                    for (int i = 0; i < addedIndexes.Count(); i++)
                    {
                        int curIndex = addedIndexes[i];
                        FieldChangeSetting curElement = castedTargetObject._FieldChangeSettings[curIndex];

                        curElement._Expression = firstArgName;
                    }
                }
            };
            u_ChangeSettings.itemsRemoved += (e) =>
            {
                ListViewItemsRemovedEventArgs args = new(this, serializedObject, u_ChangeSettings, status, e);
                ((IExpansionInspectorCustomizer)this).Publish(args);
            };

            // このオブジェクトをウィンドウ要素にバインド
            uxml.Bind(serializedObject);
        }

        public override void DelayCallCore(VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            // 参照値の更新をリクエストするイベントの発行処理
            void editorApplicationUpdateCallback()
            {
                if (!SerializedObjectUtil.IsValid(serializedObject))
                {
                    // MARK: デバッグ用
                    DebugUtil.DebugLog($"serializedObject/IsValid == false/editorApplicationUpdateCallback", LogType.Warning);
                    return;
                }
                if (serializedObject == null)
                {
                    // MARK: デバッグ用
                    DebugUtil.DebugLog($"ここは必要みたいです/editorApplicationUpdateCallback", LogType.Warning);
                    return;
                }

                // MARK: デバッグ用
                var argumentDataDictionary = UniversalDataManager.GetUniqueObjectDictionary<ArgumentData>(UniversalDataManager.IdentifierNames.ArgumentData);
                string text = $"argumentDataDictionary/{argumentDataDictionary.Count()}";
                DebugUtil.SetDebugLabelText(uxml, text);

                SelectedFieldSerializedPropertyReloadRequestEventArgs args = new(this, serializedObject, uxml, status);
                ((IExpansionInspectorCustomizer)this).Publish(args);
            }

            // 後で解除するために必要なのでUniqueObjectとして上記のイベントの発行処理を保管
            UniversalDataManager.RegisterUniqueObject((this, targetObject, serializedObject), UniversalDataManager.IdentifierNames.EditorApplicationUpdateIdentifier, (EditorApplication.CallbackFunction)editorApplicationUpdateCallback);

            // EditorApplication.updateに上記のイベントの発行処理を登録する
            EditorApplication.update += editorApplicationUpdateCallback;
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        public override void OnCleanup(SerializedObject serializedObject, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            string identifier = UniversalDataManager.IdentifierNames.EditorApplicationUpdateIdentifier;
            // EditorApplication.updateに登録した処理を取得
            var editorApplicationUpdateCallback = UniversalDataManager.GetUniqueObject<EditorApplication.CallbackFunction>((this, targetObject, serializedObject), identifier);
            // 不要なのでDBからは削除
            UniversalDataManager.ClearUniqueObject<EditorApplication.CallbackFunction>((this, targetObject, serializedObject), identifier);

            // EditorApplication.updateに登録していた処理を削除
            EditorApplication.update -= editorApplicationUpdateCallback;
        }

        private string[] GetNextIndexedName(IEnumerable<string> usedNames, string prefix, int needCount)
        {
            // 引数の番号一覧
            HashSet<int> argNums = new();
            foreach (string name in usedNames)
            {
                Match match = XIndexRegex.Match(name);
                if (match.Success && match.Groups[1].Value == prefix)
                {
                    // 一致したらその数字部をint値として取得する
                    string numString = match.Groups[2].Value;
                    if (int.TryParse(numString, out int num)) { argNums.Add(num); }
                }
            }

            List<string> resultNames = new();
            int unuseMinNum = 1;
            for (int i = 0; i < needCount; i++)
            {
                // 使われていない最小の値を引数名に使用する
                while (argNums.Contains(unuseMinNum))
                {
                    unuseMinNum++;
                }
                resultNames.Add($"{prefix}{unuseMinNum}");
                argNums.Add(unuseMinNum);
            }
            return resultNames.ToArray();
        }

        // ▲ メソッド ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public record UxmlNames
        {
            public static readonly string Enable = "MFBC_Enable";
            public static readonly string Arguments = "MFBC_Arguments";
            public static readonly string ChangeSettings = "MFBC_ChangeSettings";
        }

        // ▲ 名前辞書 ========================= ▲
    }

    //[CustomEditor(typeof(MultipleFieldBulkChanger))]
    public class MultipleFieldBulkChangerEditor_old : ExpansionEditor
    {
        private static readonly Regex XIndexRegex = new(@"^(.+?)(\d+)$", RegexOptions.Compiled);

        public MultipleFieldBulkChangerEditor_old() : base() { }

        // ▼ 初期化定義 ========================= ▼
        // MARK: ==初期化定義==

        public override void CreateInspectorGUICore(VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            MultipleFieldBulkChanger castedTargetObject = targetObject as MultipleFieldBulkChanger;

            // MARK: デバッグ用
            Button u_DebugButton = UIQuery.Q<Button>(uxml, "MFBC_DebugButton");
            u_DebugButton.clicked += () =>
            {
                (int count, int validCount) = UniversalEventManager.ManagedEventCount;
                DebugUtil.DebugLog(
                    $"drawerId:{EditorUtil.ObjectIdUtil.GetObjectId(this)}/targetId:{EditorUtil.ObjectIdUtil.GetObjectId(serializedObject.targetObject)}/propertyId:{EditorUtil.ObjectIdUtil.GetObjectId(serializedObject)}/Unsubscriptions/ManagedEventCount:{count}/valid:{validCount}/\r\n" +
                    $"\r\n" +
                    $"{UniversalDataManager.Debugger.UnsubscribeActionsInfoList}"
                    , LogType.Log);
            };
            VisualElementUtil.SetDisplay(u_DebugButton, Settings.Instance._DebugMode);

            Toggle u_Enable = BindHelper.Bind<Toggle>(uxml, UxmlNames.Enable, serializedObject, nameof(MultipleFieldBulkChanger._Enable));
            ListView u_Arguments = BindHelper.Bind<ListView>(uxml, UxmlNames.Arguments, serializedObject, nameof(MultipleFieldBulkChanger._ArgumentSettings));
            ListView u_ChangeSettings = BindHelper.Bind<ListView>(uxml, UxmlNames.ChangeSettings, serializedObject, nameof(MultipleFieldBulkChanger._FieldChangeSettings));

            // イベント発行の登録
            u_Arguments.itemsAdded += (e) =>
            {
                IExpansionInspectorCustomizer.AddListElementWithClone(castedTargetObject._ArgumentSettings, e);

                int[] addedIndexes = e.ToArray();

                // 新しい要素に使う引数名を取得
                HashSet<string> names = castedTargetObject._ArgumentSettings.Where(x => x != null).Select(x => x._ArgumentName).ToHashSet();
                string[] nextNames = GetNextIndexedName(names, "x", addedIndexes.Count());

                for (int i = 0; i < addedIndexes.Count(); i++)
                {
                    int curIndex = addedIndexes[i];
                    ArgumentSetting curElement = castedTargetObject._ArgumentSettings[curIndex];

                    curElement._ArgumentName = nextNames[i];
                }
            };
            u_Arguments.itemsRemoved += (e) =>
            {
                ListViewItemsRemovedEventArgs args = new(this, serializedObject, u_Arguments, status, e);
                ((IExpansionInspectorCustomizer)this).Publish(args);
            };

            u_ChangeSettings.itemsAdded += (e) =>
            {
                bool existQuoteTarget = IExpansionInspectorCustomizer.AddListElementWithClone(castedTargetObject._FieldChangeSettings, e);

                int[] addedIndexes = e.ToArray();

                if (!existQuoteTarget)
                {
                    // 一番上の有効な引数名を取得
                    string firstArgName = "x1";
                    foreach (ArgumentSetting item in castedTargetObject._ArgumentSettings)
                    {
                        if (!string.IsNullOrWhiteSpace(item._ArgumentName))
                        {
                            firstArgName = item._ArgumentName;
                            break;
                        }
                    }

                    for (int i = 0; i < addedIndexes.Count(); i++)
                    {
                        int curIndex = addedIndexes[i];
                        FieldChangeSetting curElement = castedTargetObject._FieldChangeSettings[curIndex];

                        curElement._Expression = firstArgName;
                    }
                }
            };
            u_ChangeSettings.itemsRemoved += (e) =>
            {
                ListViewItemsRemovedEventArgs args = new(this, serializedObject, u_ChangeSettings, status, e);
                ((IExpansionInspectorCustomizer)this).Publish(args);
            };
            u_ChangeSettings.itemIndexChanged += (e1, e2) =>
            {
                DebugUtil.DebugLog($"u_ChangeSettings.itemIndexChanged/{e1}/{e2}", LogType.Warning);
            };

            // このオブジェクトをウィンドウ要素にバインド
            uxml.Bind(serializedObject);
        }

        public override void DelayCallCore(VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            // 参照値の更新をリクエストするイベントの発行処理
            void editorApplicationUpdateCallback()
            {
                if (!SerializedObjectUtil.IsValid(serializedObject))
                {
                    // MARK: デバッグ用
                    DebugUtil.DebugLog($"serializedObject/IsValid == false/editorApplicationUpdateCallback", LogType.Warning);
                    return;
                }
                if (serializedObject == null)
                {
                    // MARK: デバッグ用
                    DebugUtil.DebugLog($"ここは必要みたいです/editorApplicationUpdateCallback", LogType.Warning);
                    return;
                }

                // MARK: デバッグ用
                var argumentDataDictionary = UniversalDataManager.GetUniqueObjectDictionary<ArgumentData>(UniversalDataManager.IdentifierNames.ArgumentData);
                string text = $"argumentDataDictionary/{argumentDataDictionary.Count()}";
                DebugUtil.SetDebugLabelText(uxml, text);

                SelectedFieldSerializedPropertyReloadRequestEventArgs args = new(this, serializedObject, uxml, status);
                ((IExpansionInspectorCustomizer)this).Publish(args);
            }

            // 後で解除するために必要なのでUniqueObjectとして上記のイベントの発行処理を保管
            UniversalDataManager.RegisterUniqueObject((this, targetObject, serializedObject), UniversalDataManager.IdentifierNames.EditorApplicationUpdateIdentifier, (EditorApplication.CallbackFunction)editorApplicationUpdateCallback);

            // EditorApplication.updateに上記のイベントの発行処理を登録する
            EditorApplication.update += editorApplicationUpdateCallback;
        }

        // ▲ 初期化定義 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        // ▲ イベントハンドラー ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        public override void OnCleanup(SerializedObject serializedObject, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            string identifier = UniversalDataManager.IdentifierNames.EditorApplicationUpdateIdentifier;
            // EditorApplication.updateに登録した処理を取得
            var editorApplicationUpdateCallback = UniversalDataManager.GetUniqueObject<EditorApplication.CallbackFunction>((this, targetObject, serializedObject), identifier);
            // 不要なのでDBからは削除
            UniversalDataManager.ClearUniqueObject<EditorApplication.CallbackFunction>((this, targetObject, serializedObject), identifier);

            // EditorApplication.updateに登録していた処理を削除
            EditorApplication.update -= editorApplicationUpdateCallback;
        }

        private string[] GetNextIndexedName(IEnumerable<string> usedNames, string prefix, int needCount)
        {
            // 引数の番号一覧
            HashSet<int> argNums = new();
            foreach (string name in usedNames)
            {
                Match match = XIndexRegex.Match(name);
                if (match.Success && match.Groups[1].Value == prefix)
                {
                    // 一致したらその数字部をint値として取得する
                    string numString = match.Groups[2].Value;
                    if (int.TryParse(numString, out int num)) { argNums.Add(num); }
                }
            }

            List<string> resultNames = new();
            int unuseMinNum = 1;
            for (int i = 0; i < needCount; i++)
            {
                // 使われていない最小の値を引数名に使用する
                while (argNums.Contains(unuseMinNum))
                {
                    unuseMinNum++;
                }
                resultNames.Add($"{prefix}{unuseMinNum}");
                argNums.Add(unuseMinNum);
            }
            return resultNames.ToArray();
        }

        // ▲ メソッド ========================= ▲


        // ▼ 名前辞書 ========================= ▼
        // MARK: ==名前辞書==

        public record UxmlNames
        {
            public static readonly string Enable = "MFBC_Enable";
            public static readonly string Arguments = "MFBC_Arguments";
            public static readonly string ChangeSettings = "MFBC_ChangeSettings";
        }

        // ▲ 名前辞書 ========================= ▲
    }

}