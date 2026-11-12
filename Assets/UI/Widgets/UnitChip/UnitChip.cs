using UnityEngine.UIElements;

namespace ContourUI
{
    // A small roster entry for one unit: name, HP bar and a short state.
    [UxmlElement]
    public partial class UnitChip : VisualElement
    {
        readonly Label nameLabel = new Label();
        readonly HpBar hpBar = new HpBar();
        readonly Label stateLabel = new Label();

        public UnitChip()
        {
            AddToClassList("unit-chip");
            nameLabel.AddToClassList("unit-chip__name");
            stateLabel.AddToClassList("unit-chip__state");
            Add(nameLabel);
            Add(hpBar);
            Add(stateLabel);
        }

        public void Show(string unitName, int hp, int maxHp, string state)
        {
            nameLabel.text = unitName;
            hpBar.SetHp(hp, maxHp);
            stateLabel.text = state;
        }

        public void SetSelected(bool selected) => EnableInClassList("unit-chip--selected", selected);
    }
}
