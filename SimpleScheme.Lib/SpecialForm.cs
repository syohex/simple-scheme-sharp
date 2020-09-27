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
            InstallSpecialForm(table, "begin", Begin, 0, true);
            InstallSpecialForm(table, "cond", Cond, 1, true);
            InstallSpecialForm(table, "let", Let, 1, true);
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

        private static SchemeObject Begin(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            var ret = SchemeObject.CreateUndefined();
            if (args.Count == 0)
            {
                return ret;
            }

            foreach (var arg in args)
            {
                ret = arg.Eval(env);
            }

            return ret;
        }

        private static SchemeObject Cond(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            foreach (var expr in args)
            {
                if (expr.Type != ObjectType.Pair)
                {
                    throw new SyntaxError($"syntax error: ${expr}");
                }

                var pair = expr.Value<Pair>();
                var cond = pair.Car.Eval(env);
                if (cond.IsTrue())
                {
                    var ret = SchemeObject.CreateUndefined();
                    var next = pair;
                    while (true)
                    {
                        if (next.Cdr.Type == ObjectType.EmptyList)
                        {
                            return ret;
                        }

                        next = next.Cdr.Value<Pair>();
                        ret = next.Car.Eval(env);
                    }
                }
            }

            return SchemeObject.CreateUndefined();
        }

        private static SchemeObject Let(Environment env, List<SchemeObject> args, SpecialForm self)
        {
            if (args[0].Type != ObjectType.Pair)
            {
                throw new SyntaxError($"syntax error: ${args[0]}");
            }

            var bindings = new Bindings();
            var pair = args[0].Value<Pair>();
            var next = pair;
            while (true)
            {
                if (next.Car.Type != ObjectType.Pair)
                {
                    throw new SyntaxError($"syntax error in let binding: ${next.Car}");
                }

                pair = next.Car.Value<Pair>();
                if (pair.Car.Type != ObjectType.Symbol)
                {
                    throw new SyntaxError($"binding name is not symbol: ${pair.Car}");
                }
                if (pair.Cdr.Type != ObjectType.Pair)
                {
                    throw new SyntaxError($"binding value is not malformed: ${pair.Cdr}");
                }

                var name = pair.Car.Value<Symbol>().Name;
                var value = pair.Cdr.Value<Pair>().Car.Eval(env);
                bindings.AddBinding(name, value);

                if (next.Cdr.Type == ObjectType.EmptyList)
                {
                    break;
                }

                if (next.Cdr.Type != ObjectType.Pair)
                {
                    throw new SyntaxError($"syntax error: ${next.Cdr}");
                }

                next = next.Cdr.Value<Pair>();
            }

            Frame letFrame = new Frame(bindings);
            env.PushFrame(letFrame);

            var ret = SchemeObject.CreateUndefined();
            for (var i = 1; i < args.Count; ++i)
            {
                ret = args[i].Eval(env);
            }

            env.PopFrame();

            return ret;
        }
    }
}
