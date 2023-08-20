using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BXFW.UI
{
    /// <summary>
    /// Manages multiple objects in an interface nicely.
    /// </summary>
    /// <typeparam name="TElement">Element Component type.</typeparam>
    [RequireComponent(typeof(RectTransform))]
    public abstract class MultiUIManager<TElement> : MultiUIManagerBase, IEnumerable<TElement>, IEquatable<IEnumerable<TElement>>
        where TElement : Component
    {
        /// <summary>
        /// Called when an element is created.
        /// <br><c>Param1 : <see cref="int"/> = index</c>, <c>Param2 : <typeparamref name="TElement"/> = created.</c></br>
        /// </summary>
        [Serializable]
        public class OnCreateEvent : UnityEvent<int, TElement> { }

        /// <summary>
        /// Event type for events that involve the <typeparamref name="TElement"/> only.
        /// </summary>
        [Serializable]
        public class ElementEvent : UnityEvent<TElement> { }

        /// <summary>
        /// Called when an element is created through <see cref="CreateUIElement"/>.
        /// </summary>
        public OnCreateEvent OnCreateElementEvent;

        /// <summary>
        /// List of the contained elements.
        /// <br>When you override <see cref="GenerateElements"/>, don't forget to add to this array.</br>
        /// </summary>
        [SerializeField] protected List<TElement> uiElements = new List<TElement>();
        /// <summary>
        /// List of the contained elements, in a read-only form.
        /// <br>Even though the '<see cref="ElementCount"/>' is set to zero, this will always contain 1 object.</br>
        /// </summary>
        public IReadOnlyList<TElement> Elements
        {
            get
            {
                return uiElements;
            }
        }
        /// <summary>
        /// Index accessor for multi ui.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <returns>Element on the given index.</returns>
        public TElement this[int index]
        {
            get
            {
                return uiElements[index];
            }
        }

        protected MultiUIManager()
        { }

        /// <summary>
        /// An override that should be done to create a element generically.
        /// <br>Removal is handled by the base script.</br>
        /// </summary>
        /// <returns>The created element.</returns>
        /// <param name="referenceElement">The reference element. This will be null if there needs to be a new element created from scratch.</param>
        protected abstract TElement OnCreateElement(TElement referenceElement);

        /// <summary>
        /// Creates an element.
        /// <br>This is the method that is normally called, but invokes the events to both to the unity event and the abstract base.</br>
        /// <br>
        /// This method sets the transform parent of the generated element to this
        /// <see cref="Component.transform"/> and sets it's <see cref="Transform.localScale"/> to <see cref="Vector3.one"/>.
        /// </br>
        /// </summary>
        /// <param name="useReferenceElement">
        /// Whether to use a reference element that already exists.
        /// <br>To change the element that is being referred, use the </br>
        /// </param>
        public TElement CreateUIElement(bool useReferenceElement = true)
        {
            int createIndex = uiElements.Count;
            TElement element = OnCreateElement(useReferenceElement ? uiElements[ReferenceElementIndex] : null);
            
            // Add to list + events
            uiElements.Add(element);
            OnCreateElementEvent?.Invoke(createIndex, element);
            // Transform stuff (because the child will be scaled weirdly)
            element.transform.SetParent(transform);
            element.transform.localScale = Vector3.one;
            // Check name and truncate the '(Clone)' from them
            if (TruncateCloneNameOnCreate)
            {
                string elemName = element.gameObject.name;
                int idxOfClone = elemName.IndexOf("(Clone)");
                if (idxOfClone != -1)
                {
                    element.gameObject.name = elemName.Remove(elemName.IndexOf("(Clone)"));
                }
            }

            return element;
        }

        /// <summary>
        /// Generate the 'uiElements' array.
        /// <br>This generically spawns a list of <typeparamref name="TElement"/>.</br>
        /// </summary>
        public override void GenerateElements()
        {
            // The generation should always retain the first element as an inactive one.
            // Removal loop
            while (uiElements.Count > ElementCount)
            {
                if (uiElements.Count <= 1)
                {
                    if (uiElements.Count < 0 || uiElements[0] == null)
                    {
                        // Null array and or null element
                        // Stop destruction after clearing the array
                        CleanupElementsList();
                        break; // Going beyond this point will cause exceptions, fail silently
                    }

                    // Disable the last object and stop the destruction
                    uiElements[0].gameObject.SetActive(false);
                    break;
                }

                if (uiElements[uiElements.Count - 1] != null)
                {
                    // We need to use DestroyImmediate here as there's no need for the reference
                    // Otherwise the script gets stuck at an infinite loop and dies.
                    GameObject destroyObject = uiElements[uiElements.Count - 1].gameObject;

                    // We record the 'Undo' on the editor script anyways
                    DestroyImmediate(destroyObject);
                }
                else // Null elements exist on the list, cleanup
                {
                    CleanupElementsList();
                    continue;
                }

                CleanupElementsList();
            }
            // Add loop
            while (uiElements.Count < ElementCount)
            {
                // Set first element to be activated
                if (uiElements.Count == 1)
                {
                    if (uiElements[0] == null)
                    {
                        CleanupElementsList();
                        continue;
                    }

                    uiElements[0].gameObject.SetActive(true);
                }

                CreateUIElement();
            }
        }
        /// <summary>
        /// Updates the appareance of the elements.
        /// <br>An optional thing for stuff that needs appearance changing.</br>
        /// </summary>
        public override void UpdateElementsAppearance()
        { }
        /// <summary>
        /// Clears all of the elements and creates a resetted manager.
        /// <br>
        /// The default object will also be cleared, to just remove all elements
        /// (and not reset the ref object) just set the <see cref="ElementCount"/> to zero.
        /// </br>
        /// </summary>
        public override void ResetElements()
        {
            ElementCount = 0;
            // Destroy first object
            if (uiElements.Count >= 1 && uiElements[0] != null)
            {
                GameObject destroyObject = uiElements[0].gameObject;
#if UNITY_EDITOR
                // Playing check
                if (Application.isPlaying)
                    Destroy(destroyObject);
                else
                    DestroyImmediate(destroyObject);
#else
                // No need for that on built games
                Destroy(destroyObject);
#endif
            }

            uiElements.Clear();
            // Since the 'ElementCount' is 0, set the object to be disabled by default.
            CreateUIElement(false).gameObject.SetActive(false);
        }

        /// <summary>
        /// Removes nulls from the list <see cref="uiElements"/>.
        /// </summary>
        protected void CleanupElementsList()
        {
            uiElements.RemoveAll(elem => elem == null);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return uiElements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return uiElements.GetEnumerator();
        }

        public bool Equals(IEnumerable<TElement> other)
        {
            return Enumerable.SequenceEqual(uiElements, other);
        }
    }
}
