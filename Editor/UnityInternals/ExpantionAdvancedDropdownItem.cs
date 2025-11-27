using UnityEditor.IMGUI.Controls;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    public abstract class ExpantionAdvancedDropdownItem : AdvancedDropdownItem
    {
        public string FullName { get; } = "";

        public ExpantionAdvancedDropdownItem(string name, string fullName) : base(name)
        {
            FullName = fullName;
            UpdateId();
        }

        public void UpdateId() => id = GetHashCode();

        public override int GetHashCode() => FullName.GetHashCode();
    }
}