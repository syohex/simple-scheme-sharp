using System.IO;

namespace SimpleScheme.Lib
{
    public static class Interpreter
    {
        private static SchemeObject ReadFixNum(Reader reader, int c)
        {
            var positive = true;
            if (c == '-')
                positive = false;
            else
                reader.PushBackCharacter(c);

            long value = 0;
            while (true)
            {
                c = reader.NextChar();
                if (!char.IsDigit((char) c)) break;

                value = value * 10 + (c - '0');
            }

            if (!positive) value *= -1;

            if (!reader.IsDelimiter(c))
            {
                throw new SyntaxError($"number is not followed by delimiter(Line {reader.Line}:{reader.Column})");
            }

            return SchemeObject.CreateFixnum(value);
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
