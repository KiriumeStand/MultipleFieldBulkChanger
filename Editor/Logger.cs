
using System;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public static class Logger
    {
        public static void DebugLog(string mes, LogType logType = LogType.Log, string color = "")
        {
            if (!Settings.Instance._DebugLog) return;

            Log(mes, logType, color);
        }

        public static void Log(string mes, LogType logType = LogType.Log, string color = "")
        {
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
                    throw new ArgumentException("LogType の指定が不正です");
            }
        }
    }
}
