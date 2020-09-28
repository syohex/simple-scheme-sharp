using System;
using System.Collections;
using System.IO;
using System.Text;

namespace SimpleScheme.Lib
{
    public class Interpreter
    {
        private readonly SchemeObject _trueObj;
        private readonly SchemeObject _falseObj;
        private readonly SymbolTable _globalSymbolTable;

        public Interpreter()
        {
            _trueObj = SchemeObject.CreateBoolean(true);
            _falseObj = SchemeObject.CreateBoolean(false);

            _globalSymbolTable = new SymbolTable();

            SpecialForm.SetupBuiltinSpecialForms(_globalSymbolTable);
            BuiltinFunction.SetupBuiltinFunction(_globalSymbolTable);
        }

        private bool IsAlphabetic(int c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private bool IsDigit(int c)
        {
            return c >= '0' && c <= '9';
        }

        private bool IsDelimiter(int c)
        {
            // -1 means EOF
            return char.IsWhiteSpace((char) c) || c == -1 || c == '(' || c == ')' || c == '"' || c == ';';
        }

        private bool IsSymbolCharacter(int c)
        {
            int[] cs = {'*', '+', '-', '/', '>', '<', '=', '?', '!'};
            return IsAlphabetic(c) || ((IList) cs).Contains(c);
        }

        private SchemeObject ReadNumber(Reader r, int c)
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
                    r.PutBackCharacter(c);
                    break;
            }

            bool hasPoint = false;
            bool hasE = false;
            double value = 0;
            double div = 10;
            double eValue = 0;
            int eSign = 1;
            while (true)
            {
                c = r.NextChar();
                if (c == -1)
                {
                    break;
                }

                if (c == '.')
                {
                    if (hasPoint)
                    {
                        throw new SyntaxError(
                            $"floating point value has multiple dot(Line {r.Line}:{r.Column}");
                    }

                    hasPoint = true;
                    continue;
                }

                if (c == 'e' || c == 'E')
                {
                    if (hasE)
                    {
                        throw new SyntaxError(
                            $"floating point value has multiple 'e'(Line {r.Line}:{r.Column}");
                    }

                    hasE = true;
                    continue;
                }

                if (hasE && (c == '+' || c == '-'))
                {
                    eSign = c == '-' ? -1 : 1;
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
                value *= Math.Pow(10, eSign * eValue);
            }

            if (!IsDelimiter(c))
            {
                throw new SyntaxError($"number is not followed by delimiter({r.PosInfo()})");
            }

            r.PutBackCharacter(c);

            if (hasPoint || (hasE && eSign == -1))
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
                        if (!IsDelimiter(r.Peek()))
                        {
                            throw new SyntaxError($"Unexpected character followed by #\\space({r.PosInfo()}");
                        }

                        return SchemeObject.CreateCharacter(' ');
                    }

                    break;
                case 'n':
                    if (r.Match("ewline"))
                    {
                        if (!IsDelimiter(r.Peek()))
                        {
                            throw new SyntaxError($"Unexpected character followed by #\\newline({r.PosInfo()}");
                        }

                        return SchemeObject.CreateCharacter('\n');
                    }

                    break;
            }

            if (!IsDelimiter(r.Peek()))
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
            if (c == -1)
            {
                throw new SyntaxError($"pair is not closed{r.PosInfo()}");
            }

            if (c == ')')
            {
                return SchemeObject.CreateEmptyList();
            }

            r.PutBackCharacter(c);

            SchemeObject? car = Read(r);
            if (car == null)
            {
                throw new SyntaxError("Unexpected EOF");
            }

            r.SkipWhiteSpaces();

            SchemeObject? cdr;
            c = r.NextChar();
            if (c == '.') // dotted pair
            {
                if (!IsDelimiter(r.Peek()))
                {
                    throw new SyntaxError($"dot is not followed by delimiter({r.PosInfo()})");
                }

                cdr = Read(r);
                if (cdr == null)
                {
                    throw new SyntaxError("Unexpected EOF");
                }

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

        public SchemeObject Intern(string name)
        {
            return _globalSymbolTable.Intern(name);
        }

        private SchemeObject ReadSymbol(Reader r, int c)
        {
            var sb = new StringBuilder();
            while (true)
            {
                if (!(IsSymbolCharacter(c) || IsDigit(c)))
                {
                    break;
                }

                sb.Append((char) c);
                c = r.NextChar();
            }

            if (!IsDelimiter(c))
            {
                throw new SyntaxError($"symbol '{sb}' is not followed by delimiter({r.PosInfo()})");
            }

            r.PutBackCharacter(c);
            return Intern(sb.ToString());
        }

        private SchemeObject? Read(Reader r)
        {
            r.SkipWhiteSpaces();

            var c = r.NextChar();
            if (c == -1)
            {
                return null;
            }

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
                        if (!IsDelimiter(r.Peek()))
                        {
                            throw new SyntaxError($"invalid character after #t {r.PosInfo()}");
                        }

                        return _trueObj;
                    case 'f':
                        if (!IsDelimiter(r.Peek()))
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

            // Pair
            if (c == '(')
            {
                return ReadPair(r);
            }


            // quoted
            if (c == '\'')
            {
                var quote = Intern("quote");
                var quoted = Read(r);
                if (quoted == null)
                {
                    throw new SyntaxError("unexpected EOF");
                }

                var rest = SchemeObject.CreatePair(quoted, SchemeObject.CreateEmptyList());
                return SchemeObject.CreatePair(quote, rest);
            }

            // Symbol
            if (IsSymbolCharacter(c))
            {
                return ReadSymbol(r, c);
            }

            throw new UnsupportedDataType("got unsupported data type");
        }

        public SchemeObject? Read(TextReader reader)
        {
            return Read(new Reader(reader));
        }

        public SchemeObject Eval(SchemeObject expr)
        {
            return expr.Eval(new Environment(_globalSymbolTable, this));
        }
    }
}
