using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class ViewModelHelper
    {
        internal static TPropVM EnsureAndRecalculate<TPropVM, TPropModel>(this TPropVM propVM, SerializedProperty propSP)
            where TPropVM : ViewModelPropertyBase<TPropModel>, new()
            where TPropModel : class
        {
            if (propVM == null)
            {
                propVM = new();
                propVM.InitializeSetting(propSP);
            }

            propVM.Recalculate();

            return propVM;
        }

        internal static void EnsureAndRecalculateByList<TPropVM, TPropModel>(List<TPropVM> propVMList, SerializedProperty propListSP)
            where TPropVM : ViewModelPropertyBase<TPropModel>, new()
            where TPropModel : class
        {
            if (propVMList.Count() > propListSP.arraySize)
            {
                propVMList.RemoveRange(propListSP.arraySize, propVMList.Count() - propListSP.arraySize);
            }

            for (int i = 0; i < propListSP.arraySize; i++)
            {
                if (propVMList.Count() <= i)
                {
                    TPropVM propVM = new();
                    SerializedProperty propSP = propListSP.GetArrayElementAtIndex(i);
                    propVM.InitializeSetting(propSP);
                    propVMList.Add(propVM);
                }

                propVMList[i].Recalculate();
            }
        }
    }
}