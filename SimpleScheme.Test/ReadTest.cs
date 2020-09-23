using System.IO;
using SimpleScheme.Lib;
using Xunit;

namespace SimpleScheme.Test
{
    public class ReadTest
    {
        [Fact]
        public void ReadPositiveFixnum()
        {
            var input = "  12345  ";
            using var reader = new StringReader(input);
            var v = Interpreter.Read(reader);
            Assert.Equal(ObjectType.Fixnum, v.Type);
            Assert.Equal(12345, v.Value<long>());
        }

        [Fact]
        public void ReadNegativeFixnum()
        {
            var input = "-42";
            using var reader = new StringReader(input);
            var v = Interpreter.Read(reader);
            Assert.Equal(ObjectType.Fixnum, v.Type);
            Assert.Equal(-42, v.Value<long>());
        }
    }
}
