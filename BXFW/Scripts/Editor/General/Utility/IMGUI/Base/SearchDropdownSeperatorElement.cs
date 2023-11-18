using UnityEngine;

namespace BXFW.Tools.Editor
{
    /// <summary>
    /// A seperator element used to draw a line.
    /// <br>Note : This element supports no child management.</br>
    /// </summary>
    public class SearchDropdownSeperatorElement : SearchDropdownElement
    {
        public Color lineColor = Color.gray;
        public int lineHeight = 2;
        public int linePadding = 3;

        public override bool Interactable { get => false; set { } }

        public SearchDropdownSeperatorElement() : base("")
        { }
        public SearchDropdownSeperatorElement(Color lineColor) : base("")
        {
            this.lineColor = lineColor;
        }
        public SearchDropdownSeperatorElement(Color lineColor, int lineHeight, int linePadding) : base("")
        {
            this.lineColor = lineColor;
            this.lineHeight = lineHeight;
            this.linePadding = linePadding;
        }
        public SearchDropdownSeperatorElement(int lineHeight, int linePadding) : base("")
        {
            this.lineHeight = lineHeight;
            this.linePadding = linePadding;
        }

        public override float GetHeight(float viewWidth)
        {
            return lineHeight + linePadding;
        }
        public override void OnGUI(Rect position, ElementGUIDrawingState drawingState)
        {
            GUIAdditionals.DrawUILine(position, lineColor, lineHeight, linePadding);
        }

        public override void Add(SearchDropdownElement item)
        {
            throw new System.NotSupportedException("[SearchDropdownSeperatorElement::Add] Given operation is not supported for this type of element.");
        }
        public override bool Remove(SearchDropdownElement item)
        {
            throw new System.NotSupportedException("[SearchDropdownSeperatorElement::Remove] Given operation is not supported for this type of element.");
        }
        public override void Clear()
        {
            throw new System.NotSupportedException("[SearchDropdownSeperatorElement::Clear] Given operation is not supported for this type of element.");
        }

        // Uh, yes. Sorting this won't keep this on place.
        // So for this element, the sorting will be ignored.
        // TODO : Make elements ignored in their sorting?
        // Or preserve element order? (StableSort?)
        public override int CompareTo(SearchDropdownElement other)
        {
            return 0;
        }
    }
}
