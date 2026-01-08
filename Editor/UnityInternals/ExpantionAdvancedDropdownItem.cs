using System;
using System.Collections;
using UnityEditor.IMGUI.Controls;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public abstract class ExpantionAdvancedDropdownItem : AdvancedDropdownItem
    {
        public virtual string FullPath { get; } = "";

        public virtual int Id { get => FullPath.GetHashCode(); }

        public ExpantionAdvancedDropdownItem(string name) : base(name) { }

        public override int GetHashCode() => FullPath.GetHashCode();

        public new void SortChildren(Comparison<AdvancedDropdownItem> comparer, bool recursive = false)
        {
            base.SortChildren(comparer, recursive);
        }
    }
}