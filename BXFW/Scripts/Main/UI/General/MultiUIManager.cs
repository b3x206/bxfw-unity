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
        /// Event type for a indexed element.
        /// <br><c>Param1 : <see cref="int"/> = index</c>, <c>Param2 : <typeparamref name="TElement"/> = created/reference.</c></br>
        /// </summary>
        [Serializable]
        public class IndexedElementEvent : UnityEvent<int, TElement> { }

        /// <summary>
        /// A unity event that takes a integer.
        /// </summary>
        [Serializable]
        public class IndexEvent : UnityEvent<int> { }

        /// <summary>
        /// Event type for events that involve the <typeparamref name="TElement"/> only.
        /// </summary>
        [Serializable]
        public class ElementEvent : UnityEvent<TElement> { }

        /// <summary>
        /// Called when an element is created through <see cref="InternalCreateUIElement"/>.
        /// </summary>
        public IndexedElementEvent onCreateElementEvent;

        /// <summary>
        /// List of the contained elements.
        /// <br>When you override <see cref="GenerateElements"/>, don't forget to add to this array.</br>
        /// </summary>
        [SerializeField] protected List<TElement> m_Elements = new List<TElement>();
        /// <summary>
        /// List of the contained elements, in a read-only form.
        /// <br>Even though the '<see cref="ElementCount"/>' is set to zero, this will always contain 1 object.</br>
        /// </summary>
        public IReadOnlyList<TElement> Elements
        {
            get
            {
                return m_Elements;
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
                return m_Elements[index];
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
        /// <br>To change the element that is being referred, use the <see cref="MultiUIManagerBase.ReferenceElementIndex"/>.</br>
        /// </param>
        /// <remarks>
        /// <b>NOTE : </b>This method DOES NOT increment <see cref="MultiUIManagerBase.m_ElementCount"/>
        /// (m_ElementCount is treated as a target value!)
        /// <br>It is meant to be used as an <b>helper</b> method to allow creation + management of values.</br>
        /// <br>For an user creation method, use <see cref="CreateUIElement(bool)"/>.</br>
        /// <br>To remove UI elements decrement <see cref="MultiUIManagerBase.ElementCount"/> by any amount you want (it is clamped anyways)</br>
        /// <br>This method does only modify the <see cref="m_Elements"/> list and it's count.</br>
        /// </remarks>
        protected TElement InternalCreateUIElement(bool useReferenceElement = true)
        {
            int createIndex = m_Elements.Count;
            if (useReferenceElement)
                useReferenceElement = m_Elements.Count > 0; // Check eligibility of using an reference element

            TElement element = OnCreateElement(useReferenceElement ? m_Elements[ReferenceElementIndex] : null);

            // Add to list + events
            m_Elements.Add(element);
            onCreateElementEvent?.Invoke(createIndex, element);
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
                    element.gameObject.name = elemName.Remove(idxOfClone);
                }
            }

            return element;
        }

        /// <summary>
        /// <inheritdoc cref="InternalCreateUIElement(bool)"/>
        /// <br/>
        /// <br>Calling this directly will increment 'm_ElementCount' by 1.</br>
        /// </summary>
        /// <param name="useReferenceElement">
        /// <inheritdoc cref="InternalCreateUIElement(bool)"/>
        /// </param>
        public TElement CreateUIElement(bool useReferenceElement = true)
        {
            InternalCreateUIElement(useReferenceElement);
            // Increment element count after just in case if an exception is thrown
            m_ElementCount++;
            return m_Elements[m_Elements.Count - 1];
        }

        /// <summary>
        /// A destroy immediate wrapper that respects editor operations and other things.
        /// <br>In standalone runtimes, this just does <see cref="UnityEngine.Object.DestroyImmediate(UnityEngine.Object)"/>.</br>
        /// <br>Unless the object needs to be immediately destroyed during generation, use <see cref="UnityEngine.Object.Destroy"/> instead.</br>
        /// </summary>
        /// <param name="target">Target to immediately destroy.</param>
        protected void ManagerDestroyImmediate(UnityEngine.Object target)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Destroy forever (no Undo stack saving)
                DestroyImmediate(target);
            }
            else
            {
                // Editor destroy (usable if the group was being recorded and was collapsed)
                UnityEditor.Undo.DestroyObjectImmediate(target);
            }
#else
            DestroyImmediate(target);
#endif
        }

        /// <summary>
        /// Generate the 'uiElements' array.
        /// <br>This generically spawns a list of <typeparamref name="TElement"/>.</br>
        /// </summary>
        public override void GenerateElements()
        {
            // The generation should always retain the first element as an inactive one.
            // Removal loop
            while (m_Elements.Count > ElementCount)
            {
                if (m_Elements.Count <= 1)
                {
                    if (m_Elements.Count < 0 || m_Elements[0] == null)
                    {
                        // Null array and or null element
                        // Stop destruction after clearing the array
                        CleanupElementsList();
                        break; // Going beyond this point will cause exceptions, fail silently
                    }

                    // Disable the last object and stop the destruction
                    m_Elements[0].gameObject.SetActive(false);
                    break;
                }

                if (m_Elements[m_Elements.Count - 1] != null)
                {
                    // We need to use DestroyImmediate here as there's no need for the reference
                    // Otherwise the script gets stuck at an infinite loop and dies.
                    GameObject destroyObject = m_Elements[m_Elements.Count - 1].gameObject;

                    // Uhh, so using 'DestroyImmediate' that is not on 'Undo' is a bad idea (in editor)
                    // It crashed unity so, yeah -_-
                    // When this gets called from unity editor we can choose which destruction we want to choose anyways
                    // So yeah.
                    ManagerDestroyImmediate(destroyObject);
                }
                else // Null elements exist on the list, cleanup
                {
                    CleanupElementsList();
                    continue;
                }

                CleanupElementsList();
            }
            // Add loop
            while (m_Elements.Count < ElementCount)
            {
                // Set first element to be activated
                if (m_Elements.Count == 1)
                {
                    if (m_Elements[0] == null)
                    {
                        CleanupElementsList();
                        continue;
                    }

                    m_Elements[0].gameObject.SetActive(true);
                }

                InternalCreateUIElement();
            }
            // Check if the element count is 1, enable the 'uiElements[0]' from here
            // This is because the 'Add loop' doesn't work when the previous 'ElementCount' is zero.
            // (first element is always in 'uiElements' unless you call ResetElements)
            if (m_Elements.Count == 1)
            {
                if (m_Elements[0] == null)
                {
                    CleanupElementsList();
                    InternalCreateUIElement();
                }

                m_Elements[0].gameObject.SetActive(ElementCount > 0);
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
        public override void ResetElements(bool clearChildTransform = false)
        {
            ElementCount = 0;
            // Destroy first object
            if (m_Elements.Count >= 1 && m_Elements[0] != null)
            {
                GameObject destroyObject = m_Elements[0].gameObject;
#if UNITY_EDITOR
                // Playing check
                if (Application.isPlaying)
                    Destroy(destroyObject);
                else
                    ManagerDestroyImmediate(destroyObject);
#else
                // No need for that on built games
                Destroy(destroyObject);
#endif
            }

            // Clear the child transform
            if (clearChildTransform)
            {
                while (transform.childCount > 0)
                {
                    GameObject destroyObject = transform.GetChild(0).gameObject;
#if UNITY_EDITOR
                    if (Application.isPlaying)
                        Destroy(destroyObject);
                    else
                        ManagerDestroyImmediate(destroyObject);
#else
                    Destroy(destroyObject);
#endif
                }
            }

            m_Elements.Clear();
            // Since the 'ElementCount' is 0, set the object to be disabled by default.
            InternalCreateUIElement(false).gameObject.SetActive(false);
        }

        /// <summary>
        /// Removes nulls from the list <see cref="m_Elements"/>.
        /// </summary>
        protected void CleanupElementsList()
        {
            m_Elements.RemoveAll(elem => elem == null);
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return m_Elements.GetEnumerator();
        }
        public override IEnumerable<Component> IterableElements()
        {
            foreach (TElement elem in m_Elements)
            {
                yield return elem;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Elements.GetEnumerator();
        }

        public bool Equals(IEnumerable<TElement> other)
        {
            return Enumerable.SequenceEqual(m_Elements, other);
        }
    }
}
