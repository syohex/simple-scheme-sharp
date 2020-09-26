using System.Collections.Generic;
using System.Globalization;

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

            // conversion functions
            InstallBuiltinFunction(table, "char->integer", CharToInteger, 1, true);
            InstallBuiltinFunction(table, "integer->char", IntegerToChar, 1, true);
            InstallBuiltinFunction(table, "number->string", NumberToString, 1, true);
            InstallBuiltinFunction(table, "string->number", StringToNumber, 1, true);
            InstallBuiltinFunction(table, "symbol->string", SymbolToString, 1, true);
            InstallBuiltinFunction(table, "string->symbol", StringToSymbol, 1, true);

            // arithmetic operator
            InstallBuiltinFunction(table, "+", Add, 0, true);
            InstallBuiltinFunction(table, "-", Substract, 1, true);
            InstallBuiltinFunction(table, "*", Multiply, 0, true);
            InstallBuiltinFunction(table, "/", Divide, 1, true);
            InstallBuiltinFunction(table, "mod", Modulo, 2, false);
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

        private static SchemeObject CharToInteger(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.Character)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            return SchemeObject.CreateFixnum(args[0].Value<char>());
        }

        private static SchemeObject IntegerToChar(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.Fixnum)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            return SchemeObject.CreateCharacter((char) args[0].Value<long>());
        }

        private static SchemeObject NumberToString(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            switch (args[0].Type)
            {
                case ObjectType.Fixnum:
                    return SchemeObject.CreateString(args[0].Value<long>().ToString());
                case ObjectType.Float:
                    return SchemeObject.CreateString(args[0].Value<double>().ToString(CultureInfo.InvariantCulture));
                default:
                    throw new WrongTypeArgument(self, args[0]);
            }
        }

        private static SchemeObject StringToNumber(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.String)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            var str = args[0].Value<string>();
            if (long.TryParse(str, out long longVar))
            {
                return SchemeObject.CreateFixnum(longVar);
            }

            if (double.TryParse(str, out double floatVar))
            {
                return SchemeObject.CreateFloat(floatVar);
            }

            throw new WrongTypeArgument(self, args[0]);
        }

        private static SchemeObject SymbolToString(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.Symbol)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            return SchemeObject.CreateString(args[0].Value<Symbol>().Name);
        }

        private static SchemeObject StringToSymbol(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.String)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            return env.Intern(args[0].Value<string>());
        }

        private static (double, bool) RetrieveNumber(SchemeObject arg, BuiltinFunction self)
        {
            switch (arg.Type)
            {
                case ObjectType.Fixnum:
                    return (arg.Value<long>(), false);
                case ObjectType.Float:
                    return (arg.Value<double>(), true);
                default:
                    throw new WrongTypeArgument(self, arg);
            }
        }

        private static SchemeObject Add(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            bool hasFloat = false;
            double ret = 0;
            foreach (var arg in args)
            {
                var (val, isFloat) = RetrieveNumber(arg, self);
                ret += val;

                if (isFloat)
                {
                    hasFloat = true;
                }
            }

            return hasFloat ? SchemeObject.CreateFloat(ret) : SchemeObject.CreateFixnum((long) ret);
        }

        private static SchemeObject Substract(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var (val, isFloat) = RetrieveNumber(args[0], self);
            if (args.Count == 1)
            {
                return isFloat ? SchemeObject.CreateFloat(-1 * val) : SchemeObject.CreateFixnum((long) (-1 * val));
            }

            bool hasFloat = false;
            double ret = val;
            for (var i = 1; i < args.Count; ++i)
            {
                (val, isFloat) = RetrieveNumber(args[i], self);
                ret -= val;
                if (isFloat)
                {
                    hasFloat = true;
                }
            }

            return hasFloat ? SchemeObject.CreateFloat(ret) : SchemeObject.CreateFixnum((long) ret);
        }

        private static SchemeObject Multiply(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            bool hasFloat = false;
            double ret = 1;
            foreach (var arg in args)
            {
                var (val, isFloat) = RetrieveNumber(arg, self);
                ret *= val;
                if (isFloat)
                {
                    hasFloat = true;
                }
            }

            return hasFloat ? SchemeObject.CreateFloat(ret) : SchemeObject.CreateFixnum((long) ret);
        }

        private static SchemeObject Divide(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var (val, _) = RetrieveNumber(args[0], self);
            if (args.Count == 1)
            {
                if (val == 0)
                {
                    throw new RuntimeException("division by zero");
                }

                return SchemeObject.CreateFloat(1.0 / val);
            }

            bool hasFloat = false;
            double ret = val;
            for (var i = 1; i < args.Count; ++i)
            {
                bool isFloat;
                (val, isFloat) = RetrieveNumber(args[i], self);
                if (val == 0)
                {
                    throw new RuntimeException("division by zero");
                }

                ret /= val;
                if (isFloat)
                {
                    hasFloat = true;
                }
            }

            return hasFloat ? SchemeObject.CreateFloat(ret) : SchemeObject.CreateFixnum((long) ret);
        }

        private static SchemeObject Modulo(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var (val1, hasFloat1) = RetrieveNumber(args[0], self);
            var (val2, hasFloat2) = RetrieveNumber(args[1], self);

            double ret = val1 % val2;
            return hasFloat1 || hasFloat2 ? SchemeObject.CreateFloat(ret) : SchemeObject.CreateFixnum((long) ret);
        }
    }
}
