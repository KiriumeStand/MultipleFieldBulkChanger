using System;
using System.Collections.Generic;
using System.Linq;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public interface IExpansionInspectorCustomizer
    {
        public abstract StyleSheet USS { get; }
        public abstract VisualTreeAsset UXML { get; }

        // ▼ 初期化定義関連 ========================= ▼
        // MARK: ==初期化定義関連==

        /// <summary>
        /// CreateInspectorGUIかCreatePropertyGUIの中でこれを呼び出す必要があります。
        /// </summary>
        /// <param name="serializedData"></param>
        /// <returns></returns>
        public virtual VisualElement CreateCustomizerGUI(IDisposable serializedData)
        {

            if (!SerializedObjectUtil.IsValid(serializedData)) return null;

            // UXML をインスタンス化
            VisualElement uxml = UXML.CloneTree();
            // ussを適用
            uxml.styleSheets.Add(USS);

            IExpansionInspectorCustomizerTargetMarker targetObject = MFBCHelper.GetTargetObject(serializedData);

            InspectorCustomizerStatus status = new();

            // メインの初期化処理
            CreateCustomizerGUIInternal(serializedData, uxml, targetObject, status);

            // DetachFromPanelEventで自動クリーンアップ
            uxml.RegisterCallback<DetachFromPanelEvent>(e =>
            {
                OnDetachFromPanelEvent(serializedData, uxml, targetObject, status);
            });

            EditorApplication.delayCall += () =>
            {
                DelayCall(serializedData, uxml, targetObject, status);
            };

            // MARK: デバッグ用(常設)
            Label u_DebugLabel = UIQuery.QOrNull<Label>(uxml, "DebugLabel");
            if (u_DebugLabel != null)
            {
                if (string.IsNullOrWhiteSpace(u_DebugLabel.text))
                {
                    VisualElementUtil.SetDisplay(u_DebugLabel, Settings.Instance._DebugMode);
                }
            }

            status.SetPhase(InspectorCustomizerStatus.Phase.BeforeDelayCall);

            return uxml;
        }

        /// <summary>
        /// この内部でCreateInspectorGUICoreかCreatePropertyGUICoreを呼び出す必要があります。
        /// </summary>
        /// <param name="serializedData"></param>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        private void CreateCustomizerGUIInternal(
                IDisposable serializedData,
                VisualElement uxml,
                IExpansionInspectorCustomizerTargetMarker targetObject,
                InspectorCustomizerStatus status)
        {
            HandleSerializedType(
                serializedData,
                (so) =>
                {
                    CreateInspectorGUICore(uxml, targetObject, status);
                    PostCreateInspectorGUICore(uxml, targetObject, status);
                },
                (sp) =>
                {
                    CreatePropertyGUICore(sp, uxml, targetObject, status);
                    PostCreatePropertyGUICore(sp, uxml, targetObject, status);
                }
            );
        }

        /// <summary>
        /// UnityのCreateInspectorGUIの代わりにユーザーが継承クラスで実装するメソッド
        /// </summary>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void CreateInspectorGUICore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        /// <summary>
        /// CreateInspectorGUICore直後に呼ばれるメソッド
        /// </summary>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void PostCreateInspectorGUICore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        /// <summary>
        /// UnityのCreatePropertyGUIの代わりにユーザーが継承クラスで実装するメソッド
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void CreatePropertyGUICore(
            SerializedProperty sp,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        /// <summary>
        /// PostCreatePropertyGUICore直後に呼ばれるメソッド
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void PostCreatePropertyGUICore(
            SerializedProperty sp,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        /// <summary>
        /// EditorApplication.delayCallで呼び出される処理
        /// </summary>
        /// <param name="serializedData"></param>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        private void DelayCall(IDisposable serializedData, VisualElement uxml, IExpansionInspectorCustomizerTargetMarker targetObject, InspectorCustomizerStatus status)
        {
            if (!SerializedObjectUtil.IsValid(serializedData)) return;

            // CurrentPhaseがDelayCall実行中以降を示していれば処理をせずに戻る
            if (status.CurrentPhase > InspectorCustomizerStatus.Phase.BeforeDelayCall) return;

            status.SetPhase(InspectorCustomizerStatus.Phase.DelayCall);
            HandleSerializedType(
                serializedData,
                (so) => DelayCallCore(uxml, targetObject, status),
                (sp) => DelayCallCore(sp, uxml, targetObject, status)
            );
            status.SetPhase(InspectorCustomizerStatus.Phase.AfterDelayCall);
            return;
        }

        /// <summary>
        /// Editor用DelayCall処理
        /// </summary>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void DelayCallCore(
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        /// <summary>
        /// PropertyDrawer用DelayCall処理
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void DelayCallCore(
            SerializedProperty sp,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        // ▲ 初期化定義関連 ========================= ▲


        // ▼ イベントハンドラー ========================= ▼
        // MARK: ==イベントハンドラー==

        /// <summary>
        /// リソース解放（通常は自動で呼ばれる）
        /// </summary>
        public void OnDetachFromPanelEvent(
            IDisposable serializedData,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        {
            DrawerCleanup(serializedData, uxml, targetObject, status);
        }

        // ▲ イベントハンドラー ========================= ▲


        // ▼ イベント制御関連 ========================= ▼
        // MARK: ==イベント制御関連==

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inspectorCustomizer"></param>
        /// <param name="serializedData"></param>
        /// <param name="status"></param>
        /// <param name="handler"></param>
        /// <param name="filter"></param>
        /// <param name="allowNestEvent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Action Subscribe<T>(IExpansionInspectorCustomizer inspectorCustomizer, IDisposable serializedData, InspectorCustomizerStatus status, EventHandler<T> handler, Func<T, bool> filter = null, bool allowNestEvent = false) where T : BaseEventArgs
        {
            if (SerializedObjectUtil.IsValid(serializedData) != true)
            {
                return () => { };
            }

            // Cleanup以降の購読登録は認めない
            if (status.CurrentPhase >= InspectorCustomizerStatus.Phase.Cleanup) return () => { };

            IExpansionInspectorCustomizerTargetMarker targetObject = MFBCHelper.GetTargetObject(serializedData);
            if (targetObject == null)
            {
                return () => { };
            }

            // PhaseがCleanup以降なら処理を行わないフィルタを追加する
            bool wrapedFilter(T e) { return (status.CurrentPhase < InspectorCustomizerStatus.Phase.Cleanup) && filter(e); }

            (EventHandler<T> eventHandler, Action unsubscribeAction) = UniversalEventManager.Subscribe(handler, wrapedFilter, allowNestEvent);

            string identifier = UniversalDataManager.IdentifierNames.UnsubscribeAction;
            RegisterUnsubscribeAction<T>(serializedData, targetObject, identifier, unsubscribeAction, EditorUtil.ObjectIdUtil.GetObjectId(inspectorCustomizer));
            return unsubscribeAction;
        }

        /// <summary>
        /// カスタム購読解除アクションの登録
        /// </summary>
        /// <param name="serializedData"></param>
        /// <param name="targetObject"></param>
        /// <param name="identifier"></param>
        /// <param name="unsubscribeAction"></param>
        /// <param name="drawerId"></param>
        /// <typeparam name="T"></typeparam>
        private void RegisterUnsubscribeAction<T>(
            IDisposable serializedData, IExpansionInspectorCustomizerTargetMarker targetObject, string identifier,
            Action unsubscribeAction, long drawerId) where T : BaseEventArgs
        {
            // 目的の辞書を取り出す
            var unsubscribeActionListDictionary = UniversalDataManager.GetUniqueObjectDictionary<List<Action>>(identifier);
            if (!unsubscribeActionListDictionary.ContainsKey((this, targetObject, serializedData)))
            {
                // 辞書に合致する要素が無ければ新しく作成
                List<Action> newElement = new();
                unsubscribeActionListDictionary[(this, targetObject, serializedData)] = newElement;
                UniversalDataManager.Debugger.UnsubscribeActionInfosDictionary[(this, targetObject, serializedData)] = new();
            }

            ((List<Action>)unsubscribeActionListDictionary[(this, targetObject, serializedData)]).Add(unsubscribeAction);
            UniversalDataManager.Debugger.UnsubscribeActionInfosDictionary[(this, targetObject, serializedData)].Add((drawerId, GetType().Name, typeof(T).Name));
        }

        /// <summary>
        /// 購読をすべて解除（通常は自動で呼ばれる）
        /// </summary>
        /// <param name="serializedData"></param>
        /// <param name="targetObject"></param>
        public void Unsubscriptions(IDisposable serializedData, IExpansionInspectorCustomizerTargetMarker targetObject)
        {
            string identifier = UniversalDataManager.IdentifierNames.UnsubscribeAction;
            var unsubscribeActionListDictionary = UniversalDataManager.GetUniqueObjectDictionary<List<Action>>(identifier);
            unsubscribeActionListDictionary.TryGetValue((this, targetObject, serializedData), out object uniqueObject);

            if (uniqueObject is not List<Action> unsubscribeActionList) return;

            foreach (Action unsubscribe in unsubscribeActionList)
            {
                try
                {
                    unsubscribe();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"イベントの購読解除に失敗しました: {ex.Message}");
                }
            }

            unsubscribeActionListDictionary.Remove((this, targetObject, serializedData));
            UniversalDataManager.Debugger.UnsubscribeActionInfosDictionary.Remove((this, targetObject, serializedData));
        }

        public void Publish<T>(T args) where T : BaseEventArgs
        {
            if (!InspectorCustomizerStatus.DisableEventPublish) UniversalEventManager.Publish(args);
        }

        // ▲ イベント制御関連 ========================= ▲


        // ▼ UniqueObject関連 ========================= ▼
        // MARK: ==UniqueObject関連==

        // ▲ UniqueObject関連 ========================= ▲


        // ▼ メソッド ========================= ▼
        // MARK: ==メソッド==

        public void DrawerCleanup(
            IDisposable serializedData,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        {
            status?.SetPhase(InspectorCustomizerStatus.Phase.Cleanup);
            HandleSerializedType(
                serializedData,
                (so) => OnCleanup(so, uxml, targetObject, status),
                (sp) => OnCleanup(sp, uxml, targetObject, status));
            Unsubscriptions(serializedData, targetObject);
            UniversalDataManager.CleanupByInspectorCustomizerIdentifier((this, targetObject, serializedData));
            status?.SetPhase(InspectorCustomizerStatus.Phase.AfterCleanup);
        }

        /// <summary>
        /// Editor用OnCleanup処理
        /// </summary>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void OnCleanup(
            SerializedObject so,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        /// <summary>
        /// PropertyDrawer用OnCleanup処理
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="uxml"></param>
        /// <param name="targetObject"></param>
        /// <param name="status"></param>
        public virtual void OnCleanup(
            SerializedProperty sp,
            VisualElement uxml,
            IExpansionInspectorCustomizerTargetMarker targetObject,
            InspectorCustomizerStatus status)
        { }

        private static void HandleSerializedType(IDisposable serializedData, Action<SerializedObject> soAction, Action<SerializedProperty> spAction)
        {
            switch (serializedData)
            {
                case SerializedObject so:
                    soAction(so);
                    break;
                case SerializedProperty sp:
                    spAction(sp);
                    break;
                default:
                    throw new ArgumentException($"{nameof(serializedData)}の型が不正です。", nameof(serializedData));
            }
        }

        public static bool AddListElementWithClone<T>(List<T> list, IEnumerable<int> indexes) where T : ICloneable, new()
        {
            int[] addedIndexes = indexes.ToArray();

            T quoteTarget = default;
            if (addedIndexes.Min() > 0)
            {
                quoteTarget = list[addedIndexes.Min() - 1];
            }

            for (int i = 0; i < addedIndexes.Count(); i++)
            {
                int curIndex = addedIndexes[i];

                // 新しい要素を追加
                if (quoteTarget == null)
                {
                    list[curIndex] = new();
                }
                else
                {
                    // データをコピーする
                    list[curIndex] = (T)quoteTarget.Clone();
                }
            }

            return quoteTarget != null;
        }

        // ▲ メソッド ========================= ▲
    }
}