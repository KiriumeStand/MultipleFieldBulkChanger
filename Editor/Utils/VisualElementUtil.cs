using UnityEngine.UIElements;

namespace io.github.kiriumestand.multiplefieldbulkchanger.editor
{
    internal static class VisualElementUtil
    {
        internal static void SetDisplays(params (VisualElement element, bool show)[] items)
        {
            foreach (var (element, show) in items)
            {
                SetDisplay(element, show);
            }
        }

        internal static void SetDisplay(VisualElement element, bool show)
        {
            element.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }

        internal static void TextBaseFieldSetReadOnlys(params (VisualElement element, bool enabled)[] items)
        {
            foreach (var (element, enabled) in items)
            {
                switch (element)
                {
                    case DoubleField doubleField:
                        doubleField.isReadOnly = enabled;
                        break;
                    case TextField textField:
                        textField.isReadOnly = enabled;
                        break;
                    case Vector2Field:
                    case Vector3Field:
                    case Vector4Field:
                    case BoundsField:
                        var textFields = element.Query<FloatField>().ToList();
                        foreach (var textField in textFields)
                        {
                            textField.isReadOnly = enabled;
                        }
                        break;
                }
            }
        }
    }
}
