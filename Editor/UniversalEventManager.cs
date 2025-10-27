using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public static class UniversalEventManager
    {
        private static readonly List<(Type type, BaseEventArgs args)> eventStacks = new();
        public static ImmutableList<(Type type, BaseEventArgs args)> EventStacks => eventStacks.ToImmutableList();

        private static readonly Dictionary<Type, List<WeakReference<Delegate>>> eventHandlers = new();

        /// <summary>
        /// 現在実行中のイベント数
        /// </summary>
        public static int NowEventingCount => EventStacks.Count;

        /// <summary>
        /// イベント中か
        /// </summary>
        public static bool IsNowEventing => EventStacks.Count > 0;

        /// <summary>
        /// ネストされたイベント中か
        /// </summary>
        public static bool IsNowNestEventing => EventStacks.Count > 1;

        public static int ManagedEventCount
        {
            get
            {
                int counter = 0;
                foreach (List<WeakReference<Delegate>> item in eventHandlers.Values)
                {
                    counter += item.Count;
                }
                return counter;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>購読解除用Action</returns>
        public static (EventHandler<T>, Action) Subscribe<T>(EventHandler<T> handler, Func<T, bool> filter = null, bool allowNestEvent = false, bool isDebugMode = false) where T : BaseEventArgs
        {
            // フィルター関数が空なら無条件でtrueを返す関数で補完
            filter ??= (args) => true;

            // フィルター処理とイベント処理を結合した処理を作成
            EventHandler<T> filteredHandler = (sender, args) =>
            {
                if (filter(args) && (!IsNowNestEventing || allowNestEvent))
                {
                    // フィルターを通る、かつ、(このイベントがネストしていない、もしくは、イベントのネストが許可されている)なら
                    EditorUtil.Debugger.EventManagerDebugLog(args, true, isDebugMode);
                    // イベント処理を実行
                    handler(sender, args);
                    EditorUtil.Debugger.EventManagerDebugLog(args, false, isDebugMode);
                }
            };

            // 弱参照でアクションを保有
            WeakReference<Delegate> weakFilterdHandler = new(filteredHandler);

            Type eventType = typeof(T);

            // 該当するイベントのリストが無ければ追加
            if (!eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType] = new();
            // ハンドラーのリストに追加
            eventHandlers[eventType].Add(weakFilterdHandler);

            // 購読解除のアクションを作成
            void unsubscribeAction() => Unsubscribe<T>(weakFilterdHandler);

            // フィルター付きイベントハンドラーと購読解除アクションを返す
            return (filteredHandler, unsubscribeAction);
        }

        public static void Unsubscribe<T>(WeakReference<Delegate> weakHandler) where T : BaseEventArgs
        {
            Type eventType = typeof(T);

            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType].Remove(weakHandler);

                // ハンドラーのリストが空になった場合は辞書からも削除
                if (eventHandlers[eventType].Count == 0)
                    eventHandlers.Remove(eventType);
            }
        }

        public static void Publish<T>(T args) where T : BaseEventArgs
        {
            Type eventType = typeof(T);

            if (!eventHandlers.ContainsKey(eventType)) return;

            // 無効になり購読解除するハンドラー
            List<WeakReference<Delegate>> removeWeakHandlers = new();

            foreach (var weakHandler in eventHandlers[eventType].ToArray())
            {
                if (!(weakHandler.TryGetTarget(out Delegate handler) && handler is EventHandler<T> eventHandler))
                {
                    // 無効なイベントなら購読解除リストに追加
                    removeWeakHandlers.Add(weakHandler);
                    continue;
                }

                (Type type, BaseEventArgs args) stack = (eventType, args);
                eventStacks.Add(stack);
                try
                {
                    eventHandler?.Invoke(null, args);
                }
                catch (Exception ex)
                {
                    RuntimeUtil.Debugger.DebugLog($"{ex.GetType().Name} in {args.EventName}: {ex.Message}\r\n*----Stack Trace----*\r\n{ex.StackTrace}\r\n*----Stack Trace End----*\r\n", LogType.Error);
                }
                finally
                {
                    eventStacks.RemoveAt(eventStacks.Count - 1);
                }
            }

            // 無効なイベントの購読を解除
            foreach (WeakReference<Delegate> removeHandler in removeWeakHandlers)
            {
                Unsubscribe<T>(removeHandler);
            }

            // MARK: デバッグ用
            if (removeWeakHandlers.Count > 0)
            {
                RuntimeUtil.Debugger.DebugLog($"{ManagedEventCount}/removeWeakHandlers", LogType.Warning);
            }
        }
    }
}