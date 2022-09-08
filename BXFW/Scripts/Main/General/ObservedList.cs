using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace BXFW
{
    /// <summary>
    /// Contains virtual methods to override to create a observed list.
    /// <br>Note : This doesn't handle the events. Instead it calls a method, for other reasons.</br>
    /// </summary>
    [Serializable]
    public abstract class ObservedListBase<T> : IList<T>
    {
        [SerializeField] protected List<T> _list = new List<T>();

        protected ObservedListBase() { }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _list.Add(item);

            OnArrayAdded(_list.Count - 1, item);
            OnArrayUpdated();
        }

        public void Clear()
        {
            OnArrayRemovedRange(0, _list);
            _list.Clear();

            OnArrayUpdated();
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            OnArrayRemoved(_list.FindIndex(i => EqualityComparer<T>.Default.Equals(i, item)), item);
            bool output = _list.Remove(item);

            OnArrayUpdated();
            return output;
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);

            OnArrayAdded(index, item);
            OnArrayUpdated();
        }

        public void RemoveAt(int index)
        {
            OnArrayUpdated();

            _list.RemoveAt(index);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            _list.AddRange(collection);

            OnArrayAddedRange(_list.Count - (collection.Count() + 1), collection);
            OnArrayUpdated();
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            OnArrayRemovedRange(0, _list.FindAll(predicate));
            _list.RemoveAll(predicate);

            OnArrayUpdated();
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _list.InsertRange(index, collection);

            OnArrayAddedRange(index, collection);
            OnArrayUpdated();
        }

        public void RemoveRange(int index, int count)
        {
            OnArrayRemovedRange(index, _list.GetRange(index, count));

            _list.RemoveRange(index, count);
            OnArrayUpdated();
        }

        protected abstract void OnArrayUpdated([CallerMemberName] string a = "metho");
        protected abstract void OnArrayChanged(int index, T oldValue, T newValue);
        protected abstract void OnArrayAdded(int index, T added);
        protected abstract void OnArrayAddedRange(int index, IEnumerable<T> added);
        protected abstract void OnArrayRemoved(int index, T removed);
        protected abstract void OnArrayRemovedRange(int index, IEnumerable<T> removed);

        public T this[int index]
        {
            get { return _list[index]; }
            set
            {
                var oldValue = _list[index];
                _list[index] = value;

                OnArrayChanged(index, oldValue, value);
            }
        }

        /// <summary>
        /// Pretend that this list is a list.
        /// <br>No observation features are supported, use this ONLY as a read-only conversion.</br>
        /// </summary>
        public static explicit operator List<T>(ObservedListBase<T> list)
        {
            return list._list;
        }
    }

    /// <summary>
    /// List that has a delegate checking for changes.
    /// </summary>
    [Serializable]
    public class ObservedList<T> : ObservedListBase<T>
    {
        public delegate void ChangedDelegate(int index, T oldValue, T newValue);
        public delegate void AddRemoveDelegate(int index, T updatedElement);
        public delegate void AddRemoveRangeDelegate(int index, IEnumerable<T> updatedElement);

        /// <summary>
        /// Called when the list content is changed.
        /// <br>This is (usually) invoked by <see cref="this[int]"/>'s setter.</br>
        /// </summary>
        public event ChangedDelegate Changed;
        /// <summary>
        /// Called when the singular <see cref="ObservedListBase{T}.Add(T)"/> is called.
        /// <br>For multiple adds, use <see cref="OnAddRange"/> instead.</br>
        /// </summary>
        public event AddRemoveDelegate OnAdd;
        /// <summary>
        /// Called when multiple objects are added to this array.
        /// </summary>
        public event AddRemoveRangeDelegate OnAddRange;
        /// <summary>
        /// Called when multiple objects are removed from this array.
        /// </summary>
        public event AddRemoveRangeDelegate OnRemoveRange;
        /// <summary>
        /// Called when the singular <see cref="ObservedListBase{T}.Remove(T)"/> is called.
        /// <br>For multiple removals, use <see cref="OnRemoveRange"/> instead.</br>
        /// </summary>
        public event AddRemoveDelegate OnRemove;
        /// <summary>
        /// Called when anything happens to a list.
        /// <br>This includes adding and removing, unlike the <see cref="Changed"/> event.</br>
        /// </summary>
        public event Action Updated;

        public ObservedList() { }
        public ObservedList(List<T> list)
        {
            _list = list;
        }
        public ObservedList(IEnumerable<T> list)
        {
            _list = new List<T>(list);
        }

        protected override void OnArrayUpdated([CallerMemberName] string mems = "Method")
        {
            Debug.Log($"OnArrayUpdated from {mems}");
            Updated?.Invoke();
        }
        protected override void OnArrayChanged(int index, T oldValue, T newValue)
        {
            Changed?.Invoke(index, oldValue, newValue);
        }
        protected override void OnArrayAdded(int index, T added)
        {
            OnAdd?.Invoke(index, added);
        }
        protected override void OnArrayAddedRange(int index, IEnumerable<T> added)
        {
            OnAddRange?.Invoke(index, added);
        }
        protected override void OnArrayRemoved(int index, T removed)
        {
            OnRemove?.Invoke(index, removed);
        }
        protected override void OnArrayRemovedRange(int index, IEnumerable<T> removed)
        {
            OnRemoveRange?.Invoke(index, removed);
        }
    }

    /// <summary>
    /// List that also has a <see cref="UnityEvent"/> for editing the events in the inspector.
    /// <br>For other delegates of the <see cref="ObservedList{T}"/>, register to them from code. (Using the <see cref="unityUpdated"/>)</br>
    /// </summary>
    [Serializable]
    public class ObservedUnityEventList<T> : ObservedList<T>
    {
        public class ChangedDelegateUnityEvent : UnityEvent<int, T, T> { }

        public class UpdatedUnityEvent : UnityEvent<ObservedUnityEventList<T>> { }

        /// <summary>
        /// Called when the list content is changed.
        /// </summary>
        public ChangedDelegateUnityEvent unityChanged;
        /// <summary>
        /// Called when anything happens to a list.
        /// <br>Passes the class itself as an parameter.</br>
        /// </summary>
        public UpdatedUnityEvent unityUpdated;

        public ObservedUnityEventList() { }
        public ObservedUnityEventList(List<T> list)
        {
            _list = list;
        }
        public ObservedUnityEventList(IEnumerable<T> list)
        {
            _list = new List<T>(list);
        }

        protected override void OnArrayUpdated([CallerMemberName] string mems = "Method")
        {
            base.OnArrayUpdated();
            unityUpdated?.Invoke(this);
        }
        protected override void OnArrayChanged(int index, T oldValue, T newValue)
        {
            base.OnArrayChanged(index, oldValue, newValue);
            unityChanged?.Invoke(index, oldValue, newValue);
        }
    }
}
