using System;
using System.IO;
using SimpleScheme.Lib;
using Xunit;

namespace SimpleScheme.Test
{
    public class ReadTest
    {
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
                using var reader = new StringReader(input);
                var v = Interpreter.Read(reader);
                Assert.Equal(ObjectType.Fixnum, v.Type);
                Assert.Equal(expected, v.Value<long>());
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
                using var reader = new StringReader(input);
                var v = Interpreter.Read(reader);
                Assert.Equal(ObjectType.Float, v.Type);
                Assert.Equal(expected, v.Value<double>());
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
    }
}
