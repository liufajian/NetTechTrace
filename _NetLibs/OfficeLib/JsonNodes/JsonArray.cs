using System;
using System.Collections;
using System.Collections.Generic;

namespace OfficeLib.JsonNodes
{
    public class JsonArray : JsonValue, IList<JsonValue>
    {
        private readonly List<JsonValue> _list;

        public JsonArray(params JsonValue[] items)
        {
            _list = new List<JsonValue>();
            AddRange(items);
        }

        public JsonArray(IEnumerable<JsonValue> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _list = new List<JsonValue>(items);
        }

        public override int Count => _list.Count;

        public bool IsReadOnly => false;

        public override sealed JsonValue this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        public override JsonType JsonType => JsonType.Array;

        public void Add(JsonValue item)
        {
            _list.Add(item);
        }

        public void AddRange(IEnumerable<JsonValue> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _list.AddRange(items);
        }

        public void AddRange(params JsonValue[] items)
        {
            if (items != null)
            {
                _list.AddRange(items);
            }
        }

        public void Clear() => _list.Clear();

        public bool Contains(JsonValue item) => _list.Contains(item);

        public void CopyTo(JsonValue[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public int IndexOf(JsonValue item) => _list.IndexOf(item);

        public void Insert(int index, JsonValue item) => _list.Insert(index, item);

        public bool Remove(JsonValue item) => _list.Remove(item);

        public void RemoveAt(int index) => _list.RemoveAt(index);

        IEnumerator<JsonValue> IEnumerable<JsonValue>.GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
    }
}
