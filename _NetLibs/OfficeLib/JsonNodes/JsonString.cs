using System.IO;
using System.Text;

namespace OfficeLib.JsonNodes
{
    public class JsonString : JsonValue
    {
        public JsonString(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override JsonType JsonType => JsonType.String;

        public override string ToString()
        {
            return Value;
        }
    }
}
