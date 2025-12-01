using System.Collections.Generic;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class Settings : ScriptableObject
    {
        public const string DebugSettingsPath = "Assets/MultipleFieldBulkChangerDebugSettings.asset";

        private static Settings _instance;

        public bool _Limitter = true;
        public bool _DebugMode = false;
        public bool _DebugLog = false;

        private Settings() { }


        public static Settings Instance
        {
            get
            {
                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(_instance))
                {
                    Settings loadedSettings = AssetDatabase.LoadAssetAtPath<Settings>(DebugSettingsPath);
                    if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(loadedSettings))
                    {
                        _instance = CreateInstance<Settings>();
                        AssetDatabase.CreateAsset(_instance, DebugSettingsPath);
                        AssetDatabase.SaveAssets();
                    }
                    else
                    {
                        _instance = loadedSettings;
                    }
                }
                return _instance;
            }
        }
    }

    public static class DebugSettingsRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider("Preferences/MutipleFieldBulkChanger", SettingsScope.User, null)
            {
                // タイトル
                label = "Mutiple Field Bulk Changer",
                // GUI描画
                guiHandler = searchContext =>
                {
                    SerializedObject debugSettingsSO = new(Settings.Instance);
                    EditorGUILayout.PropertyField(debugSettingsSO.FindProperty(nameof(Settings._Limitter)), new GUIContent("Limitter"));
                    EditorGUILayout.PropertyField(debugSettingsSO.FindProperty(nameof(Settings._DebugMode)), new GUIContent("Debug Mode"));
                    EditorGUILayout.PropertyField(debugSettingsSO.FindProperty(nameof(Settings._DebugLog)), new GUIContent("Debug Log"));
                    debugSettingsSO.ApplyModifiedPropertiesWithoutUndo();
                },
                // 検索時のキーワード
                keywords = new HashSet<string>(new[] { "Mutiple Field Bulk Changer" })
            };

            return provider;
        }
    }
}
