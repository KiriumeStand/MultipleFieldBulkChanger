using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class ViewModelHelper
    {
        internal static TVMProp EnsureAndRecalculate<TVMProp, TModel>(this TVMProp vmProp, SerializedProperty modelPropSP)
            where TVMProp : ViewModelPropertyBase<TModel>, new()
            where TModel : class
        {
            if (vmProp == null)
            {
                vmProp = new();
                vmProp.InitializeSetting(modelPropSP);
            }

            vmProp.Recalculate();

            return vmProp;
        }

        internal static void EnsureAndRecalculateByList<TVMProp, TModel>(List<TVMProp> vmPropList, SerializedProperty modelPropListSP)
            where TVMProp : ViewModelPropertyBase<TModel>, new()
            where TModel : class
        {
            if (vmPropList.Count() > modelPropListSP.arraySize)
            {
                vmPropList.RemoveRange(modelPropListSP.arraySize, vmPropList.Count() - modelPropListSP.arraySize);
            }

            for (int i = 0; i < modelPropListSP.arraySize; i++)
            {
                if (vmPropList.Count() <= i)
                {
                    SerializedProperty modelPropSP = modelPropListSP.GetArrayElementAtIndex(i);
                    TVMProp vmProp = new();
                    vmProp.InitializeSetting(modelPropSP);
                    vmPropList.Add(vmProp);
                }

                vmPropList[i].Recalculate();
            }
        }
    }
}