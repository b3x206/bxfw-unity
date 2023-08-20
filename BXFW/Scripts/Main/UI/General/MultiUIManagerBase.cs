using UnityEngine;
using UnityEngine.EventSystems;

namespace BXFW.UI
{
    /// This class solely exists for making an editor for the <see cref="MultiUIManager{TElement}"/>.
    /// (it could also have been an interface but i wanted to limit the type
    /// that the MultiUIManager will inherit + custom property drawers don't like interfaces)
    /// <summary>
    /// Contains the base variables for multi UI management.
    /// </summary>
    public abstract class MultiUIManagerBase : UIBehaviour
    {
        /// <summary>
        /// The element count that does not call <see cref="GenerateElements"/> when you set it.
        /// <br>Setting this value does not do <see cref="MaxElementCount"/> checking.</br>
        /// </summary>
        [SerializeField, ReadOnlyView] protected int m_ElementCount = 0;
        /// <summary>
        /// The element count of this MultiUIManager.
        /// <br>Setting this will spawn more objects.</br>
        /// </summary>
        public int ElementCount
        {
            get
            {
                return m_ElementCount;
            }
            set
            {
                m_ElementCount = Mathf.Clamp(value, 0, MaxElementCount);
                GenerateElements();
            }
        }
        /// <summary>
        /// Maximum amount of elements that can be spawned in.
        /// <br>The minimum value that 'ElementCount' can be set is 0.</br>
        /// </summary>
        protected virtual int MaxElementCount => short.MaxValue;

        /// <summary>
        /// <inheritdoc cref="ReferenceElementIndex"/>
        /// </summary>
        [SerializeField, ReadOnlyView] protected int m_ReferenceElementIndex = 0;
        /// <summary>
        /// The <b>index</b> of the element that is being used for <see cref="MultiUIManager{TElement}.CreateUIElement(bool)"/>'s useReferenceElement.
        /// </summary>
        public int ReferenceElementIndex
        {
            get
            {
                return m_ReferenceElementIndex;
            }
            set
            {
                m_ReferenceElementIndex = Mathf.Clamp(value, 0, ElementCount - 1);
            }
        }
        /// <summary>
        /// Truncates the '(Clone)' tag after <see cref="MultiUIManager{TElement}.CreateUIElement(bool)"/> is called.
        /// </summary>
        public bool TruncateCloneNameOnCreate = true;

        /// <summary>
        /// Generate the 'uiElements' array on this method.
        /// </summary>
        public abstract void GenerateElements();
        /// <summary>
        /// Updates the appareance of the elements.
        /// <br>An optional thing for stuff that needs appearance changing.</br>
        /// </summary>
        public abstract void UpdateElementsAppearance();
        /// <summary>
        /// Clears all of the elements and creates a resetted manager.
        /// <br>
        /// The default object will also be cleared, to just remove all elements
        /// (and not reset the ref object) just set the <see cref="ElementCount"/> to zero.
        /// </br>
        /// </summary>
        public abstract void ResetElements();

#if UNITY_EDITOR
        /// <summary>
        /// <c>[ EDITOR ONLY ] : </c> Calls <see cref="ResetElements"/> when the editor resetting is initiated.
        /// <br>If you are gonna override this, use the '#if UNITY_EDITOR' statement for the whole method to avoid compilation errors.</br>
        /// <br>This is because the <see cref="UIBehaviour"/> unity base class does the virtual declaration on the same '#if UNITY_EDITOR' statement.</br>
        /// </summary>
        protected override void Reset()
        {
            base.Reset();
            ResetElements();
        }
#endif
    }
}
