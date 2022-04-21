using System;
using System.Globalization;
using System.IO;

namespace OfficeLib.Json
{
    public class JsonNumber : JsonValue
    {
        private readonly byte[] _buffer;

        public JsonNumber(byte value)
        {
            _buffer = new[] { value };
            NumberType = JsonNumberType._byte;
        }

        public JsonNumber(short value)
        {
            _buffer = BitConverter.GetBytes(value);
            NumberType = JsonNumberType._short;
        }

        public JsonNumber(int value)
        {
            _buffer = BitConverter.GetBytes(value);
            NumberType = JsonNumberType._int;
        }

        public JsonNumber(long value)
        {
            _buffer = BitConverter.GetBytes(value);
            NumberType = JsonNumberType._long;
        }

        public JsonNumber(ushort value)
        {
            _buffer = BitConverter.GetBytes(value);
            NumberType = JsonNumberType._ushort;
        }

        public JsonNumber(uint value)
        {
            _buffer = BitConverter.GetBytes(value);
            NumberType = JsonNumberType._uint;
        }

        public JsonNumber(ulong value)
        {
            _buffer = BitConverter.GetBytes(value);
            NumberType = JsonNumberType._ulong;
        }

        public JsonNumber(float value)
        {
            _buffer = BitConverter.GetBytes(value);
            NumberType = JsonNumberType._float;
        }

        public JsonNumber(double value)
        {
            _buffer = BitConverter.GetBytes(value);
            NumberType = JsonNumberType._double;
        }

        public JsonNumber(decimal value)
        {
            using (var ms = new MemoryStream(20))
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write(value);

                }
                _buffer = ms.ToArray();
            }

            NumberType = JsonNumberType._decimal;
        }

        public JsonNumberType NumberType { get; }

        public byte[] ValueBuffer => _buffer;

        public override JsonType JsonType => JsonType.Number;

        public override string ToString()
        {
            switch (NumberType)
            {
                case JsonNumberType._byte:
                    return _buffer[0].ToString(CultureInfo.InvariantCulture);
                case JsonNumberType._short:
                    return BitConverter.ToInt16(_buffer).ToString(CultureInfo.InvariantCulture);
                case JsonNumberType._ushort:
                    return BitConverter.ToUInt16(_buffer).ToString(CultureInfo.InvariantCulture);
                case JsonNumberType._int:
                    return BitConverter.ToInt32(_buffer).ToString(CultureInfo.InvariantCulture);
                case JsonNumberType._uint:
                    return BitConverter.ToUInt32(_buffer).ToString(CultureInfo.InvariantCulture);
                case JsonNumberType._long:
                    return BitConverter.ToInt64(_buffer).ToString(CultureInfo.InvariantCulture);
                case JsonNumberType._ulong:
                    return BitConverter.ToUInt64(_buffer).ToString(CultureInfo.InvariantCulture);
                case JsonNumberType._float:
                    {
                        var f = BitConverter.ToSingle(_buffer);
                        if (float.IsNaN(f))
                        {
                            return "\"NaN\"";
                        }
                        if (float.IsFinite(f))
                        {
                            return f.ToString("G9", CultureInfo.InvariantCulture);
                        }
                        if (float.IsPositiveInfinity(f))
                        {
                            return "\"Infinity\"";
                        }
                        if (double.IsNegativeInfinity(f))
                        {
                            return "\"-Infinity\"";
                        }
                        return f.ToString("G9", CultureInfo.InvariantCulture);
                    }

                case JsonNumberType._double:
                    {
                        var d = BitConverter.ToDouble(_buffer);
                        if (double.IsNaN(d))
                        {
                            return "\"NaN\"";
                        }
                        if (double.IsFinite(d))
                        {
                            return d.ToString("G17", CultureInfo.InvariantCulture);
                        }
                        if (double.IsPositiveInfinity(d))
                        {
                            return "\"Infinity\"";
                        }
                        if (double.IsNegativeInfinity(d))
                        {
                            return "\"-Infinity\"";
                        }
                        return d.ToString("G17", CultureInfo.InvariantCulture);
                    }
                case JsonNumberType._decimal:
                    {
                        using (var ms = new MemoryStream(_buffer))
                        using (var br = new BinaryReader(ms))
                        {
                            var d = br.ReadDecimal();
                            return d.ToString();
                        }
                    }
                default: return BitConverter.ToString(_buffer);
            }
        }
    }

    public enum JsonNumberType
    {
        _byte,
        _short, _ushort,
        _int, _uint,
        _long, _ulong,
        _float, _double, _decimal
    }
}
