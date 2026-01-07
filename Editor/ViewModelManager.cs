using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class ViewModelManager
    {
        static ViewModelManager()
        {
            FieldInfoDicInitialize();
        }

        private static void FieldInfoDicInitialize()
        {
            Type mfbcType = typeof(MultipleFieldBulkChanger);
            Type mfbcVMType = typeof(MultipleFieldBulkChangerVM);
            FieldInfoDicAddEntry(mfbcType, nameof(MultipleFieldBulkChanger._ArgumentSettings), mfbcVMType, nameof(MultipleFieldBulkChangerVM.vm_ArgumentSettings));
            FieldInfoDicAddEntry(mfbcType, nameof(MultipleFieldBulkChanger._FieldChangeSettings), mfbcVMType, nameof(MultipleFieldBulkChangerVM.vm_FieldChangeSettings));

            Type asType = typeof(ArgumentSetting);
            Type asVMType = typeof(ArgumentSettingVM);
            FieldInfoDicAddEntry(asType, nameof(ArgumentSetting._SourceField), asVMType, nameof(ArgumentSettingVM.vm_SourceField));

            Type fcsType = typeof(FieldChangeSetting);
            Type fcsVMType = typeof(FieldChangeSettingVM);
            FieldInfoDicAddEntry(fcsType, nameof(FieldChangeSetting._TargetFields), fcsVMType, nameof(FieldChangeSettingVM.vm_TargetFields));

            Type sfscType = typeof(SingleFieldSelectorContainer);
            Type sfscVMType = typeof(SingleFieldSelectorContainerVM);
            FieldInfoDicAddEntry(sfscType, nameof(SingleFieldSelectorContainer._FieldSelector), sfscVMType, nameof(SingleFieldSelectorContainerVM.vm_FieldSelector));

            Type mfscType = typeof(MultipleFieldSelectorContainer);
            Type mfscVMType = typeof(MultipleFieldSelectorContainerVM);
            FieldInfoDicAddEntry(mfscType, nameof(MultipleFieldSelectorContainer._FieldSelectors), mfscVMType, nameof(MultipleFieldSelectorContainerVM.vm_FieldSelectors));
        }

        private static void FieldInfoDicAddEntry(Type type1, string fieldName1, Type type2, string fieldName2)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            (FieldInfo, FieldInfo) newEntry = (type1.GetField(fieldName1, flags), type2.GetField(fieldName2, flags));
            int existIndex = target2VMFieldInfoPair.FindIndex(x => x.Item1 == newEntry.Item1);
            if (existIndex >= 0)
            {
                target2VMFieldInfoPair.RemoveAt(existIndex);
            }
            target2VMFieldInfoPair.Add(newEntry);
        }

        private static readonly List<(FieldInfo, FieldInfo)> target2VMFieldInfoPair = new();

        internal static string GetVMSPPath(SerializedProperty targetSP)
        {
            SerializedProperty[] spStack = SerializedObjectUtil.GetSerializedPropertyStack(targetSP);

            List<string> relativePathStack = new();
            string prevAbsolutePath = "";
            for (int i = 0; i < spStack.Length; i++)
            {
                SerializedProperty curSP = spStack[i];
                string curRelativePath = curSP.propertyPath;
                if (prevAbsolutePath != "")
                {
                    curRelativePath = curSP.propertyPath[(prevAbsolutePath.Length + 1)..];
                }
                relativePathStack.Add(curRelativePath);

                prevAbsolutePath = curSP.propertyPath;

                Type parentType = null;
                if (i > 0)
                {
                    (bool success, Type type, string errorLog) = spStack[i - 1].GetFieldType();
                    parentType = type;
                }
                else
                {
                    parentType = curSP.serializedObject.targetObject.GetType();
                }

                (FieldInfo, FieldInfo) findPair = target2VMFieldInfoPair.FirstOrDefault(
                    x => x.Item1.ReflectedType == parentType &&
                    (curRelativePath == x.Item1.Name || curRelativePath.StartsWith($"{x.Item1.Name}."))
                );
                FieldInfo targetType = findPair.Item1;
                FieldInfo vmType = findPair.Item2;
                if (targetType == null || vmType == null) continue;

                // 相対パスをViewModel用に置き換える
                relativePathStack[i] = vmType.Name + relativePathStack[i][targetType.Name.Length..];
            }
            string vmSPPath = string.Join(".", relativePathStack);
            return vmSPPath;
        }
    }
}