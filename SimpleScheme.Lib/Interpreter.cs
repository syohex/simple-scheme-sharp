using System;
using System.IO;
using System.Text;

namespace SimpleScheme.Lib
{
    public class Interpreter
    {
        private readonly SchemeObject _trueObj;
        private readonly SchemeObject _falseObj;
        public SchemeObject EmptyList { get; }

        public Interpreter()
        {
            _trueObj = SchemeObject.CreateBoolean(true);
            _falseObj = SchemeObject.CreateBoolean(false);
            EmptyList = SchemeObject.CreateEmptyList();
        }

        private SchemeObject ReadNumber(Reader reader, int c)
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

        private SchemeObject ReadCharacter(Reader r)
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

        private SchemeObject ReadString(Reader r)
        {
            var sb = new StringBuilder();
            int c;
            while ((c = r.NextChar()) != '"')
            {
                switch (c)
                {
                    case '\\':
                        c = r.NextChar();
                        if (c == 'n')
                        {
                            c = '\n';
                        }

                        break;
                    case -1: // EOF
                        throw new SyntaxError($"string is un-terminated({r.PosInfo()})");
                }

                sb.Append((char) c);
            }

            return SchemeObject.CreateString(sb.ToString());
        }

        private SchemeObject ReadPair(Reader r)
        {
            r.SkipWhiteSpaces();

            int c = r.NextChar();
            if (c == ')')
            {
                return EmptyList;
            }

            r.PutBackCharacter(c);

            SchemeObject car = Read(r);
            r.SkipWhiteSpaces();

            SchemeObject cdr;
            c = r.NextChar();
            if (c == '.') // dotted pair
            {
                if (!r.IsDelimiter(r.Peek()))
                {
                    throw new SyntaxError($"dot is not followed by delimiter({r.PosInfo()})");
                }

                cdr = Read(r);
                r.SkipWhiteSpaces();

                c = r.NextChar();
                if (c != ')')
                {
                    throw new SyntaxError($"dotted pair is not closed({r.PosInfo()})");
                }

                return SchemeObject.CreatePair(car, cdr);
            }

            r.PutBackCharacter(c);

            cdr = ReadPair(r);
            return SchemeObject.CreatePair(car, cdr);
        }

        private SchemeObject Read(Reader r)
        {
            r.SkipWhiteSpaces();

            var c = r.NextChar();

            // Number(Fixnum, Float)
            if (char.IsDigit((char) c) || ((c == '+' || c == '-') && char.IsDigit((char) r.Peek())))
            {
                return ReadNumber(r, c);
            }

            // Character or Boolean
            if (c == '#')
            {
                c = r.NextChar();
                switch (c)
                {
                    case 't':
                        if (!r.IsDelimiter(r.Peek()))
                        {
                            throw new SyntaxError($"invalid character after #t {r.PosInfo()}");
                        }

                        return _trueObj;
                    case 'f':
                        if (!r.IsDelimiter(r.Peek()))
                        {
                            throw new SyntaxError($"invalid character after #f {r.PosInfo()}");
                        }

                        return _falseObj;
                    case '\\':
                        return ReadCharacter(r);
                    default:
                        throw new SyntaxError($"unknown '#' literal {r.PosInfo()}");
                }
            }

            // String
            if (c == '"')
            {
                return ReadString(r);
            }

            if (c == '(')
            {
                return ReadPair(r);
            }

            throw new UnsupportedDataType("got unsupported data type");
        }

        public SchemeObject Read(TextReader reader)
        {
            return Read(new Reader(reader));
        }

        public bool IsEmptyList(SchemeObject obj)
        {
            return EmptyList == obj;
        }
    }
}
