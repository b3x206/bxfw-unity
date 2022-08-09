using System;
using System.Collections;
using System.Collections.Generic;
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

            OnArrayUpdated();
        }

        public void Clear()
        {
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

            OnArrayUpdated();
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);

            OnArrayUpdated();
        }

        public void AddRange(IEnumerable<T> collection)
        {
            _list.AddRange(collection);

            OnArrayUpdated();
        }

        public void RemoveAll(Predicate<T> predicate)
        {
            _list.RemoveAll(predicate);

            OnArrayUpdated();
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _list.InsertRange(index, collection);

            OnArrayUpdated();
        }

        public void RemoveRange(int index, int count)
        {
            _list.RemoveRange(index, count);

            OnArrayUpdated();
        }

        public abstract void OnArrayUpdated();
        public abstract void OnArrayChanged(int index, T oldValue, T newValue);

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

        /// <summary>
        /// Called when the list content is changed.
        /// <br>This is (usually) invoked by <see cref="this[int]"/>'s setter.</br>
        /// </summary>
        public event ChangedDelegate Changed;
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

        public override void OnArrayUpdated()
        {
            Updated?.Invoke();
        }
        public override void OnArrayChanged(int index, T oldValue, T newValue)
        {
            Changed?.Invoke(index, oldValue, newValue);
        }
    }

    /// <summary>
    /// List that also has a <see cref="UnityEvent"/> for editing the events in the inspector.
    /// </summary>
    [Serializable]
    public class ObservedUnityEventList<T> : ObservedList<T>
    {
        public class ChangedDelegateUnityEvent : UnityEvent<int, T, T> { }

        /// <summary>
        /// Called when the list content is changed.
        /// </summary>
        public ChangedDelegateUnityEvent unityChanged;
        /// <summary>
        /// Called when anything happens to a list.
        /// </summary>
        public UnityEvent unityUpdated;

        public ObservedUnityEventList() { }
        public ObservedUnityEventList(List<T> list)
        {
            _list = list;
        }
        public ObservedUnityEventList(IEnumerable<T> list)
        {
            _list = new List<T>(list);
        }

        public override void OnArrayUpdated()
        {
            base.OnArrayUpdated();
            unityUpdated?.Invoke();
        }
        public override void OnArrayChanged(int index, T oldValue, T newValue)
        {
            base.OnArrayChanged(index, oldValue, newValue);
            unityChanged?.Invoke(index, oldValue, newValue);
        }
    }
}
