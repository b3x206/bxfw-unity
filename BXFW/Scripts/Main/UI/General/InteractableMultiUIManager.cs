using UnityEngine;
using System.Collections.Generic;

namespace BXFW.UI
{
    /// <summary>
    /// An MultiUIManager parent object that has interactability values.
    /// <br>This class is for a parent that has interactibility. The child elements may not have interactibility.</br>
    /// <br>
    /// For ensuring the child element interactability, make a custom class
    /// where the <typeparamref name="TElement"/> is constrainted to <see cref="UnityEngine.UI.Selectable"/> (or to your custom selectable class).
    /// </br>
    /// </summary>
    public abstract class InteractableMultiUIManager<TElement> : MultiUIManager<TElement>
        where TElement : Component
    {
        [SerializeField, InspectorLine(LineColor.Gray), Tooltip("Can the UI element be interacted with?")]
        private bool interactable = true;
        /// <summary>
        /// Defines whether if this object is interactable with.
        /// <br/>
        /// <br>Warning : Don't set this from <see cref="MultiUIManagerBase.UpdateElementsAppearance"/>, 
        /// it will cause a infinite recursion if there's no break condition.</br>
        /// </summary>
        public bool Interactable
        {
            get { return IsInteractable(); }
            set
            {
                interactable = value;
                UpdateElementsAppearance();
            }
        }
        /// <summary>
        /// Runtime variable for whether if the object is allowed to be interacted with.
        /// </summary>
        private bool groupsAllowInteraction = true;
        /// <summary>
        /// Whether if the UI element is allowed to be interactable.
        /// </summary>
        protected internal virtual bool IsInteractable()
        {
            if (groupsAllowInteraction)
            {
                return interactable;
            }

            return false;
        }
        private readonly List<CanvasGroup> canvasGroupCache = new List<CanvasGroup>(8);
        protected override void OnCanvasGroupChanged()
        {
            // This event is part of Selectable (but i adapted it to this script).
            // Search for 'CanvasGroup' behaviours & apply preferences to this object.
            // 1: Search for parenting transforms that contain 'CanvasGroup'
            // 2: Keep them in 'GetComponents' list cache (no alloc method)
            // 3: Update the interaction state accordingly to the parent one.
            bool groupAllowInteraction = true;
            Transform t = transform;

            while (t != null)
            {
                t.GetComponents(canvasGroupCache);
                bool shouldBreak = false;

                for (int i = 0; i < canvasGroupCache.Count; i++)
                {
                    if (!canvasGroupCache[i].interactable)
                    {
                        groupAllowInteraction = false;
                        shouldBreak = true;
                    }
                    if (canvasGroupCache[i].ignoreParentGroups)
                    {
                        shouldBreak = true;
                    }
                }
                if (shouldBreak)
                {
                    break;
                }

                t = t.parent;
            }

            // Check if the parent canvases has changed
            if (groupAllowInteraction != groupsAllowInteraction)
            {
                groupsAllowInteraction = groupAllowInteraction;
                UpdateElementsAppearance();
            }
        }

        protected override abstract TElement OnCreateElement(TElement referenceElement);
    }
}
