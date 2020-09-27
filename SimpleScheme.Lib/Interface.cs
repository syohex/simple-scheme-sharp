using System.Collections.Generic;

namespace SimpleScheme.Lib
{
    public abstract class Callable
    {
        public string? Name { get; }
        public int Arity { get; }
        public bool Variadic { get; }

        protected Callable(string? name, int arity, bool variadic)
        {
            Name = name;
            Arity = arity;
            Variadic = variadic;
        }
    }

    public interface IApplication
    {
        public SchemeObject Apply(Environment env, List<SchemeObject> actualArgs);
    }
}
