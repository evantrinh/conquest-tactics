using UnityEngine;
using UnityEngine.UIElements;

namespace ContourUI
{
    // A reusable HP bar. Use it in any UXML as <c:HpBar hp="14" max-hp="20" />.
    [UxmlElement]
    public partial class HpBar : VisualElement
    {
        const float LowThreshold = 0.3f;

        readonly VisualElement fill = new VisualElement { name = "hpFill" };
        readonly Label text = new Label { name = "hpText" };
        int hpValue = 10;
        int maxHpValue = 10;

        [UxmlAttribute]
        public int hp
        {
            get => hpValue;
            set { hpValue = value; Refresh(); }
        }

        [UxmlAttribute]
        public int maxHp
        {
            get => maxHpValue;
            set { maxHpValue = value; Refresh(); }
        }

        public HpBar()
        {
            AddToClassList("hp-bar");

            var track = new VisualElement();
            track.AddToClassList("hp-bar__track");
            fill.AddToClassList("hp-bar__fill");
            track.Add(fill);
            text.AddToClassList("hp-bar__text");

            Add(track);
            Add(text);
            Refresh();
        }

        public void SetHp(int current, int max)
        {
            hpValue = current;
            maxHpValue = max;
            Refresh();
        }

        void Refresh()
        {
            float t = maxHpValue > 0 ? Mathf.Clamp01((float)hpValue / maxHpValue) : 0f;
            fill.style.width = Length.Percent(t * 100f);
            text.text = hpValue + " / " + maxHpValue;
            EnableInClassList("hp-bar--low", t <= LowThreshold);
        }
    }
}
