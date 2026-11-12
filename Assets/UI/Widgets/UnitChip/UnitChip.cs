using UnityEngine.UIElements;

namespace ContourUI
{
    // A small roster card for one unit: a portrait box, a thin HP bar and a dot while it can still act.
    [UxmlElement]
    public partial class UnitChip : VisualElement
    {
        readonly Label portrait = new Label();
        readonly HpBar hpBar = new HpBar();
        readonly VisualElement dot = new VisualElement();

        public UnitChip()
        {
            AddToClassList("unit-chip");
            portrait.AddToClassList("unit-chip__portrait");
            dot.AddToClassList("unit-chip__dot");
            Add(portrait);
            Add(hpBar);
            Add(dot);
        }

        public void Show(string unitName, int hp, int maxHp, bool ready)
        {
            portrait.text = string.IsNullOrEmpty(unitName) ? "" : unitName.Substring(0, 1);
            hpBar.SetHp(hp, maxHp);
            dot.EnableInClassList("hidden", !ready);
        }

        public void SetSelected(bool selected) => EnableInClassList("unit-chip--selected", selected);
    }
}
