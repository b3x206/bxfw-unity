using UnityEngine;
using UnityEngine.UI;

namespace BXFW.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class FGridLayout : LayoutGroup
    {
        public enum FitType
        {
            Uniform = 0,
            Width = 1,
            Height = 2,
            FixedRows = 3,
            FixedColumns = 4,
            FixedAll = 5 // Fixed tile with scaling (scaled correctly when the condition of child item amount is satisfied).
        }

        [Space]
        public FitType fitType;
        public int Rows;
        public int Columns;
        [Space]
        public Vector2 CellSize;
        public Vector2 Spacing;
        public bool fitX = true;
        public bool fitY = true;

        /// <summary>
        /// Returns the <see cref="RectTransform"/> that this layout group uses.
        /// </summary>
        public RectTransform RectTransform { get { return base.rectTransform; } }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
            {
                float sqrRt = Mathf.Sqrt(transform.childCount);
                Rows = Mathf.CeilToInt(sqrRt);
                Columns = Mathf.CeilToInt(sqrRt);
            }

            if (fitType == FitType.Height || fitType == FitType.FixedRows)
            {
                Rows = Mathf.CeilToInt(transform.childCount / (float)Rows);
            }
            if (fitType == FitType.Width || fitType == FitType.FixedColumns)
            {
                Columns = Mathf.CeilToInt(transform.childCount / (float)Columns);
            }

            float parentWidth = rectTransform.rect.width;
            float parentHeight = rectTransform.rect.height;

            float cellWidth = (parentWidth / (float)Columns) -
                ((Spacing.x / (float)Columns) * (Columns - 1)) -
                (padding.left / (float)Columns) - (padding.right / (float)Columns);

            float cellHeight = (parentHeight / (float)Rows) -
                ((Spacing.y / (float)Rows) * (Rows - 1)) -
                (padding.top / (float)Rows) - (padding.bottom / (float)Rows);

            CellSize.x = fitX ? cellWidth : CellSize.x;
            CellSize.y = fitY ? cellHeight : CellSize.y;

            // Invalid value clamping (this method is called OnValidate [hah that fits])
            if (Columns <= 0)
            {
                Columns = 1;
            }

            if (Rows <= 0)
            {
                Rows = 1;
            }

            int columnCount;
            int rowCount;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                columnCount = i % Columns;
                rowCount = i / Columns;

                RectTransform item = rectChildren[i];

                float xPos = (CellSize.x * columnCount) + (Spacing.x * columnCount) + padding.left;
                float yPos = (CellSize.y * rowCount) + (Spacing.y * rowCount) + padding.top;

                SetChildAlongAxis(item, 0, xPos, CellSize.x);
                SetChildAlongAxis(item, 1, yPos, CellSize.y);
            }
        }

        public override void CalculateLayoutInputVertical()
        { }
        public override void SetLayoutHorizontal()
        { }
        public override void SetLayoutVertical()
        { }
    }
}