// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Collections.Generic.Enumerable
{
    public static class EnumerableIList
    {
        public static EnumerableIList<T> Create<T>(IList<T> list) => new EnumerableIList<T>(list);
    }

    public struct EnumerableIList<T> : IEnumerableIList<T>, IList<T>
    {
        private readonly IList<T> _list;

        public EnumerableIList(IList<T> list) => _list = list;

        public EnumeratorIList<T> GetEnumerator() => new EnumeratorIList<T>(_list);

        public static implicit operator EnumerableIList<T>(List<T> list) => new EnumerableIList<T>(list);

        public static implicit operator EnumerableIList<T>(T[] array) => new EnumerableIList<T>(array);

        public static EnumerableIList<T> Empty = default;


        // IList pass through

        /// <inheritdoc />
        public T this[int index] { get => _list[index]; set => _list[index] = value; }

        /// <inheritdoc />
        public int Count => _list.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _list.IsReadOnly;

        /// <inheritdoc />
        public void Add(T item) => _list.Add(item);

        /// <inheritdoc />
        public void Clear() => _list.Clear();

        /// <inheritdoc />
        public bool Contains(T item) => _list.Contains(item);

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public int IndexOf(T item) => _list.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, T item) => _list.Insert(index, item);

        /// <inheritdoc />
        public bool Remove(T item) => _list.Remove(item);

        /// <inheritdoc />
        public void RemoveAt(int index) => _list.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    }
}
