using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace BXFW
{
    /// <summary>
    /// Allows for listening in to the parent <see cref="CanvasGroup"/>'s and the itself's interactability.
    /// <br>This is just how <see cref="Selectable"/>'s interactability occurs without the <see cref="Selectable"/>'s bloat.</br>
    /// </summary>
    public abstract class InteractableUIBehaviour : UIBehaviour
    {
        [SerializeField, InspectorLine(LineColor.Gray), Tooltip("Can the UI element be interacted with?")]
        private bool interactable = true;
        /// <summary>
        /// Defines whether if this object is interactable with.
        /// <br/>
        /// <br>Warning : Don't set this from <see cref="OnInteractableStateChanged"/>, 
        /// it will cause a infinite recursion if there's no break condition.</br>
        /// </summary>
        public bool Interactable
        {
            get { return IsInteractable(); }
            set
            {
                interactable = value;
                OnInteractableStateChanged();
            }
        }
        /// <summary>
        /// Runtime variable for whether if the parent <see cref="CanvasGroup"/> is allowed to be interacted with.
        /// </summary>
        protected bool CanvasGroupsAllowInteraction { get; private set; } = true;
        /// <summary>
        /// Whether if the UI element is allowed to be interactable.
        /// </summary>
        protected virtual bool IsInteractable()
        {
            if (CanvasGroupsAllowInteraction)
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
            bool parentGroupsAllowInteraction = true;
            Transform t = transform;

            while (t != null)
            {
                t.GetComponents(canvasGroupCache);
                bool shouldBreak = false;

                for (int i = 0; i < canvasGroupCache.Count; i++)
                {
                    if (!canvasGroupCache[i].interactable)
                    {
                        parentGroupsAllowInteraction = false;
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
            if (parentGroupsAllowInteraction != CanvasGroupsAllowInteraction)
            {
                CanvasGroupsAllowInteraction = parentGroupsAllowInteraction;
                OnInteractableStateChanged();
            }
        }

        /// <summary>
        /// Called when the interactability of this UI element changed.
        /// <br>Use the <see cref="Interactable"/> bool to check for the state.</br>
        /// <br>The base behaviour does nothing.</br>
        /// </summary>
        protected virtual void OnInteractableStateChanged()
        { }
    }
}
