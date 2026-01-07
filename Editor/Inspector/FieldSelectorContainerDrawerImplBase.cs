using io.github.kiriumestand.multiplefieldbulkchanger.runtime;
using UnityEditor;
using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public abstract class FieldSelectorContainerDrawerImplBase<TDrawer> : ExpansionPropertyDrawerImpl<TDrawer> where TDrawer : ExpansionPropertyDrawer
    {
    }
}