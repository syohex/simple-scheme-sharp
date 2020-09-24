using System;
using System.IO;
using SimpleScheme.Lib;
using Xunit;

namespace SimpleScheme.Test
{
    public class ReadTest
    {
        private static void CheckReadValue<TE>(string input, ObjectType type, TE expected)
        {
            using var reader = new StringReader(input);
            var v = Interpreter.Read(reader);
            Assert.Equal(type, v.Type);
            Assert.Equal(expected, v.Value<TE>());
        }

        [Fact]
        public void ReadFixnum()
        {
            var inputs = new ValueTuple<string, long>[]
            {
                ("  12345 ", 12345),
                ("-42", -42),
                ("+789", 789),
                ("1e4", 10000)
            };
            foreach (var (input, expected) in inputs)
            {
                CheckReadValue(input, ObjectType.Fixnum, expected);
            }
        }

        [Fact]
        public void ReadFloatingNumber()
        {
            var inputs = new[]
            {
                ("  123.5 ", 123.5),
                ("45.25", 45.25)
            };
            foreach (var (input, expected) in inputs)
            {
                CheckReadValue(input, ObjectType.Float, expected);
            }
        }

        [Fact]
        public void ReadInvalidFloatingNumber()
        {
            var input = "123.5.123";
            using var reader = new StringReader(input);
            Assert.Throws<SyntaxError>(() =>
            {
                var _ = Interpreter.Read(reader);
            });
        }

        [Fact]
        public void ReadBoolean()
        {
            Interpreter.Initialize();

            var inputs = new[]
            {
                ("#t", true),
                ("#f", false)
            };

            foreach (var (input, expected) in inputs)
            {
                CheckReadValue(input, ObjectType.Boolean, expected);
            }
        }

        [Fact]
        public void ReadCharacter()
        {
            var inputs = new[]
            {
                ("#\\space", ' '),
                ("#\\newline", '\n'),
                ("#\\a", 'a'),
                ("#\\Z", 'Z')
            };

            foreach (var (input, expected) in inputs)
            {
                CheckReadValue(input, ObjectType.Character, expected);
            }
        }
    }
}
