namespace OfficeLib.Json
{
    /// <summary>
    /// 
    /// </summary>
    public class JsonBoolean : JsonValue
    {
        public JsonBoolean(bool value)
        {
            Value = value;
        }

        public bool Value { get; }

        public override JsonType JsonType => JsonType.Boolean;

        public override string ToString()
        {
            return Value ? "true" : "false";
        }
    }
}
