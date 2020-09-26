using System.Collections.Generic;

namespace SimpleScheme.Lib
{
    public class SpecialForm
    {
        private delegate SchemeObject SpecialFormCode(Environment env, List<SchemeObject> args, SpecialForm self);

        public string? Name { get; }
        private readonly SpecialFormCode _code;
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

            return _code(env, args, this);
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
            InstallSpecialForm(table, "set!", Set, 2, false);
            InstallSpecialForm(table, "if", If, 2, true);
        }

        private static SchemeObject Quote(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            return args[0];
        }

        private static SchemeObject Define(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            if (args[0].Type != ObjectType.Symbol)
            {
                throw new SyntaxError("first argument of 'define' must be Symbol");
            }

            return env.Define(args[0], args[1]);
        }

        private static SchemeObject Set(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            if (args[0].Type != ObjectType.Symbol)
            {
                throw new SyntaxError("first argument of 'set' must be Symbol");
            }

            return env.Set(args[0], args[1]);
        }

        private static SchemeObject If(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            if (args.Count > 3)
            {
                throw new WrongNumberArguments(self, args.Count);
            }

            SchemeObject condition = args[0].Eval(env);
            if (condition.IsTrue())
            {
                return args[1].Eval(env);
            }

            if (args.Count == 2)
            {
                return SchemeObject.CreateUndefined();
            }

            return args[2].Eval(env);
        }
    }
}
