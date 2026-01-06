using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class UniversalEventManager
    {
        private static readonly List<(Type type, BaseEventArgs args)> eventStacks = new();
        internal static ImmutableList<(Type type, BaseEventArgs args)> EventStacks => eventStacks.ToImmutableList();

        private static readonly Dictionary<Type, List<Delegate>> eventHandlers = new();

        /// <summary>
        /// ネストされたイベント中か
        /// </summary>
        internal static bool IsNowNestEventing => EventStacks.Count > 1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>購読解除用Action</returns>
        internal static (EventHandler<T>, Action) Subscribe<T>(EventHandler<T> handler, Func<T, bool> filter = null, bool allowNestEvent = false, bool isDebugMode = false) where T : BaseEventArgs
        {
            // フィルター関数が空なら無条件でtrueを返す関数で補完
            filter ??= (args) => true;

            // フィルター処理とイベント処理を結合した処理を作成
            EventHandler<T> filteredHandler = (sender, args) =>
            {
                if (filter(args) && (!IsNowNestEventing || allowNestEvent))
                {
                    // フィルターを通る、かつ、(このイベントがネストしていない、もしくは、イベントのネストが許可されている)なら
                    DebugUtil.EventManagerDebugLog(args, true, isDebugMode);
                    // イベント処理を実行
                    handler(sender, args);
                    DebugUtil.EventManagerDebugLog(args, false, isDebugMode);
                }
            };

            Type eventType = typeof(T);

            // 該当するイベントのリストが無ければ追加
            if (!eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType] = new();
            // ハンドラーのリストに追加
            eventHandlers[eventType].Add(filteredHandler);

            // 購読解除のアクションを作成
            void unsubscribeAction() => Unsubscribe<T>(filteredHandler);

            // フィルター付きイベントハンドラーと購読解除アクションを返す
            return (filteredHandler, unsubscribeAction);
        }

        internal static void Unsubscribe<T>(Delegate handler) where T : BaseEventArgs
        {
            Type eventType = typeof(T);

            if (eventHandlers.ContainsKey(eventType))
            {
                eventHandlers[eventType].Remove(handler);

                // ハンドラーのリストが空になった場合は辞書からも削除
                if (eventHandlers[eventType].Count == 0)
                    eventHandlers.Remove(eventType);
            }
        }

        internal static void Publish<T>(T args) where T : BaseEventArgs
        {
            Type eventType = typeof(T);

            if (!eventHandlers.ContainsKey(eventType)) return;

            // 無効になり購読解除するハンドラー
            List<Delegate> removeHandlers = new();

            foreach (Delegate handler in eventHandlers[eventType].ToArray())
            {
                if (handler is not EventHandler<T> eventHandler)
                {
                    // 無効なイベントなら購読解除リストに追加
                    removeHandlers.Add(handler);
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
                    DebugUtil.DebugLog($"{ex.GetType().Name} in {args.EventName}: {ex.Message}\r\n*----Stack Trace----*\r\n{ex.StackTrace}\r\n*----Stack Trace End----*\r\n", LogType.Error);
                }
                finally
                {
                    eventStacks.RemoveAt(eventStacks.Count - 1);
                }
            }

            // 無効なイベントの購読を解除
            foreach (Delegate removeHandler in removeHandlers)
            {
                Unsubscribe<T>(removeHandler);
            }
        }
    }
}