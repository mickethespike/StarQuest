﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended.Collections;

namespace StarQuest
{
    // listyboy
    public class ListArray<T> :
        ICollection<T>, IList<T>, IReadOnlyCollection<T>,
        IReadOnlyList<T>, IEnumerable<T>, IReferenceList<T>
    {
        public delegate void VersionChangedDelegate(int oldVersion, int newVersion);
        public event VersionChangedDelegate Changed;

        private const int _defaultCapacity = 4;

        private int __version;

        public bool IsReadOnly { get; private set; }
        public bool IsFixedCapacity { get; private set; }
        public int Version { get => __version; protected set => SetVersion(value); }

        public T[] InnerArray { get; private set; }
        public int Count { get; private set; }

        public T this[int index]
        {
            get
            {
                if (index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return InnerArray[index];
            }
            set
            {
                CheckAccessibility();

                InnerArray[index] = value;
                Version++;
            }
        }

        public int Capacity
        {
            get => InnerArray.Length;
            set
            {
                if (IsFixedCapacity)
                    throw new InvalidOperationException(
                        "This collection has a fixed capacity therefore cannot be resized.");

                CheckAccessibility();

                if (value != InnerArray.Length)
                {
                    if (value < Count)
                        throw new ArgumentException(
                            "The new capacity is not enough to contain existing items.", nameof(value));

                    if (value > 0)
                    {
                        T[] newItems = new T[value];
                        if (Count > 0)
                        {
                            Array.Copy(InnerArray, 0, newItems, 0, Count);
                        }
                        InnerArray = newItems;
                    }
                    else
                    {
                        InnerArray = Array.Empty<T>();
                    }
                    Version++;
                }
            }
        }

        public ListArray()
        {
            InnerArray = Array.Empty<T>();
        }

        public ListArray(int capacity)
        {
            InnerArray = new T[capacity];
        }

        public ListArray(int capacity, bool fixedCapacity) : this(capacity)
        {
            IsFixedCapacity = fixedCapacity;
        }

        public ListArray(T[] sourceArray, int startOffset, int count)
        {
            InnerArray = sourceArray;
            Count = count;
            Capacity = sourceArray.Length;
            IsFixedCapacity = true;

            if (startOffset != 0)
            {
                Array.ConstrainedCopy(sourceArray, startOffset, sourceArray, 0, count);
                Array.Clear(sourceArray, count, Capacity - count);
            }
        }

        public ListArray(T[] sourceArray, int count) : this(sourceArray, 0, count)
        {
        }

        public ListArray(T[] sourceArray) : this(sourceArray, 0, sourceArray.Length)
        {
        }

        public ListArray(IEnumerable<T> collection, bool readOnly)
        {
            if (collection is ICollection<T> c)
            {
                int count = c.Count;
                if (count == 0)
                {
                    InnerArray = Array.Empty<T>();
                }
                else
                {
                    InnerArray = new T[count];
                    c.CopyTo(InnerArray, 0);
                    Count = count;
                }
            }
            else
            {
                Count = 0;
                InnerArray = Array.Empty<T>();

                AddRange(collection);
            }

            IsReadOnly = readOnly;
        }

        public ListArray(IEnumerable<T> collection) : this(collection, false)
        {
        }

        private void CheckAccessibility()
        {
            if (IsReadOnly)
                throw new InvalidOperationException("This collection is marked as read-only.");
        }

        public ref T GetReferenceAt(int index)
        {
            if (index >= Count)
                throw new IndexOutOfRangeException();

            return ref InnerArray[index];
        }

        private void SetVersion(int value)
        {
            Changed?.Invoke(__version, value);
            __version = value;
        }

        public void AddRef(ref T item)
        {
            AddCheck();
            InnerArray[Count++] = item;
            Version++;
        }

        public void Add(T item)
        {
            AddCheck();
            InnerArray[Count++] = item;
            Version++;
        }

        private void AddCheck()
        {
            CheckAccessibility();

            if (Count == InnerArray.Length)
                EnsureCapacity(Count + 1);
        }

        public void AddRange(IEnumerable<T> collection)
        {
            InsertRange(Count, collection);
        }

        public void Sort(IComparer<T> comparer)
        {
            Sort(0, Count, comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            CheckAccessibility();

            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index));

            if (count < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(count), "Needs a non-negative number.");

            if (Count - index < count)
                throw new ArgumentException("Invalid offset length.");

            Array.Sort(InnerArray, index, count, comparer);
            Version++;
        }

        public void Clear()
        {
            Clear(true);
        }

        /// <summary>
        /// Clears the list with the option to make all elements their default (to null if <typeparamref name="T"/> is a class).
        /// </summary>
        /// <param name="setToDefault">
        /// <see langword="true"/> to clear the <see cref="InnerArray"/>
        /// (only available if <typeparamref name="T"/> is a value type).
        /// </param>
        public void Clear(bool setToDefault)
        {
            CheckAccessibility();

            if (Count > 0)
            {
                if (setToDefault || !typeof(T).IsValueType)
                {
                    Array.Clear(InnerArray, 0, Count);
                }
                Count = 0;
                Version++;
            }
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.ConstrainedCopy(InnerArray, 0, array, arrayIndex, Count);
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index != -1)
            {
                RemoveAtInternal(index);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            GetAndRemoveAt(index);
        }

        public T GetAndRemoveAt(int index)
        {
            if (index >= Count || index < 0)
                throw new IndexOutOfRangeException();

            return RemoveAtInternal(index);
        }

        public T GetAndRemoveLast()
        {
            if (Count > 0)
                return GetAndRemoveAt(Count - 1);
            return default;
        }

        public int FindIndex(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; i++)
            {
                if (predicate.Invoke(InnerArray[i]) == true)
                    return i;
            }
            return -1;
        }

        private T RemoveAtInternal(int index)
        {
            CheckAccessibility();

            Count--;
            if (index < Count)
            {
                Array.Copy(InnerArray, index + 1, InnerArray, index, Count - index);
            }
            T item = InnerArray[Count];
            InnerArray[Count] = default;
            Version++;
            return item;
        }

        public int IndexOf(T item)
        {
            if (item == null)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (InnerArray[i] == null)
                        return i;
                }
            }
            else
            {
                for (int i = 0; i < Count; i++)
                {
                    T obj = InnerArray[i];
                    if (obj != null && obj.Equals(item))
                        return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            CheckAccessibility();

            InternalInsert(index, item);
        }

        private void InternalInsert(int index, T item)
        {
            if (index > Capacity)
                throw new IndexOutOfRangeException();

            if (Count == InnerArray.Length)
                EnsureCapacity(Count + 1);

            if (index < Count)
            {
                Array.Copy(InnerArray, index, InnerArray, index + 1, Count - index);
            }

            InnerArray[index] = item;
            Count++;
            Version++;
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            CheckAccessibility();

            if (collection is ICollection<T> c)
            {
                int count = c.Count;
                if (count > 0)
                {
                    EnsureCapacity(Count + count);
                    if (index < Count)
                    {
                        Array.Copy(InnerArray, index, InnerArray, index + count, count - index);
                    }

                    if (c == this)
                    {
                        Array.Copy(InnerArray, 0, InnerArray, index, index);
                        Array.Copy(InnerArray, index + count, InnerArray, index * 2, Count - index);
                    }
                    else
                    {
                        c.CopyTo(InnerArray, index);
                    }
                    Count += count;
                }
            }
            else
            {
                foreach (var item in collection)
                {
                    Insert(index++, item);
                }
            }
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            return new ReadOnlyCollection<T>(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new InvalidOperationException();
        }

        private void EnsureCapacity(int min)
        {
            if (InnerArray.Length < min)
            {
                int newCapacity = InnerArray.Length == 0 ? _defaultCapacity : InnerArray.Length * 2;
                if (newCapacity < min)
                    newCapacity = min;

                Capacity = newCapacity;
            }
        }

        struct Enumerator : IEnumerator<T>, IEnumerator
        {
            private ListArray<T> _list;
            private int _index;
            private readonly int _version;

            public T Current { get; private set; }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _list.Count + 1)
                    {
                        throw new InvalidOperationException(
                            "Either MoveNext has not been called or index is beyond item count.");
                    }
                    return Current;
                }
            }

            public Enumerator(ListArray<T> list)
            {
                _list = list;
                _index = 0;
                _version = _list.__version;
                Current = default;
            }

            public bool MoveNext()
            {
                if (_version == _list.__version && _index < _list.Count)
                {
                    Current = _list.InnerArray[_index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _list.__version)
                {
                    throw GetVersionException();
                }

                _index = _list.Count + 1;
                Current = default;
                return false;
            }

            void IEnumerator.Reset()
            {
                if (_version != _list.__version)
                {
                    throw GetVersionException();
                }

                _index = 0;
                Current = default;
            }

            private InvalidOperationException GetVersionException()
            {
                return new InvalidOperationException(
                    "The underlying list version has changed.");
            }

            public void Dispose()
            {
            }
        }
    }
}
