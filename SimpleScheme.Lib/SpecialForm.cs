using System.Collections.Generic;

namespace SimpleScheme.Lib
{
    public class SpecialForm : Callable
    {
        private delegate SchemeObject SpecialFormCode(Environment env, List<SchemeObject> args, SpecialForm self);

        private readonly SpecialFormCode _code;

        private SpecialForm(string? name, SpecialFormCode code, int arity, bool variadic) : base(name, arity, variadic)
        {
            _code = code;
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
            table.RegisterValue(table.Intern(name), form);
        }

        public static void SetupBuiltinSpecialForms(SymbolTable table)
        {
            InstallSpecialForm(table, "quote", Quote, 1, false);
            InstallSpecialForm(table, "define", Define, 2, true);
            InstallSpecialForm(table, "set!", Set, 2, false);
            InstallSpecialForm(table, "if", If, 2, true);
            InstallSpecialForm(table, "lambda", Lambda, 1, true);
        }

        private static SchemeObject Quote(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            return args[0];
        }

        private static SchemeObject Define(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            switch (args[0].Type)
            {
                case ObjectType.Symbol:
                    return env.Define(args[0], args[1].Eval(env));
                case ObjectType.Pair:
                {
                    Pair p = args[0].Value<Pair>();
                    if (p.Car.Type != ObjectType.Symbol || !p.Cdr.IsListType())
                    {
                        throw new SyntaxError($"Syntax Error: {args[0]}");
                    }

                    Symbol function = p.Car.Value<Symbol>();
                    List<SchemeObject> dummyArgs;
                    if (p.Cdr.Type == ObjectType.EmptyList)
                    {
                        dummyArgs = new List<SchemeObject>();
                    }
                    else
                    {
                        dummyArgs = p.Cdr.Value<Pair>().ToList();
                    }

                    var body = new List<SchemeObject>();
                    for (var i = 1; i < args.Count; ++i)
                    {
                        body.Add(args[i]);
                    }

                    var closure = SchemeObject.CreateClosure(new Closure(function.Name, dummyArgs, body, env));
                    return env.Define(p.Car, closure);
                }
                default:
                    throw new SyntaxError("first argument of 'define' must be Symbol or Pair");
            }
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

        private static SchemeObject Lambda(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            if (!args[0].IsListType())
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            List<SchemeObject> param;
            if (args[0].Type == ObjectType.EmptyList)
            {
                param = new List<SchemeObject>();
            }
            else
            {
                param = args[0].Value<Pair>().ToList();
            }

            var body = new List<SchemeObject>();

            for (var i = 1; i < args.Count; ++i)
            {
                body.Add(args[i]);
            }

            return SchemeObject.CreateClosure(new Closure(null, param, body, env));
        }
    }
}
