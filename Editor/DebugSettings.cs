using System.Collections.Generic;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public class DebugSettings : ScriptableObject
    {
        public const string DebugSettingsPath = "Assets/MultipleFieldBulkChangerDebugSettings.asset";

        private static DebugSettings _instance;

        [SerializeField]
        public bool _DebugMode = false;
        [SerializeField]
        public bool _DebugLog = false;


        public static DebugSettings Instance
        {
            get
            {
                if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(_instance))
                {
                    DebugSettings loadedSettings = AssetDatabase.LoadAssetAtPath<DebugSettings>(DebugSettingsPath);
                    if (RuntimeUtil.FakeNullUtil.IsNullOrFakeNull(loadedSettings))
                    {
                        _instance = CreateInstance<DebugSettings>();
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
                    SerializedObject debugSettingsSO = new(DebugSettings.Instance);
                    EditorGUILayout.PropertyField(debugSettingsSO.FindProperty(nameof(DebugSettings._DebugMode)), new GUIContent("Debug Mode"));
                    EditorGUILayout.PropertyField(debugSettingsSO.FindProperty(nameof(DebugSettings._DebugLog)), new GUIContent("Debug Log"));
                    debugSettingsSO.ApplyModifiedPropertiesWithoutUndo();
                },
                // 検索時のキーワード
                keywords = new HashSet<string>(new[] { "Mutiple Field Bulk Changer" })
            };

            return provider;
        }
    }
}
