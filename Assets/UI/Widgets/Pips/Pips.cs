using UnityEngine.UIElements;

namespace ContourUI
{
    // A row of small boxes, some filled. Use it for action points or skill points:
    // <c:Pips total="6" filled="3" />.
    [UxmlElement]
    public partial class Pips : VisualElement
    {
        int totalValue = 3;
        int filledValue = 2;

        [UxmlAttribute]
        public int total
        {
            get => totalValue;
            set { totalValue = value; Refresh(); }
        }

        [UxmlAttribute]
        public int filled
        {
            get => filledValue;
            set { filledValue = value; Refresh(); }
        }

        public Pips()
        {
            AddToClassList("pips");
            Refresh();
        }

        public void SetPips(int filledCount, int totalCount)
        {
            filledValue = filledCount;
            totalValue = totalCount;
            Refresh();
        }

        void Refresh()
        {
            while (childCount < totalValue)
            {
                var pip = new VisualElement();
                pip.AddToClassList("pips__pip");
                Add(pip);
            }
            while (childCount > totalValue) RemoveAt(childCount - 1);

            for (int i = 0; i < childCount; i++)
                this[i].EnableInClassList("pips__pip--filled", i < filledValue);
        }
    }
}
