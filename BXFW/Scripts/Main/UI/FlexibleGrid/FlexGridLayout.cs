using UnityEngine;
using UnityEngine.UI;

namespace BXFW.UI
{
    /// <summary>
    /// A grid <see cref="LayoutGroup"/> that flexes it's elements to fit accordingly.
    /// <br>Unlike <see cref="GridLayoutGroup"/>, this acts inline with the other unity ugui layout group elements.</br>
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class FlexGridLayout : LayoutGroup
    {
        /// <summary>
        /// Fitting type for the <see cref="FlexGridLayout"/>.
        /// </summary>
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
        public int rows;
        public int columns;
        [Space]
        public Vector2 cellSize;
        public Vector2 spacing;
        public bool fitX = true;
        public bool fitY = true;

        /// <summary>
        /// Returns the <see cref="RectTransform"/> that this layout group uses.
        /// </summary>
        public RectTransform RectTransform { get { return rectTransform; } }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            // Calculations divide by zero when there's no transform child
            // So always assume that there's one single transform child
            // Because 'NaN' is definitely one of the floating point values of all time.
            if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
            {
                float sqrtChildren = Mathf.Sqrt(Mathf.Max(transform.childCount, 1));
                rows = Mathf.CeilToInt(sqrtChildren);
                columns = Mathf.CeilToInt(sqrtChildren);
            }

            if (fitType == FitType.Height || fitType == FitType.FixedRows)
            {
                rows = Mathf.CeilToInt(Mathf.Max(transform.childCount, 1) / (float)rows);
            }
            if (fitType == FitType.Width || fitType == FitType.FixedColumns)
            {
                columns = Mathf.CeilToInt(Mathf.Max(transform.childCount, 1) / (float)columns);
            }

            float parentWidth = rectTransform.rect.width;
            float parentHeight = rectTransform.rect.height;

            float cellWidth = (parentWidth / columns) -
                ((spacing.x / columns) * (columns - 1)) -
                (padding.left / (float)columns) - (padding.right / (float)columns);

            float cellHeight = (parentHeight / rows) -
                ((spacing.y / rows) * (rows - 1)) -
                (padding.top / (float)rows) - (padding.bottom / (float)rows);

            cellSize.x = fitX ? cellWidth : cellSize.x;
            cellSize.y = fitY ? cellHeight : cellSize.y;

            // Invalid value clamping (this method is called OnValidate [hah that fits])
            if (columns <= 0)
            {
                columns = 1;
            }

            if (rows <= 0)
            {
                rows = 1;
            }

            int columnCount;
            int rowCount;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                columnCount = i % columns;
                rowCount = i / columns;

                RectTransform item = rectChildren[i];

                float xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
                float yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

                SetChildAlongAxis(item, 0, xPos, cellSize.x);
                SetChildAlongAxis(item, 1, yPos, cellSize.y);
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
