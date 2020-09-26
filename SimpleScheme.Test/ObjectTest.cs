using SimpleScheme.Lib;
using Xunit;

namespace SimpleScheme.Test
{
    public class ObjectTest
    {
        [Fact]
        public void TestToString()
        {
            var obj1 = SchemeObject.CreateFixnum(42);
            var obj2 = SchemeObject.CreateString("hello");

            var pair = SchemeObject.CreatePair(
                obj1,
                SchemeObject.CreatePair(obj2, SchemeObject.CreateEmptyList())
            );

            var interpreter = new Interpreter();
            var tests = new[]
            {
                ("10", SchemeObject.CreateFixnum(10)),
                ("12.5", SchemeObject.CreateFloat(12.5)),
                ("\"foobar\"", SchemeObject.CreateString("foobar")),
                ("#\\c", SchemeObject.CreateCharacter('c')),
                ("#t", SchemeObject.CreateBoolean(true)),
                ("#f", SchemeObject.CreateBoolean(false)),
                ("()", SchemeObject.CreateEmptyList()),
                ("(42 . \"hello\")", SchemeObject.CreatePair(obj1, obj2)),
                ("(42 \"hello\")", pair),
                ("foo", interpreter.Intern("foo")),
            };

            foreach (var (expected, input) in tests)
            {
                Assert.Equal(expected, input.ToString());
            }
        }
    }
}
