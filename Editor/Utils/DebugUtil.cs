
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public static class DebugUtil
    {
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
