using System;
using System.Collections.Generic;
using System.IO;
using SimpleScheme.Lib;
using Xunit;

namespace SimpleScheme.Test
{
    public class ReadTest
    {
        private static void TestReadValue<TE>(string input, ObjectType type, TE expected)
        {
            Interpreter interpreter = new Interpreter();

            using var reader = new StringReader(input);
            var v = interpreter.Read(reader);
            Assert.Equal(type, v.Type);
            Assert.Equal(expected, v.Value<TE>());
        }

        private static void TestSyntaxException(string input)
        {
            Interpreter interpreter = new Interpreter();
            using var reader = new StringReader(input);
            Assert.Throws<SyntaxError>(() =>
            {
                var _ = interpreter.Read(reader);
            });
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
                TestReadValue(input, ObjectType.Fixnum, expected);
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
                TestReadValue(input, ObjectType.Float, expected);
            }
        }

        [Fact]
        public void ReadInvalidFloatingNumber()
        {
            var inputs = new[] {"123.5.123", "123e1234e"};
            foreach (var input in inputs)
            {
                TestSyntaxException(input);
            }
        }

        [Fact]
        public void ReadBoolean()
        {
            var inputs = new[]
            {
                ("#t", true),
                ("#f", false)
            };

            foreach (var (input, expected) in inputs)
            {
                TestReadValue(input, ObjectType.Boolean, expected);
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
                TestReadValue(input, ObjectType.Character, expected);
            }
        }

        [Fact]
        public void ReadCharacterInvalid()
        {
            var inputs = new[] {"#\\spaces", "#foo"};
            foreach (var input in inputs)
            {
                TestSyntaxException(input);
            }
        }

        [Fact]
        public void ReadString()
        {
            var inputs = new[]
            {
                ("\"foobar\"", "foobar"),
                ("\"foo\\nbar\"", "foo\nbar"),
            };

            foreach (var (input, expected) in inputs)
            {
                TestReadValue(input, ObjectType.String, expected);
            }
        }

        [Fact]
        public void ReadStringInvalid()
        {
            var inputs = new[] {"\"foo"};
            foreach (var input in inputs)
            {
                TestSyntaxException(input);
            }
        }

        [Fact]
        public void ReadEmptyList()
        {
            var inputs = new[]
            {
                " (           ) ",
                "()"
            };

            var interpreter = new Interpreter();

            foreach (var input in inputs)
            {
                using var reader = new StringReader(input);
                var v = interpreter.Read(reader);
                Assert.Equal(ObjectType.EmptyList, v.Type);
                Assert.True(v.Equal(interpreter.EmptyList));
            }
        }

        [Fact]
        public void ReadPair()
        {
            var interpreter = new Interpreter();
            using var reader = new StringReader("(42 #t #\\c \"foo\")");
            var v = interpreter.Read(reader);
            var p = v.Value<Pair>();
            Assert.Equal(42, p.Car.Value<long>());

            p = p.Cdr.Value<Pair>();
            Assert.True(p.Car.Value<bool>());

            p = p.Cdr.Value<Pair>();
            Assert.Equal('c', p.Car.Value<char>());

            p = p.Cdr.Value<Pair>();
            Assert.Equal("foo", p.Car.Value<string>());

            Assert.Equal(ObjectType.EmptyList, p.Cdr.Type);
        }

        [Fact]
        public void ReadDottedPair()
        {
            var interpreter = new Interpreter();
            using var reader = new StringReader("(#t . #f)");
            var v = interpreter.Read(reader);
            var p = v.Value<Pair>();
            Assert.True(p.Car.Value<bool>());
            Assert.False(p.Cdr.Value<bool>());
        }

        [Fact]
        public void ReadSymbol()
        {
            string[] inputs =
            {
                "foo",
                "bar",
                "foo"
            };

            var interpreter = new Interpreter();
            var objs = new List<SchemeObject>();
            foreach (var input in inputs)
            {
                using var reader = new StringReader(input);
                objs.Add(interpreter.Read(reader));
            }

            // same name symbols are same object
            Assert.True(objs[0] == objs[2]);
            Assert.True(objs[0] != objs[1]);
        }
    }
}
