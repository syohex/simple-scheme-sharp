using System.IO;
using SimpleScheme.Lib;
using Xunit;

namespace SimpleScheme.Test
{
    public class EvalTest
    {
        [Fact]
        public void EvalSelfEvaluated()
        {
            var inputs = new[]
            {
                (SchemeObject.CreateFixnum(12345), SchemeObject.CreateFixnum(12345)),
                (SchemeObject.CreateBoolean(true), SchemeObject.CreateBoolean(true)),
                (SchemeObject.CreateString("foo"), SchemeObject.CreateString("foo")),
                (SchemeObject.CreateCharacter('c'), SchemeObject.CreateCharacter('c')),
            };

            var interpreter = new Interpreter();
            foreach (var (input, expected) in inputs)
            {
                var got = interpreter.Eval(input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalQuote()
        {
            var interpreter = new Interpreter();
            var inputs = new[]
            {
                ("(quote 78)", SchemeObject.CreateFixnum(78)),
                ("(quote \"foo\")", SchemeObject.CreateString("foo")),
                ("'42", SchemeObject.CreateFixnum(42)),
                ("'bar", interpreter.Intern("bar"))
            };

            foreach (var (input, expected) in inputs)
            {
                using var reader = new StringReader(input);
                var expr = interpreter.Read(reader);
                var got = interpreter.Eval(expr);
                Assert.True(got.Equal(expected));
            }
        }
    }
}
