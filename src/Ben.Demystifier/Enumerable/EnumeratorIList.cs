// Copyright (c) Ben A Adams. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace System.Collections.Generic.Enumerable
{
    public struct EnumeratorIList<T> : IEnumerator<T>
    {
        private readonly IList<T> _list;
        private int _index;

        public EnumeratorIList(IList<T> list)
        {
            _index = -1;
            _list = list;
        }

        public T Current => _list[_index];

        public bool MoveNext()
        {
            _index++;

            return _index < (_list?.Count ?? 0);
        }

        public void Dispose() { }
        object IEnumerator.Current => Current;
        public void Reset() => _index = -1;
    }
}
