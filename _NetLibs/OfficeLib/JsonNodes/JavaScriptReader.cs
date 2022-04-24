using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OfficeLib.JsonNodes
{
    internal class JavaScriptReader
    {
        const string ArgumentException_ExtraCharacters = "Extra characters in JSON input.";
        const string ArgumentException_IncompleteInput = "Incomplete JSON input.";
        const string ArgumentException_ArrayMustEndWithBracket = "JSON array must end with ']'.";
        const string ArgumentException_UnexpectedCharacter = "Unexpected character '{0}'.";
        const string ArgumentException_LeadingZeros = "Leading zeros are not allowed.";
        const string ArgumentException_NoDigitFound = "Invalid JSON numeric literal; no digit found.";
        const string ArgumentException_ExtraDot = "Invalid JSON numeric literal; extra dot.";
        const string ArgumentException_IncompleteExponent = "Invalid JSON numeric literal; incomplete exponent.";
        const string ArgumentException_InvalidLiteralFormat = "Invalid JSON string literal format.";
        const string ArgumentException_StringNotClosed = "JSON string is not closed.";
        const string ArgumentException_IncompleteEscapeSequence = "Invalid JSON string literal; incomplete escape sequence.";
        const string ArgumentException_IncompleteEscapeLiteral = "Incomplete unicode character escape literal.";
        const string ArgumentException_UnexpectedEscapeCharacter = "Invalid JSON string literal; unexpected escape character.";
        const string ArgumentException_ExpectedXButGotY = "Expected '{0}', got '{1}'.";
        const string ArgumentException_ExpectedXDiferedAtY = "Expected '{0}', differed at {1}.";
        const string ArgumentException_MessageAt = "{0} At line {1}, column {2}.";

        private readonly TextReader _r;
        private int _line = 1, _column = 0;
        private int _peek;
        private bool _has_peek;
        private bool _prev_lf;

        public JavaScriptReader(TextReader reader)
        {
            Debug.Assert(reader != null);

            _r = reader;
        }

        public object Read()
        {
            var v = ReadCore();
            SkipSpaces();
            if (ReadChar() >= 0)
            {
                throw JsonError(ArgumentException_ExtraCharacters);
            }
            return v;
        }

        private object ReadCore()
        {
            SkipSpaces();
            var c = PeekChar();
            if (c < 0)
            {
                throw JsonError(ArgumentException_IncompleteInput);
            }

            switch (c)
            {
                case '[':
                    ReadChar();
                    var list = new List<object>();
                    SkipSpaces();
                    if (PeekChar() == ']')
                    {
                        ReadChar();
                        return list;
                    }

                    while (true)
                    {
                        list.Add(ReadCore());
                        SkipSpaces();
                        c = PeekChar();
                        if (c != ',')
                            break;
                        ReadChar();
                        continue;
                    }

                    if (ReadChar() != ']')
                    {
                        throw JsonError(ArgumentException_ArrayMustEndWithBracket);
                    }

                    return list.ToArray();

                case '{':
                    ReadChar();
                    var obj = new Dictionary<string, object>();
                    SkipSpaces();
                    if (PeekChar() == '}')
                    {
                        ReadChar();
                        return obj;
                    }

                    while (true)
                    {
                        SkipSpaces();
                        if (PeekChar() == '}')
                        {
                            ReadChar();
                            break;
                        }
                        var name = ReadStringLiteral();
                        SkipSpaces();
                        Expect(':');
                        SkipSpaces();
                        obj[name] = ReadCore(); // it does not reject duplicate names.
                        SkipSpaces();
                        c = ReadChar();
                        if (c == ',')
                        {
                            continue;
                        }
                        if (c == '}')
                        {
                            break;
                        }
                    }
                    return obj.ToArray();

                case 't':
                    Expect("true");
                    return true;

                case 'f':
                    Expect("false");
                    return false;

                case 'n':
                    Expect("null");
                    return null;

                case '"':
                    return ReadStringLiteral();

                default:
                    if ('0' <= c && c <= '9' || c == '-')
                    {
                        return ReadNumericLiteral();
                    }
                    throw JsonError(string.Format(ArgumentException_UnexpectedCharacter, (char)c));
            }
        }

        private int PeekChar()
        {
            if (!_has_peek)
            {
                _peek = _r.Read();
                _has_peek = true;
            }
            return _peek;
        }

        private int ReadChar()
        {
            var v = _has_peek ? _peek : _r.Read();

            _has_peek = false;

            if (_prev_lf)
            {
                _line++;
                _column = 0;
                _prev_lf = false;
            }

            if (v == '\n')
            {
                _prev_lf = true;
            }

            _column++;

            return v;
        }

        private void SkipSpaces()
        {
            while (true)
            {
                switch (PeekChar())
                {
                    case ' ':
                    case '\t':
                    case '\r':
                    case '\n':
                        ReadChar();
                        continue;

                    default:
                        return;
                }
            }
        }

        // It could return either int, long, ulong, decimal or double, depending on the parsed value.
        private object ReadNumericLiteral()
        {
            var sb = new StringBuilder();

            if (PeekChar() == '-')
            {
                sb.Append((char)ReadChar());
            }

            int c;
            var x = 0;
            var zeroStart = PeekChar() == '0';
            for (; ; x++)
            {
                c = PeekChar();
                if (c < '0' || '9' < c)
                {
                    break;
                }

                sb.Append((char)ReadChar());
                if (zeroStart && x == 1)
                {
                    throw JsonError(ArgumentException_LeadingZeros);
                }
            }

            if (x == 0) // Reached e.g. for "- "
            {
                throw JsonError(ArgumentException_NoDigitFound);
            }

            // fraction
            var hasFrac = false;
            var fdigits = 0;
            if (PeekChar() == '.')
            {
                hasFrac = true;
                sb.Append((char)ReadChar());
                if (PeekChar() < 0)
                {
                    throw JsonError(ArgumentException_ExtraDot);
                }

                while (true)
                {
                    c = PeekChar();
                    if (c < '0' || '9' < c)
                    {
                        break;
                    }

                    sb.Append((char)ReadChar());
                    fdigits++;
                }
                if (fdigits == 0)
                {
                    throw JsonError(ArgumentException_ExtraDot);
                }
            }

            c = PeekChar();
            if (c != 'e' && c != 'E')
            {
                if (!hasFrac)
                {
                    if (int.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var valueInt))
                    {
                        return valueInt;
                    }

                    if (long.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var valueLong))
                    {
                        return valueLong;
                    }

                    if (ulong.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var valueUlong))
                    {
                        return valueUlong;
                    }
                }

                if (decimal.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var valueDecimal) && valueDecimal != 0)
                {
                    return valueDecimal;
                }
            }
            else
            {
                // exponent
                sb.Append((char)ReadChar());
                if (PeekChar() < 0)
                {
                    throw JsonError(ArgumentException_IncompleteExponent);
                }

                c = PeekChar();
                if (c == '-')
                {
                    sb.Append((char)ReadChar());
                }
                else if (c == '+')
                {
                    sb.Append((char)ReadChar());
                }

                if (PeekChar() < 0)
                {
                    throw JsonError(ArgumentException_IncompleteExponent);
                }

                while (true)
                {
                    c = PeekChar();
                    if (c < '0' || '9' < c)
                    {
                        break;
                    }

                    sb.Append((char)ReadChar());
                }
            }

            return double.Parse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private readonly StringBuilder _vb = new StringBuilder();

        private string ReadStringLiteral()
        {
            if (PeekChar() != '"')
            {
                throw JsonError(ArgumentException_InvalidLiteralFormat);
            }

            ReadChar();
            _vb.Length = 0;
            while (true)
            {
                var c = ReadChar();
                if (c < 0)
                {
                    throw JsonError(ArgumentException_StringNotClosed);
                }

                if (c == '"')
                {
                    return _vb.ToString();
                }
                else if (c != '\\')
                {
                    _vb.Append((char)c);
                    continue;
                }

                // escaped expression
                c = ReadChar();
                if (c < 0)
                {
                    throw JsonError(ArgumentException_IncompleteEscapeSequence);
                }
                switch (c)
                {
                    case '"':
                    case '\\':
                    case '/':
                        _vb.Append((char)c);
                        break;
                    case 'b':
                        _vb.Append('\x8');
                        break;
                    case 'f':
                        _vb.Append('\f');
                        break;
                    case 'n':
                        _vb.Append('\n');
                        break;
                    case 'r':
                        _vb.Append('\r');
                        break;
                    case 't':
                        _vb.Append('\t');
                        break;
                    case 'u':
                        ushort cp = 0;
                        for (var i = 0; i < 4; i++)
                        {
                            cp <<= 4;
                            if ((c = ReadChar()) < 0)
                            {
                                throw JsonError(ArgumentException_IncompleteEscapeLiteral);
                            }

                            if ('0' <= c && c <= '9')
                            {
                                cp += (ushort)(c - '0');
                            }
                            if ('A' <= c && c <= 'F')
                            {
                                cp += (ushort)(c - 'A' + 10);
                            }
                            if ('a' <= c && c <= 'f')
                            {
                                cp += (ushort)(c - 'a' + 10);
                            }
                        }
                        _vb.Append((char)cp);
                        break;
                    default:
                        throw JsonError(ArgumentException_UnexpectedEscapeCharacter);
                }
            }
        }

        private void Expect(char expected)
        {
            int c;
            if ((c = ReadChar()) != expected)
            {
                throw JsonError(string.Format(ArgumentException_ExpectedXButGotY, expected, (char)c));
            }
        }

        private void Expect(string expected)
        {
            for (var i = 0; i < expected.Length; i++)
            {
                if (ReadChar() != expected[i])
                {
                    throw JsonError(string.Format(ArgumentException_ExpectedXDiferedAtY, expected, i));
                }
            }
        }

        private Exception JsonError(string msg)
        {
            return new ArgumentException(string.Format(ArgumentException_MessageAt, msg, _line, _column));
        }
    }
}