using UnityEngine;
using UnityEngine.UIElements;

namespace ContourUI
{
    // A ring cut into segments, some filled, with the count in the middle.
    // Use it for skill points: <c:SkillRing total="6" filled="3" />.
    [UxmlElement]
    public partial class SkillRing : VisualElement
    {
        const float LineWidth = 5f;
        const float GapDegrees = 10f;

        readonly Label count = new Label();
        int totalValue = 6;
        int filledValue = 3;

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

        [UxmlAttribute] public Color filledColor { get; set; } = new Color(0.65f, 0.55f, 0.98f);
        [UxmlAttribute] public Color emptyColor { get; set; } = new Color(0.27f, 0.3f, 0.4f);

        public SkillRing()
        {
            AddToClassList("skill-ring");
            count.AddToClassList("skill-ring__count");
            Add(count);
            generateVisualContent += Draw;
            Refresh();
        }

        public void SetPoints(int filledCount, int totalCount)
        {
            filledValue = filledCount;
            totalValue = totalCount;
            Refresh();
        }

        void Refresh()
        {
            count.text = filledValue.ToString();
            MarkDirtyRepaint();
        }

        void Draw(MeshGenerationContext ctx)
        {
            float w = contentRect.width;
            float h = contentRect.height;
            if (w < 1f || h < 1f || totalValue < 1) return;

            var painter = ctx.painter2D;
            var center = new Vector2(w / 2f, h / 2f);
            float radius = Mathf.Min(w, h) / 2f - LineWidth / 2f;
            float step = 360f / totalValue;

            painter.lineWidth = LineWidth;
            for (int i = 0; i < totalValue; i++)
            {
                float start = -90f + i * step + GapDegrees / 2f;
                painter.strokeColor = i < filledValue ? filledColor : emptyColor;
                painter.BeginPath();
                painter.Arc(center, radius, Angle.Degrees(start), Angle.Degrees(start + step - GapDegrees), ArcDirection.Clockwise);
                painter.Stroke();
            }
        }
    }
}
