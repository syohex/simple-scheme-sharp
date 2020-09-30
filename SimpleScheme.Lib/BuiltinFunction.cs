using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace SimpleScheme.Lib
{
    public class BuiltinFunction : Callable, IApplication
    {
        private delegate SchemeObject BuiltinFunctionCode(Environment env, List<SchemeObject> args,
            BuiltinFunction self);

        private readonly BuiltinFunctionCode _code;
        private static double floatEpsilon = 1.0e-10;

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
            var func = SchemeObject.CreateBuiltinFunction(new BuiltinFunction(name, code, arity, variadic));
            table.RegisterValue(table.Intern(name), func);
        }

        public static void SetupBuiltinFunction(SymbolTable table)
        {
            InstallBuiltinFunction(table, "environment", NewEnvironment, 0, false);

            // type functions
            InstallBuiltinFunction(table, "null?", IsNull, 1, false);
            InstallBuiltinFunction(table, "boolean?", IsBool, 1, false);
            InstallBuiltinFunction(table, "symbol?", IsSymbol, 1, false);
            InstallBuiltinFunction(table, "integer?", IsInteger, 1, false);
            InstallBuiltinFunction(table, "float?", IsFloat, 1, false);
            InstallBuiltinFunction(table, "char?", IsCharacter, 1, false);
            InstallBuiltinFunction(table, "string?", IsString, 1, false);
            InstallBuiltinFunction(table, "pair?", IsPair, 1, false);
            InstallBuiltinFunction(table, "procedure?", IsProcedure, 1, false);

            // conversion functions
            InstallBuiltinFunction(table, "char->integer", CharToInteger, 1, false);
            InstallBuiltinFunction(table, "integer->char", IntegerToChar, 1, false);
            InstallBuiltinFunction(table, "number->string", NumberToString, 1, false);
            InstallBuiltinFunction(table, "string->number", StringToNumber, 1, false);
            InstallBuiltinFunction(table, "symbol->string", SymbolToString, 1, false);
            InstallBuiltinFunction(table, "string->symbol", StringToSymbol, 1, false);

            // arithmetic operator
            InstallBuiltinFunction(table, "+", Add, 0, true);
            InstallBuiltinFunction(table, "-", Substract, 1, true);
            InstallBuiltinFunction(table, "*", Multiply, 0, true);
            InstallBuiltinFunction(table, "/", Divide, 1, true);
            InstallBuiltinFunction(table, "mod", Modulo, 2, false);

            // pair operator
            InstallBuiltinFunction(table, "cons", Cons, 2, false);
            InstallBuiltinFunction(table, "car", Car, 1, false);
            InstallBuiltinFunction(table, "cdr", Cdr, 1, false);
            InstallBuiltinFunction(table, "set-car!", SetCar, 2, false);
            InstallBuiltinFunction(table, "set-cdr!", SetCdr, 2, false);
            InstallBuiltinFunction(table, "list", List, 0, true);
            InstallBuiltinFunction(table, "nth", Nth, 2, false);

            // compare
            InstallBuiltinFunction(table, "eq?", Eq, 2, false);

            // numeric compare
            InstallBuiltinFunction(table, "=", NumericEq, 2, false);
            InstallBuiltinFunction(table, "/=", NumericNotEq, 2, false);
            InstallBuiltinFunction(table, ">", NumericGreaterThan, 2, false);
            InstallBuiltinFunction(table, ">=", NumericGreaterThanEqual, 2, false);
            InstallBuiltinFunction(table, "<", NumericLessThan, 2, false);
            InstallBuiltinFunction(table, "<=", NumericLessThanEqual, 2, false);

            // function
            InstallBuiltinFunction(table, "apply", Apply, 1, true);

            InstallBuiltinFunction(table, "eval", Eval, 2, false);
            InstallBuiltinFunction(table, "load", Load, 1, false);

            // port
            InstallBuiltinFunction(table, "open-input-file", OpenInputFile, 1, false);
            InstallBuiltinFunction(table, "close-input-port", CloseInputPort, 1, false);
            InstallBuiltinFunction(table, "input-port?", IsInputPort, 1, false);
            InstallBuiltinFunction(table, "open-output-file", OpenOutputFile, 1, false);
            InstallBuiltinFunction(table, "close-output-port", CloseOutputPort, 1, false);
            InstallBuiltinFunction(table, "output-port?", IsOutputPort, 1, false);
            InstallBuiltinFunction(table, "eof-object?", IsEof, 1, false);

            InstallBuiltinFunction(table, "read", Read, 1, true);
            InstallBuiltinFunction(table, "read-char", ReadChar, 1, true);
            InstallBuiltinFunction(table, "peek-char", PeekChar, 1, true);

            InstallBuiltinFunction(table, "write", Write, 1, true);
            InstallBuiltinFunction(table, "write-char", WriteChar, 1, true);

            InstallBuiltinFunction(table, "error", Error, 1, false);
        }

        private static SchemeObject NewEnvironment(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateEnvironment(env.GlobalTable);
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
            return SchemeObject.CreateBoolean(args[0].IsApplicable());
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

        private static SchemeObject Cons(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreatePair(args[0], args[1]);
        }

        private static SchemeObject Car(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.Pair)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            return args[0].Value<Pair>().Car;
        }

        private static SchemeObject Cdr(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.Pair)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            return args[0].Value<Pair>().Cdr;
        }

        private static SchemeObject SetCar(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.Pair)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            var pair = args[0].Value<Pair>();
            pair.Car = args[1];
            return SchemeObject.CreateUndefined();
        }

        private static SchemeObject SetCdr(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.Pair)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            Pair pair = args[0].Value<Pair>();
            pair.Cdr = args[1];
            return SchemeObject.CreateUndefined();
        }

        private static SchemeObject List(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var emptyList = SchemeObject.CreateEmptyList();
            if (args.Count == 0)
            {
                return emptyList;
            }

            SchemeObject ret = SchemeObject.CreatePair(emptyList, emptyList);
            SchemeObject iter = ret;
            foreach (var arg in args)
            {
                var p = iter.Value<Pair>();
                p.Car = arg;
                p.Cdr = SchemeObject.CreatePair(emptyList, emptyList);

                iter = p.Cdr;
            }

            iter.Value<Pair>().Cdr = emptyList;
            return ret;
        }

        private static SchemeObject Nth(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.Fixnum)
            {
                throw new WrongTypeArgument(self, args[1]);
            }

            if (args[1].Type != ObjectType.Pair)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            long index = args[0].Value<long>();
            if (index < 0)
            {
                throw new RuntimeException("index must be larger than equal 0");
            }

            var pair = args[1].Value<Pair>();
            if (index >= pair.Length())
            {
                throw new RuntimeException("index must be smaller than length");
            }

            return pair.Nth(index);
        }

        private static SchemeObject Eq(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != args[1].Type)
            {
                return SchemeObject.CreateBoolean(false);
            }

            switch (args[0].Type)
            {
                case ObjectType.Symbol:
                    return SchemeObject.CreateBoolean(args[0].Value<bool>() == args[1].Value<bool>());
                case ObjectType.Boolean:
                    return SchemeObject.CreateBoolean(args[0].Value<bool>() == args[1].Value<bool>());
                case ObjectType.EmptyList:
                case ObjectType.Undefined:
                    return SchemeObject.CreateBoolean(true);
                default:
                    return SchemeObject.CreateBoolean(args[0] == args[1]);
            }
        }

        private static SchemeObject NumericEq(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var (val1, isFloat1) = RetrieveNumber(args[0], self);
            var (val2, isFloat2) = RetrieveNumber(args[1], self);

            if (isFloat1 || isFloat2)
            {
                return SchemeObject.CreateBoolean(Math.Abs(val1 - val2) < floatEpsilon);
            }

            return SchemeObject.CreateBoolean((long) val1 == (long) val2);
        }

        private static SchemeObject NumericNotEq(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var (val1, isFloat1) = RetrieveNumber(args[0], self);
            var (val2, isFloat2) = RetrieveNumber(args[1], self);

            if (isFloat1 || isFloat2)
            {
                return SchemeObject.CreateBoolean(!(Math.Abs(val1 - val2) < floatEpsilon));
            }

            return SchemeObject.CreateBoolean((long) val1 != (long) val2);
        }

        private static SchemeObject NumericGreaterThan(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var (val1, isFloat1) = RetrieveNumber(args[0], self);
            var (val2, isFloat2) = RetrieveNumber(args[1], self);

            if (isFloat1 || isFloat2)
            {
                return SchemeObject.CreateBoolean(val1 > val2);
            }

            return SchemeObject.CreateBoolean((long) val1 > (long) val2);
        }

        private static SchemeObject NumericGreaterThanEqual(Environment env, List<SchemeObject> args,
            BuiltinFunction self)
        {
            var (val1, isFloat1) = RetrieveNumber(args[0], self);
            var (val2, isFloat2) = RetrieveNumber(args[1], self);

            if (isFloat1 || isFloat2)
            {
                return SchemeObject.CreateBoolean(val1 >= val2);
            }

            return SchemeObject.CreateBoolean((long) val1 >= (long) val2);
        }

        private static SchemeObject NumericLessThan(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var (val1, isFloat1) = RetrieveNumber(args[0], self);
            var (val2, isFloat2) = RetrieveNumber(args[1], self);

            if (isFloat1 || isFloat2)
            {
                return SchemeObject.CreateBoolean(val1 < val2);
            }

            return SchemeObject.CreateBoolean((long) val1 < (long) val2);
        }

        private static SchemeObject NumericLessThanEqual(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var (val1, isFloat1) = RetrieveNumber(args[0], self);
            var (val2, isFloat2) = RetrieveNumber(args[1], self);

            if (isFloat1 || isFloat2)
            {
                return SchemeObject.CreateBoolean(val1 <= val2);
            }

            return SchemeObject.CreateBoolean((long) val1 <= (long) val2);
        }

        private static SchemeObject Apply(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (!args[0].IsApplicable())
            {
                throw new SyntaxError("first argument of apply must be an application");
            }

            if (args.Count > 1 && !args.Last().IsListType())
            {
                throw new SyntaxError($"last argument of apply must be a pair: {args.Last()}");
            }

            var actualArgs = new List<SchemeObject>();
            for (var i = 1; i < args.Count - 1; ++i)
            {
                actualArgs.Add(args[i]);
            }

            if (args.Count > 1 && args.Last().Type != ObjectType.EmptyList)
            {
                var next = args.Last().Value<Pair>();
                while (true)
                {
                    actualArgs.Add(next.Car.Eval(env));
                    if (next.Cdr.Type == ObjectType.EmptyList)
                    {
                        break;
                    }

                    next = next.Cdr.Value<Pair>();
                }
            }

            var application = args[0].Value<IApplication>();
            return application.Apply(env, actualArgs);
        }

        private static SchemeObject Eval(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[1].Type != ObjectType.Environment)
            {
                throw new WrongTypeArgument(self, args[1]);
            }

            return args[0].Eval(args[1].Value<Environment>());
        }

        private static SchemeObject Load(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.String)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            var interpreter = env.Interpreter;
            if (interpreter == null)
            {
                throw new InternalException("interpreter is not set");
            }

            using var file = File.OpenRead(args[0].Value<string>());
            var reader = new StreamReader(file);
            while (true)
            {
                var expr = interpreter.Read(reader);
                if (expr == null)
                {
                    break;
                }

                expr.Eval(env);
            }

            return SchemeObject.CreateBoolean(true);
        }

        private static SchemeObject OpenInputFile(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.String)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            return SchemeObject.CreateInputPort(new StreamReader(args[0].Value<string>()));
        }

        private static SchemeObject CloseInputPort(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.InputPort)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            args[0].Value<StreamReader>().Close();
            return SchemeObject.CreateBoolean(true);
        }

        private static SchemeObject IsInputPort(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.InputPort);
        }

        private static SchemeObject OpenOutputFile(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.String)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            var file = File.Open(args[0].Value<string>(), FileMode.Open, FileAccess.Write, FileShare.None);
            return SchemeObject.CreateOutputPort(file);
        }

        private static SchemeObject CloseOutputPort(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.OutputPort)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            args[0].Value<FileStream>().Close();
            return SchemeObject.CreateBoolean(true);
        }

        private static SchemeObject IsOutputPort(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            return SchemeObject.CreateBoolean(args[0].Type == ObjectType.OutputPort);
        }

        private static SchemeObject IsEof(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (!(args[0].Type == ObjectType.InputPort || args[0].Type == ObjectType.OutputPort))
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            if (args[0].Type == ObjectType.InputPort)
            {
                var stream = args[0].Value<StreamReader>();
                return SchemeObject.CreateBoolean(stream.BaseStream.Length == stream.BaseStream.Position);
            }

            var file = args[0].Value<FileStream>();
            return SchemeObject.CreateBoolean(file.Length == file.Position);
        }

        private static StreamReader ArgToInputPort(List<SchemeObject> args, BuiltinFunction self)
        {
            switch (args.Count)
            {
                case 0:
                    return new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
                case 1:
                {
                    if (args[0].Type != ObjectType.InputPort)
                    {
                        throw new WrongTypeArgument(self, args[0]);
                    }

                    return args[0].Value<StreamReader>();
                }
                default:
                    throw new WrongNumberArguments(self, args.Count);
            }
        }


        private static SchemeObject Read(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            StreamReader stream = ArgToInputPort(args, self);
            var ret = env.Interpreter?.Read(stream);
            if (ret == null)
            {
                throw new SyntaxError("unexpected EOF");
            }

            return ret;
        }

        private static SchemeObject ReadChar(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            StreamReader stream = ArgToInputPort(args, self);
            return SchemeObject.CreateCharacter((char) stream.Read());
        }

        private static SchemeObject PeekChar(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            StreamReader stream = ArgToInputPort(args, self);
            return SchemeObject.CreateCharacter((char) stream.Peek());
        }

        private static SchemeObject Write(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            Stream stream;
            if (args.Count == 1)
            {
                stream = Console.OpenStandardOutput();
            }
            else
            {
                if (args[1].Type != ObjectType.OutputPort)
                {
                    throw new WrongTypeArgument(self, args[0]);
                }

                stream = args[1].Value<FileStream>();
            }

            var bytes = Encoding.UTF8.GetBytes(args[0].ToString());
            stream.Write(bytes, 0, bytes.Length);
            return SchemeObject.CreateBoolean(true);
        }

        private static SchemeObject WriteChar(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            if (args[0].Type != ObjectType.Character)
            {
                throw new WrongTypeArgument(self, args[0]);
            }

            Stream stream;
            if (args.Count == 1)
            {
                stream = Console.OpenStandardOutput();
            }
            else
            {
                if (args[1].Type != ObjectType.OutputPort)
                {
                    throw new WrongTypeArgument(self, args[0]);
                }

                stream = args[1].Value<FileStream>();
            }

            stream.WriteByte((byte) args[0].Value<char>());
            return SchemeObject.CreateBoolean(true);
        }

        private static SchemeObject Error(Environment env, List<SchemeObject> args, BuiltinFunction self)
        {
            var bytes = Encoding.UTF8.GetBytes(args[0].ToString());
            Console.OpenStandardError().Write(bytes, 0, bytes.Length);
            return SchemeObject.CreateBoolean(true);
        }
    }
}
