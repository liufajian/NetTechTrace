using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace OfficeLib.Json
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class JsonValue : IEnumerable
    {
        public virtual int Count
        {
            get { throw new InvalidOperationException(); }
        }

        public abstract JsonType JsonType { get; }

        public virtual JsonValue this[int index]
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual JsonValue this[string key]
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual bool ContainsKey(string key)
        {
            throw new InvalidOperationException();
        }

        public string ToJsonString()
        {
            var sb = new StringBuilder(1024);
            ToJsonString(sb);
            return sb.ToString();
        }

        private void ToJsonString(StringBuilder sb)
        {
            switch (JsonType)
            {
                case JsonType.Object:
                    sb.Append('{');
                    var following = false;
                    foreach (var pair in ((JsonObject)this))
                    {
                        if (following)
                        {
                            sb.Append(", ");
                        }

                        sb.Append('\"');
                        sb.Append(JsonHelper.EscapeString(pair.Key));
                        sb.Append("\": ");
                        if (pair.Value == null)
                        {
                            sb.Append("null");
                        }
                        else
                        {
                            pair.Value.ToJsonString(sb);
                        }

                        following = true;
                    }
                    sb.Append('}');
                    break;

                case JsonType.Array:
                    sb.Append('[');
                    following = false;
                    foreach (var v in ((JsonArray)this))
                    {
                        if (following)
                        {
                            sb.Append(", ");
                        }

                        if (v != null)
                        {
                            v.ToJsonString(sb);
                        }
                        else
                        {
                            sb.Append("null");
                        }

                        following = true;
                    }
                    sb.Append(']');
                    break;

                case JsonType.Boolean:
                    sb.Append(ToString());
                    break;

                case JsonType.String:
                    sb.Append('"');
                    sb.Append(JsonHelper.EscapeString(ToString()));
                    sb.Append('"');
                    break;

                default:
                    sb.Append(ToString());
                    break;
            }
        }

        public override string ToString()
        {
            switch (JsonType)
            {
                case JsonType.Object:
                case JsonType.Array: return ToJsonString();
                case JsonType.Boolean:
                    return ((JsonBoolean)this).ToString();
                case JsonType.String:
                    return ((JsonString)this).ToString();
                case JsonType.Number:
                    return ((JsonNumber)this).ToString();
                default: throw new NotSupportedException();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new InvalidOperationException();
        }

        // CLI -> JsonValue

        public static implicit operator JsonValue(bool value) => new JsonBoolean(value);

        public static implicit operator JsonValue(byte value) => new JsonNumber(value);

        public static implicit operator JsonValue(char value) => new JsonString(value.ToString());

        public static implicit operator JsonValue(decimal value) => new JsonNumber(value);

        public static implicit operator JsonValue(double value) => new JsonNumber(value);

        public static implicit operator JsonValue(float value) => new JsonNumber(value);

        public static implicit operator JsonValue(int value) => new JsonNumber(value);

        public static implicit operator JsonValue(long value) => new JsonNumber(value);

        public static implicit operator JsonValue(sbyte value) => new JsonNumber(value);

        public static implicit operator JsonValue(short value) => new JsonNumber(value);

        public static implicit operator JsonValue(string value) => new JsonString(value);

        public static implicit operator JsonValue(uint value) => new JsonNumber(value);

        public static implicit operator JsonValue(ulong value) => new JsonNumber(value);

        public static implicit operator JsonValue(ushort value) => new JsonNumber(value);

        public static implicit operator JsonValue(DateTime value) => new JsonString(value.ToString(CultureInfo.InvariantCulture));

        public static implicit operator JsonValue(DateTimeOffset value) => new JsonString(value.ToString(CultureInfo.InvariantCulture));

        public static implicit operator JsonValue(Guid value) => new JsonString(value.ToString());

        public static implicit operator JsonValue(TimeSpan value) => new JsonString(value.ToString());

        public static implicit operator JsonValue(Uri value) => new JsonString(value?.ToString());
    }
}
