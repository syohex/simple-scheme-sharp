using System;
using System.IO;

namespace SimpleScheme.Lib
{
    public static class Interpreter
    {
        private static SchemeObject _trueObj;
        private static SchemeObject _falseObj;

        public static void Initialize()
        {
            _trueObj = SchemeObject.CreateBoolean(true);
            _falseObj = SchemeObject.CreateBoolean(false);
        }

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
                    reader.PutBackCharacter(c);
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
                throw new SyntaxError($"number is not followed by delimiter({reader.PosInfo()})");
            }

            if (hasPoint)
            {
                return SchemeObject.CreateFloat(value);
            }

            return SchemeObject.CreateFixnum((long) value);
        }

        private static SchemeObject ReadCharacter(Reader r)
        {
            int c = r.NextChar();
            switch (c)
            {
                case -1: // EOF
                    break;
                case 's':
                    if (r.Match("pace"))
                    {
                        if (!r.IsDelimiter(r.Peek()))
                        {
                            throw new SyntaxError($"Unexpected character followed by #\\space({r.PosInfo()}");
                        }

                        return SchemeObject.CreateCharacter(' ');
                    }

                    break;
                case 'n':
                    if (r.Match("ewline"))
                    {
                        if (!r.IsDelimiter(r.Peek()))
                        {
                            throw new SyntaxError($"Unexpected character followed by #\\newline({r.PosInfo()}");
                        }

                        return SchemeObject.CreateCharacter('\n');
                    }

                    break;
            }

            if (!r.IsDelimiter(r.Peek()))
            {
                throw new SyntaxError($"Unexpected character followed by #\\({r.PosInfo()}");
            }

            return SchemeObject.CreateCharacter((char) c);
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

            if (c == '#')
            {
                c = reader.NextChar();
                switch (c)
                {
                    case 't':
                        return _trueObj;
                    case 'f':
                        return _falseObj;
                    case '\\':
                        return ReadCharacter(reader);
                    default:
                        throw new SyntaxError($"unknown '#' literal {reader.PosInfo()}");
                }
            }

            throw new UnsupportedDataType("got unsupported data type");
        }
    }
}
