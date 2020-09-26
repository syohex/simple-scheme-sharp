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
            // type functions
            InstallBuiltinFunction(table, "null?", IsNull, 1, true);
            InstallBuiltinFunction(table, "boolean?", IsBool, 1, true);
            InstallBuiltinFunction(table, "symbol?", IsSymbol, 1, true);
            InstallBuiltinFunction(table, "integer?", IsInteger, 1, true);
            InstallBuiltinFunction(table, "float?", IsFloat, 1, true);
            InstallBuiltinFunction(table, "char?", IsCharacter, 1, true);
            InstallBuiltinFunction(table, "string?", IsString, 1, true);
            InstallBuiltinFunction(table, "pair?", IsPair, 1, true);
            InstallBuiltinFunction(table, "procedure?", IsProcedure, 1, true);

            InstallBuiltinFunction(table, "+", Add, 0, true);
        }

        private static SchemeObject IsNull(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.EmptyList);
        }

        private static SchemeObject IsBool(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.Boolean);
        }

        private static SchemeObject IsSymbol(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.Symbol);
        }

        private static SchemeObject IsInteger(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.Fixnum);
        }

        private static SchemeObject IsFloat(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.Float);
        }

        private static SchemeObject IsCharacter(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.Character);
        }

        private static SchemeObject IsString(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.String);
        }

        private static SchemeObject IsPair(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.Pair);
        }

        private static SchemeObject IsProcedure(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var types = new List<ObjectType> {ObjectType.BuiltinFunction, ObjectType.Closure};
            return SchemeObject.CreateBoolean(types.Contains(args[0].Type));
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
