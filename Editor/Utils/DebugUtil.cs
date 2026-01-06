
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public static class DebugUtil
    {
        public static void EventManagerDebugLog(BaseEventArgs args, bool isStart, bool isDebugMode)
        {
            if (!isDebugMode) return;

            string senderPropertyInstancePath = GetBindableElementIfExists(args);

            Debug.Log(
                $"イベントタイプ：{args.GetType().Name}\r\n" +
                $"ネスト：{UniversalEventManager.EventStacks.Count}\t" +
                $"始終：{(isStart ? "Start" : "End")}\t" +
                $"{senderPropertyInstancePath}"
            );
        }

        private static string GetBindableElementIfExists(BaseEventArgs eventArgs)
        {
            Type type = eventArgs.GetType();

            PropertyInfo targetProperty = type.GetProperty("SenderSerializedProperty");
            if (targetProperty != null)
            {
                SerializedProperty property = targetProperty.GetValue(eventArgs) as SerializedProperty;
                return SerializedObjectUtil.GetSerializedPropertyInstancePath(property);
            }
            return "";
        }

        public static void SetDebugLabelText(VisualElement uxml, string text)
        {
            Label u_DebugLabel = UIQuery.QOrNull<Label>(uxml, "DebugLabel");
            if (u_DebugLabel != null)
            {
                u_DebugLabel.text = text;
            }
        }

        public static void DebugLog(string mes, LogType logType, string color = "")
        {
            if (!Settings.Instance._DebugLog) return;

            if (!string.IsNullOrWhiteSpace(color))
            {
                mes = $"<color={color}>{mes}</color>";
            }

            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(mes);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(mes);
                    break;
                case LogType.Error:
                    Debug.LogError(mes);
                    break;
                case LogType.Assert:
                case LogType.Exception:
                    Debug.LogError("logTypeの指定が不正です!!!!!!!!!!!");
                    break;
            }
        }

        public static void ErrorDebugLog(string mes, LogType logType)
        {
            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(mes);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(mes);
                    break;
                case LogType.Error:
                    Debug.LogError(mes);
                    break;
                case LogType.Assert:
                case LogType.Exception:
                    Debug.LogError("logTypeの指定が不正です!!!!!!!!!!!");
                    break;
            }
        }
    }

}
