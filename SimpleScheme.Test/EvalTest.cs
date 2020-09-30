using System;
using System.Collections.Generic;
using System.IO;
using SimpleScheme.Lib;
using Xunit;

namespace SimpleScheme.Test
{
    public class EvalTest
    {
        private static SchemeObject ReadEval(Interpreter interpreter, string input, bool dump = false)
        {
            using var reader = new StringReader(input);
            var expr = interpreter.Read(reader);
            if (expr == null)
            {
                throw new Exception("input is invalid");
            }

            if (dump)
            {
                Console.WriteLine($"## Read expr: {expr}");
            }

            return interpreter.Eval(expr);
        }

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
                var got = ReadEval(interpreter, input);
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
                ReadEval(interpreter, input);
            }

            var tests = new[]
            {
                ("foo", SchemeObject.CreateFixnum(99)),
                ("bar", SchemeObject.CreateString("hello")),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
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
                ReadEval(interpreter, input);
            }

            var tests = new[]
            {
                ("foo", SchemeObject.CreateFixnum(99)),
                ("bar", SchemeObject.CreateBoolean(true)),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
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
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }

            {
                var got = ReadEval(interpreter, "(if #f 10)");
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
                var got = ReadEval(interpreter, input);
                Assert.True(got.Value<bool>() == expected);
            }
        }

        [Fact]
        public void EvalValueConversion()
        {
            var interpreter = new Interpreter();
            var tests = new[]
            {
                ("(char->integer #\\a)", SchemeObject.CreateFixnum(97)),
                ("(char->integer #\\A)", SchemeObject.CreateFixnum(65)),
                ("(integer->char 97)", SchemeObject.CreateCharacter('a')),
                ("(integer->char 65)", SchemeObject.CreateCharacter('A')),
                ("(number->string 123)", SchemeObject.CreateString("123")),
                ("(number->string 123.5)", SchemeObject.CreateString("123.5")),
                ("(string->number \"42\")", SchemeObject.CreateFixnum(42)),
                ("(string->number \"34.5\")", SchemeObject.CreateFloat(34.5)),
                ("(symbol->string 'foo)", SchemeObject.CreateString("foo")),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }

            var inputs = new[]
            {
                "(string->symbol \"hello\")",
                "(string->symbol \"hello\")",
                "(string->symbol \"helloWorld\")",
            };

            var ret = new List<SchemeObject>();
            foreach (var input in inputs)
            {
                ret.Add(ReadEval(interpreter, input));
            }

            Assert.True(ret[0].Equal(ret[1]));
            Assert.False(ret[0].Equal(ret[2]));
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
                ("(- 1)", SchemeObject.CreateFixnum(-1)),
                ("(- 10 5 3)", SchemeObject.CreateFixnum(2)),
                ("(*)", SchemeObject.CreateFixnum(1)),
                ("(* 2 2.5)", SchemeObject.CreateFloat(5.0)),
                ("(/ 2)", SchemeObject.CreateFloat(0.5)),
                ("(/ 10 2.5)", SchemeObject.CreateFloat(4.0)),
                ("(mod 10 3)", SchemeObject.CreateFixnum(1)),
                ("(- (+ 1 2 3 4 5) (* 2 3))", SchemeObject.CreateFixnum(9)),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalPairOperators()
        {
            var interpreter = new Interpreter();
            {
                var got = ReadEval(interpreter, "(cons 1 '(\"foo\"))");
                var pair = got.Value<Pair>();
                Assert.True(pair.Car.Equal(SchemeObject.CreateFixnum(1)));
                Assert.True(pair.Cdr.Value<Pair>().Car.Equal(SchemeObject.CreateString("foo")));
                Assert.True(pair.Cdr.Value<Pair>().Cdr.Equal(SchemeObject.CreateEmptyList()));
            }

            var tests = new[]
            {
                ("(car '(1 2 3 4))", SchemeObject.CreateFixnum(1)),
                ("(cdr '(1 . #f))", SchemeObject.CreateBoolean(false)),
                ("(nth 0 (list 42 #f))", SchemeObject.CreateFixnum(42)),
                ("(nth 1 (list 99 \"foo\"))", SchemeObject.CreateString("foo")),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }

            var inputs = new[]
            {
                "(define foo '(1 2))",
                "(set-car! foo \"hello world\")",
                "(set-cdr! foo '(3 4.5))",
                "foo"
            };

            SchemeObject ret = SchemeObject.CreateUndefined();
            foreach (var input in inputs)
            {
                ret = ReadEval(interpreter, input);
            }

            Pair val = ret.Value<Pair>();
            Assert.True(val.Car.Equal(SchemeObject.CreateString("hello world")));
            Assert.True(val.Cdr.Value<Pair>().Car.Equal(SchemeObject.CreateFixnum(3)));
            Assert.True(val.Cdr.Value<Pair>().Cdr.Value<Pair>().Car.Equal(SchemeObject.CreateFloat(4.5)));
        }

        [Fact]
        public void EvalComparison()
        {
            var tests = new[]
            {
                ("(eq? #t #t)", true),
                ("(eq? #t #f)", false),
                ("(eq? () ())", true),
            };

            var interpreter = new Interpreter();
            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.Equal(got.Value<bool>(), expected);
            }
        }

        [Fact]
        public void EvalLambda()
        {
            var tests = new[]
            {
                ("((lambda (a b) (+ a b)) 10 20)", SchemeObject.CreateFixnum(30)),
            };

            var interpreter = new Interpreter();
            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalDefineFunction()
        {
            var interpreter = new Interpreter();
            var inputs = new[]
            {
                "(define (add x y) (+ x y))",
                "(define k ((lambda (x) x) \"bar\"))",
                "(define c ((lambda (x) (lambda () x)) \"foo\"))"
            };

            foreach (var input in inputs)
            {
                ReadEval(interpreter, input);
            }

            var tests = new[]
            {
                ("(add 12 34)", SchemeObject.CreateFixnum(46)),
                ("k", SchemeObject.CreateString("bar")),
                ("(c)", SchemeObject.CreateString("foo")),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalBegin()
        {
            var interpreter = new Interpreter();
            var tests = new[]
            {
                ("(begin 1 2)", SchemeObject.CreateFixnum(2)),
                ("(begin)", SchemeObject.CreateUndefined()),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalCond()
        {
            var interpreter = new Interpreter();
            var tests = new[]
            {
                ("(cond (#t 10) (#f 20))", SchemeObject.CreateFixnum(10)),
                ("(cond (#f 10) (#t 1 2 3 20))", SchemeObject.CreateFixnum(20)),
                ("(cond (#f 10) (#f 20))", SchemeObject.CreateUndefined())
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalLet()
        {
            var interpreter = new Interpreter();
            var tests = new[]
            {
                (@"(let ((x (+ 1 1))
                         (y (- 5 2)))
                    (+ x y))", SchemeObject.CreateFixnum(5)),
                (@"(let ((a 10)))", SchemeObject.CreateUndefined()),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalOrAnd()
        {
            var interpreter = new Interpreter();
            var tests = new[]
            {
                ("(and 1 2 #f 3)", SchemeObject.CreateBoolean(false)),
                ("(and 1 2 3)", SchemeObject.CreateFixnum(3)),
                ("(and)", SchemeObject.CreateBoolean(true)),
                ("(or)", SchemeObject.CreateBoolean(false)),
                ("(or #f \"foo\" #t #t)", SchemeObject.CreateString("foo")),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalApply()
        {
            var interpreter = new Interpreter();
            var tests = new[]
            {
                ("(apply +)", SchemeObject.CreateFixnum(0)),
                ("(apply *)", SchemeObject.CreateFixnum(1)),
                ("(apply + '(1 2 3))", SchemeObject.CreateFixnum(6)),
                ("(apply + 1 2 3 '(4 5))", SchemeObject.CreateFixnum(15)),
                ("(apply * 1 2 3 4 5 '())", SchemeObject.CreateFixnum(120)),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalEval()
        {
            var interpreter = new Interpreter();
            var inputs = new[]
            {
                "(define env (environment))",
                "(eval '(define z 25) env)",
                "(eval '(define y \"foo\") env)",
            };
            foreach (var input in inputs)
            {
                ReadEval(interpreter, input);
            }

            var tests = new[]
            {
                ("(eval 'z env)", SchemeObject.CreateFixnum(25)),
                ("(eval 'y env)", SchemeObject.CreateString("foo")),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }
        }

        [Fact]
        public void EvalLoad()
        {
            var tempFile = Path.GetTempFileName();
            var content = @"
(define (length lst)
  (if (null? lst)
       0
     (+ 1 (length (cdr lst)))))
";
            using (var stream = new StreamWriter(tempFile))
            {
                stream.Write(content);
            }

            var interpreter = new Interpreter();
            var inputs = new[]
            {
                $"(load \"{tempFile}\")",
            };
            foreach (var input in inputs)
            {
                ReadEval(interpreter, input);
            }

            var tests = new[]
            {
                ("(length '(1 2 3 4 5))", SchemeObject.CreateFixnum(5)),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }

            File.Delete(tempFile);
        }

        [Fact]
        public void EvalPort()
        {
            var tempFileIn = Path.GetTempFileName();
            using (File.Create(tempFileIn))
            {
                // create empty file
            }

            var tempFileOut = Path.GetTempFileName();
            using (var stream = new StreamWriter(tempFileOut))
            {
                stream.Write("hello world");
            }

            var interpreter = new Interpreter();
            var inputs = new[]
            {
                $"(define i-port (open-input-file \"{tempFileIn}\"))",
                $"(define o-port (open-output-file \"{tempFileOut}\"))",
            };
            foreach (var input in inputs)
            {
                ReadEval(interpreter, input);
            }

            var tests = new[]
            {
                ("(input-port? i-port)", SchemeObject.CreateBoolean(true)),
                ("(input-port? o-port)", SchemeObject.CreateBoolean(false)),

                ("(output-port? i-port)", SchemeObject.CreateBoolean(false)),
                ("(output-port? o-port)", SchemeObject.CreateBoolean(true)),

                ("(eof-object? i-port)", SchemeObject.CreateBoolean(true)),
                ("(eof-object? o-port)", SchemeObject.CreateBoolean(false)),

                ("(close-input-port i-port)", SchemeObject.CreateBoolean(true)),
                ("(close-output-port o-port)", SchemeObject.CreateBoolean(true)),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }

            File.Delete(tempFileIn);
            File.Delete(tempFileOut);
        }

        [Fact]
        public void EvalReadPort()
        {
            var tempFile1 = Path.GetTempFileName();
            using (var stream = new StreamWriter(tempFile1))
            {
                stream.Write("hello world");
            }

            var tempFile2 = Path.GetTempFileName();
            using (var stream = new StreamWriter(tempFile2))
            {
                stream.Write("#t");
            }

            var interpreter = new Interpreter();
            var inputs = new[]
            {
                $"(define i-port1 (open-input-file \"{tempFile1}\"))",
                $"(define i-port2 (open-input-file \"{tempFile2}\"))",
            };
            foreach (var input in inputs)
            {
                ReadEval(interpreter, input);
            }

            var tests = new[]
            {
                ("(read-char i-port1)", SchemeObject.CreateCharacter('h')),
                ("(peek-char i-port1)", SchemeObject.CreateCharacter('e')),
                ("(peek-char i-port1)", SchemeObject.CreateCharacter('e')),
                ("(read i-port2)", SchemeObject.CreateBoolean(true)),
                ("(close-input-port i-port1)", SchemeObject.CreateBoolean(true)),
                ("(close-input-port i-port2)", SchemeObject.CreateBoolean(true)),
            };

            foreach (var (input, expected) in tests)
            {
                var got = ReadEval(interpreter, input);
                Assert.True(got.Equal(expected));
            }

            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }
    }
}
