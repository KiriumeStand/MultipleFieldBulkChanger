using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal class Settings : ScriptableObject
    {
        private const string DebugSettingsPath = "Assets/MultipleFieldBulkChangerDebugSettings.asset";

        private static Settings _instance;

        [SerializeField]
        internal bool _Limitter = true;
        [SerializeField]
        internal bool _DebugMode = false;
        [SerializeField]
        internal bool _DebugLog = false;

        private Settings() { }

        internal static Settings Instance
        {
            get
            {
                if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(_instance))
                {
                    Settings loadedSettings = AssetDatabase.LoadAssetAtPath<Settings>(DebugSettingsPath);
                    if (EditorUtil.FakeNullUtil.IsNullOrFakeNull(loadedSettings))
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

    internal static class DebugSettingsRegister
    {
        [SettingsProvider]
        internal static SettingsProvider CreateProvider()
        {
            SettingsProvider provider = new("Preferences/MutipleFieldBulkChanger", SettingsScope.User, null)
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
