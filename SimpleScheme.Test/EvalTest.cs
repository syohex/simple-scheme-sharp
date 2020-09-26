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
            var tests = new[]
            {
                ("(quote 78)", SchemeObject.CreateFixnum(78)),
                ("(quote \"foo\")", SchemeObject.CreateString("foo")),
                ("'42", SchemeObject.CreateFixnum(42)),
                ("'bar", interpreter.Intern("bar"))
            };

            foreach (var (input, expected) in tests)
            {
                using var reader = new StringReader(input);
                var expr = interpreter.Read(reader);
                var got = interpreter.Eval(expr);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalDefine()
        {
            var interpreter = new Interpreter();
            var inputs = new[]
            {
                "(define foo 42)",
                "(define bar \"hello\")",
                "(define foo 99)"
            };

            foreach (var input in inputs)
            {
                using var reader = new StringReader(input);
                var expr = interpreter.Read(reader);
                interpreter.Eval(expr);
            }

            var tests = new[]
            {
                ("foo", SchemeObject.CreateFixnum(99)),
                ("bar", SchemeObject.CreateString("hello")),
            };

            foreach (var (input, expected) in tests)
            {
                using var reader = new StringReader(input);
                var expr = interpreter.Read(reader);
                var got = interpreter.Eval(expr);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalSet()
        {
            var interpreter = new Interpreter();
            var inputs = new[]
            {
                "(define foo 42)",
                "(define bar \"hello\")",
                "(set! foo 99)",
                "(define bar #t)"
            };

            foreach (var input in inputs)
            {
                using var reader = new StringReader(input);
                var expr = interpreter.Read(reader);
                interpreter.Eval(expr);
            }

            var tests = new[]
            {
                ("foo", SchemeObject.CreateFixnum(99)),
                ("bar", SchemeObject.CreateBoolean(true)),
            };

            foreach (var (input, expected) in tests)
            {
                using var reader = new StringReader(input);
                var expr = interpreter.Read(reader);
                var got = interpreter.Eval(expr);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalIf()
        {
            var interpreter = new Interpreter();
            var tests = new[]
            {
                ("(if #t 1 2)", SchemeObject.CreateFixnum(1)),
                ("(if #f 1 2)", SchemeObject.CreateFixnum(2)),
                ("(if 10 \"foo\")", SchemeObject.CreateString("foo"))
            };

            foreach (var (input, expected) in tests)
            {
                using var reader = new StringReader(input);
                var expr = interpreter.Read(reader);
                var got = interpreter.Eval(expr);
                Assert.True(got.Equal(expected));
            }

            {
                using var reader = new StringReader("(if #f 10)");
                var expr = interpreter.Read(reader);
                var got = interpreter.Eval(expr);
                Assert.True(got.IsUndefined());
            }
        }

        [Fact]
        public void EvalTypePredicate()
        {
            var interpreter = new Interpreter();
            var tests = new[]
            {
                ("(null? ())", true),
                ("(null? 32)", false),
                ("(null? #t)", false),
                ("(boolean? #t)", true),
                ("(boolean? #f)", true),
                ("(boolean? 10)", false),
                ("(symbol? 'foo)", true),
                ("(symbol? 42)", false),
                ("(integer? 42)", true),
                ("(integer? 123.5)", false),
                ("(integer? \"foo\")", false),
                ("(float? 123.25)", true),
                ("(float? #\\c)", false),
                ("(char? #\\c)", true),
                ("(char? #f)", false),
                ("(string? \"foo bar\")", true),
                ("(string? #\\newline)", false),
                ("(pair? '(1 2 3))", true),
                ("(pair? ())", false),
                ("(pair? 42)", false),
                ("(procedure? +)", true),
                ("(procedure? procedure?)", true),
                ("(procedure? quote)", false),
            };

            foreach (var (input, expected) in tests)
            {
                using var reader = new StringReader(input);
                var expr = interpreter.Read(reader);
                var got = interpreter.Eval(expr);
                Assert.Equal(got.Value<bool>(), expected);
            }
        }

        [Fact]
        public void EvalArithmeticOperator()
        {
            var interpreter = new Interpreter();
            var tests = new[]
            {
                ("(+)", SchemeObject.CreateFixnum(0)),
                ("(+ -10)", SchemeObject.CreateFixnum(-10)),
                ("(+ 1 2 3)", SchemeObject.CreateFixnum(6)),
                ("(+ 1 2 3.5)", SchemeObject.CreateFloat(6.5)),
                ("(if #t (+ 1 2) (+ 3 4))", SchemeObject.CreateFixnum(3)),
            };

            foreach (var (input, expected) in tests)
            {
                using var reader = new StringReader(input);
                var expr = interpreter.Read(reader);
                var got = interpreter.Eval(expr);
                Assert.True(got.Equal(expected));
            }
        }
    }
}
