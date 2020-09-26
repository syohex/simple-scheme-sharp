using System.Collections.Generic;

namespace SimpleScheme.Lib
{
    public class SpecialForm
    {
        public delegate SchemeObject SpecialFormCode(Environment env, List<SchemeObject> args);

        public string? Name { get; }
        private SpecialFormCode _code;
        public int Arity { get; }
        public bool Variadic { get; }

        private SpecialForm(string? name, SpecialFormCode code, int arity, bool variadic)
        {
            Name = name;
            _code = code;
            Arity = arity;
            Variadic = variadic;
        }

        public SchemeObject Apply(Environment env, List<SchemeObject> args)
        {
            if (Variadic)
            {
                if (args.Count < Arity)
                {
                    throw new WrongNumberArguments(this, args.Count);
                }
            }
            else
            {
                if (args.Count != Arity)
                {
                    throw new WrongNumberArguments(this, args.Count);
                }
            }

            return _code(env, args);
        }

        private static void InstallSpecialForm(SymbolTable table, string name, SpecialFormCode code, int arity,
            bool variadic)
        {
            var form = SchemeObject.CreateSpecialForm(new SpecialForm(name, code, arity, variadic));
            table.RegisterSymbol(SchemeObject.CreateSymbol(name, form));
        }

        public static void SetupBuiltinSpecialForms(SymbolTable table)
        {
            InstallSpecialForm(table, "quote", Quote, 1, false);
            InstallSpecialForm(table, "define", Define, 2, false);
        }

        private static SchemeObject Quote(Environment env, List<SchemeObject> args)
        {
            return args[0];
        }

        private static SchemeObject Define(Environment env, List<SchemeObject> args)
        {
            if (args[0].Type != ObjectType.Symbol)
            {
                throw new SyntaxError("first argument of 'define' must be Symbol");
            }

            return env.Define(args[0], args[1]);
        }
    }
}
