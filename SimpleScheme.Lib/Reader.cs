using System.IO;
using System.Linq;

namespace SimpleScheme.Lib
{
    public class Reader
    {
        private int _line;
        private int _column;
        private int _buffer;
        private readonly TextReader _reader;

        public Reader(TextReader reader)
        {
            _line = 1;
            _column = 1;
            _buffer = -1;
            _reader = reader;
        }

        public int NextChar()
        {
            ++_column;
            if (_buffer != -1)
            {
                var ret = _buffer;
                _buffer = -1;
                return ret;
            }

            return _reader.Read();
        }

        public void PutBackCharacter(int c)
        {
            if (c == -1) // EOF
            {
                return;
            }

            --_column;
            if (_buffer != -1)
            {
                throw new InternalException("PutBackCharacter is called more than one time");
            }

            _buffer = c;
        }

        public int Peek()
        {
            if (_buffer != -1)
            {
                return _buffer;
            }

            return _reader.Peek();
        }

        public bool Match(string pattern)
        {
            foreach (var c in pattern)
            {
                if (NextChar() != c)
                {
                    PutBackCharacter(c);
                    return false;
                }
            }

            return true;
        }

        public void SkipWhiteSpaces()
        {
            int[] spaceChars = {' ', '\t', '\v', '\f', '\r'};
            int c;
            while ((c = NextChar()) != -1)
            {
                if (spaceChars.Contains(c)) continue;

                if (c == '\n')
                {
                    ++_line;
                    _column = 1;
                    continue;
                }

                // read comment
                if (c == ';')
                {
                    while ((c = NextChar()) != -1 && c != '\n')
                    {
                    }

                    continue;
                }

                PutBackCharacter(c);
                break;
            }
        }

        public string PosInfo()
        {
            return $"Line {_line}:{_column}";
        }

        public int Line => _line;
        public int Column => _column;
    }
}
