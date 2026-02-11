#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEditor.IMGUI.Controls;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public abstract class ExpansionAdvancedDropdownItem : AdvancedDropdownItem
    {
        public virtual string FullPath { get; } = "";

        public virtual int Id { get => FullPath.GetHashCode(); }

        public ExpansionAdvancedDropdownItem(string name) : base(name) { }

        public override int GetHashCode() => FullPath.GetHashCode();

        public new void SortChildren(Comparison<AdvancedDropdownItem> comparer, bool recursive = false)
        {
            base.SortChildren(comparer, recursive);
        }
    }
}
#endif