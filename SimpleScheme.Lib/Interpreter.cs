using System;
using System.IO;

namespace SimpleScheme.Lib
{
    public static class Interpreter
    {
        private static SchemeObject ReadNumber(Reader reader, int c)
        {
            var positive = true;
            switch (c)
            {
                case '+':
                    break;
                case '-':
                    positive = false;
                    break;
                default:
                    reader.PushBackCharacter(c);
                    break;
            }

            bool hasPoint = false;
            bool hasE = false;
            double value = 0;
            double div = 10;
            double eValue = 0;
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

                if (c == 'e' || c == 'E')
                {
                    if (hasE)
                    {
                        throw new SyntaxError(
                            $"floating point value has multiple 'e'(Line {reader.Line}:{reader.Column}");
                    }
                    hasE = true;
                    continue;
                }

                if (!char.IsDigit((char) c))
                {
                    break;
                }

                if (hasE)
                {
                    eValue = eValue * 10 + c - '0';
                }
                else if (hasPoint)
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

            if (hasE && eValue != 0)
            {
                value *= Math.Pow(10, eValue);
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
            if (char.IsDigit((char) c) || ((c == '+' || c == '-') && char.IsDigit((char) reader.Peek())))
            {
                return ReadNumber(reader, c);
            }

            throw new UnsupportedDataType("got unsupported data type");
        }
    }
}
