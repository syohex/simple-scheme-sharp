using System;
using System.IO;

namespace SimpleScheme.Lib
{
    public static class Interpreter
    {
        private static SchemeObject ReadFixNum(Reader reader, int c)
        {
            var positive = true;
            if (c == '-')
            {
                positive = false;
            }
            else
            {
                reader.PushBackCharacter(c);
            }

            bool hasPoint = false;
            double value = 0;
            double div = 10;
            while (true)
            {
                c = reader.NextChar();
                if (c == -1)
                {
                    break;
                }

                if (c == '.')
                {
                    if (hasPoint)
                    {
                        throw new SyntaxError(
                            $"floating point value has multiple dot(Line {reader.Line}:{reader.Column}");
                    }

                    hasPoint = true;
                    continue;
                }

                if (!char.IsDigit((char) c))
                {
                    break;
                }

                if (hasPoint)
                {
                    value = value + ((c - '0') / div);
                    div *= 10;
                }
                else
                {
                    value = value * 10 + (c - '0');
                }
            }

            if (!positive)
            {
                value *= -1;
            }

            if (!reader.IsDelimiter(c))
            {
                throw new SyntaxError($"number is not followed by delimiter(Line {reader.Line}:{reader.Column})");
            }

            if (hasPoint)
            {
                return SchemeObject.CreateFloat(value);
            }

            return SchemeObject.CreateFixnum((long) value);
        }

        public static SchemeObject Read(TextReader r)
        {
            var reader = new Reader(r);

            reader.SkipWhiteSpaces();

            var c = reader.NextChar();
            if (char.IsDigit((char) c) || c == '-' && char.IsDigit((char) reader.Peek()))
            {
                return ReadFixNum(reader, c);
            }

            throw new UnsupportedDataType("got unsupported data type");
        }
    }
}
