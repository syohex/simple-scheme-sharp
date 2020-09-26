using System.Collections.Generic;

namespace SimpleScheme.Lib
{
    public class BuiltinFunction : Callable
    {
        private delegate SchemeObject BuiltinFunctionCode(Environment env, List<SchemeObject> args,
            BuiltinFunction self);

        private readonly BuiltinFunctionCode _code;

        private BuiltinFunction(string name, BuiltinFunctionCode code, int arity, bool variadic) : base(name, arity,
            variadic)
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

        private static void InstallBuiltinFunction(SymbolTable table, string name, BuiltinFunctionCode code, int arity,
            bool variadic)
        {
            var form = SchemeObject.CreateBuiltinFunction(new BuiltinFunction(name, code, arity, variadic));
            table.RegisterSymbol(SchemeObject.CreateSymbol(name, form));
        }

        public static void SetupBuiltinFunction(SymbolTable table)
        {
            InstallBuiltinFunction(table, "+", Add, 0, true);
        }

        private static SchemeObject Add(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            bool hasFloat = false;
            double val = 0;
            foreach (var arg in args)
            {
                switch (arg.Type)
                {
                    case ObjectType.Fixnum:
                        val += arg.Value<long>();
                        break;
                    case ObjectType.Float:
                        hasFloat = true;
                        val += arg.Value<double>();
                        break;
                    default:
                        throw new WrongTypeArgument(self, arg);
                }
            }

            return hasFloat ? SchemeObject.CreateFloat(val) : SchemeObject.CreateFixnum((long) val);
        }
    }
}
