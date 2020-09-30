using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SimpleScheme.Lib
{
    public enum ObjectType
    {
        Fixnum,
        Float,
        String,
        Boolean,
        Character,
        EmptyList,
        Pair,
        Symbol,
        Undefined,
        SpecialForm,
        BuiltinFunction,
        Closure,
        Environment,
        InputPort,
        OutputPort,
    }

    public class Pair
    {
        public SchemeObject Car { get; set; }
        public SchemeObject Cdr { get; set; }

        public Pair(SchemeObject car, SchemeObject cdr)
        {
            Car = car;
            Cdr = cdr;
        }

        public int Length()
        {
            var next = this;
            int len = 0;
            while (true)
            {
                if (next.Cdr.Type == ObjectType.EmptyList)
                {
                    return len;
                }

                ++len;

                if (next.Cdr.Type != ObjectType.Pair)
                {
                    throw new InternalException("dotted pair cannot convert into listed");
                }

                next = next.Cdr.Value<Pair>();
            }
        }

        public SchemeObject Nth(long index)
        {
            var next = this;
            int len = 0;
            while (true)
            {
                if (next.Cdr.Type == ObjectType.EmptyList)
                {
                    throw new RuntimeException("passed value larger than list length");
                }

                if (len == index)
                {
                    return next.Car;
                }

                ++len;
                if (next.Cdr.Type != ObjectType.Pair)
                {
                    throw new InternalException("dotted pair cannot convert into listed");
                }

                next = next.Cdr.Value<Pair>();
            }
        }

        public List<SchemeObject> ToList()
        {
            var args = new List<SchemeObject>();
            var next = this;
            while (true)
            {
                args.Add(next.Car);
                if (next.Cdr.Type == ObjectType.EmptyList)
                {
                    break;
                }

                if (next.Cdr.Type != ObjectType.Pair)
                {
                    throw new InternalException("dotted pair cannot convert into listed");
                }

                next = next.Cdr.Value<Pair>();
            }

            return args;
        }

        public override string ToString()
        {
            string carStr = Car.ToString();
            switch (Cdr.Type)
            {
                case ObjectType.Pair:
                {
                    var rest = Cdr.Value<Pair>();
                    return $"{carStr} {rest}";
                }
                case ObjectType.EmptyList:
                    return carStr;
                default:
                {
                    string cdrStr = Cdr.ToString();
                    return $"{carStr} . {cdrStr}";
                }
            }
        }
    }

    public class Symbol
    {
        public string Name { get; }

        public Symbol(string name)
        {
            Name = name;
        }
    }

    public class SchemeObject
    {
        public ObjectType Type { get; }
        private readonly object _value;

        private SchemeObject(ObjectType type, object value)
        {
            Type = type;
            _value = value;
        }

        public static SchemeObject CreateFixnum(long value)
        {
            return new SchemeObject(ObjectType.Fixnum, value);
        }

        public static SchemeObject CreateFloat(double value)
        {
            return new SchemeObject(ObjectType.Float, value);
        }

        public static SchemeObject CreateBoolean(bool value)
        {
            return new SchemeObject(ObjectType.Boolean, value);
        }

        public static SchemeObject CreateCharacter(char value)
        {
            return new SchemeObject(ObjectType.Character, value);
        }

        public static SchemeObject CreateString(string value)
        {
            return new SchemeObject(ObjectType.String, value);
        }

        public static SchemeObject CreateEmptyList()
        {
            return new SchemeObject(ObjectType.EmptyList, 0); // dummy value
        }

        public static SchemeObject CreatePair(SchemeObject car, SchemeObject cdr)
        {
            return new SchemeObject(ObjectType.Pair, new Pair(car, cdr));
        }

        internal static SchemeObject CreateSymbol(string name)
        {
            return new SchemeObject(ObjectType.Symbol, new Symbol(name));
        }

        public static SchemeObject CreateSpecialForm(SpecialForm form)
        {
            return new SchemeObject(ObjectType.SpecialForm, form);
        }

        public static SchemeObject CreateBuiltinFunction(BuiltinFunction func)
        {
            return new SchemeObject(ObjectType.BuiltinFunction, func);
        }

        public static SchemeObject CreateClosure(Closure closure)
        {
            return new SchemeObject(ObjectType.Closure, closure);
        }

        public static SchemeObject CreateUndefined()
        {
            return new SchemeObject(ObjectType.Undefined, -1); // dummy value
        }

        public static SchemeObject CreateEnvironment(SymbolTable globalTable)
        {
            return new SchemeObject(ObjectType.Environment, new Environment(globalTable, null));
        }

        public static SchemeObject CreateInputPort(StreamReader value)
        {
            return new SchemeObject(ObjectType.InputPort, value);
        }

        public static SchemeObject CreateOutputPort(FileStream value)
        {
            return new SchemeObject(ObjectType.OutputPort, value);
        }

        public T Value<T>()
        {
            return (T) _value;
        }

        public bool Equal(SchemeObject obj)
        {
            switch (Type)
            {
                case ObjectType.Fixnum:
                    return Value<long>() == obj.Value<long>();
                case ObjectType.Float:
                    return Math.Abs(Value<double>() - obj.Value<double>()) < 1e-10;
                case ObjectType.String:
                    return Value<string>() == obj.Value<string>();
                case ObjectType.Character:
                    return Value<char>() == obj.Value<char>();
                case ObjectType.Boolean:
                    return Value<bool>() == obj.Value<bool>();
                case ObjectType.Symbol:
                case ObjectType.SpecialForm:
                case ObjectType.BuiltinFunction:
                case ObjectType.Environment:
                    return this == obj;
                case ObjectType.EmptyList:
                case ObjectType.Undefined:
                    return Type == obj.Type;
                default:
                    throw new InternalException($"type '{Type}' is not comparable");
            }
        }

        public override string ToString()
        {
            switch (Type)
            {
                case ObjectType.Fixnum:
                    return Value<long>().ToString();
                case ObjectType.Float:
                    return Value<double>().ToString(CultureInfo.InvariantCulture);
                case ObjectType.String:
                    return $"\"{Value<string>()}\"";
                case ObjectType.Character:
                    return $"#\\{Value<char>()}";
                case ObjectType.Boolean:
                    return Value<bool>() ? "#t" : "#f";
                case ObjectType.EmptyList:
                    return "()";
                case ObjectType.Pair:
                    return $"({Value<Pair>()})";
                case ObjectType.Symbol:
                    return Value<Symbol>().Name;
                case ObjectType.SpecialForm:
                {
                    var form = Value<SpecialForm>();
                    return $"#<special {form.Name}>";
                }
                case ObjectType.BuiltinFunction:
                {
                    var func = Value<BuiltinFunction>();
                    return $"#<builtin {func.Name}>";
                }
                case ObjectType.Closure:
                {
                    var closure = Value<Closure>();
                    var name = closure.Name ?? "#f";
                    return $"#<closure {name}>";
                }
                case ObjectType.Undefined:
                    return "#<undef>";
                case ObjectType.Environment:
                    return "#<environment>";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool IsSelfEvaluated()
        {
            switch (Type)
            {
                case ObjectType.Fixnum:
                case ObjectType.Float:
                case ObjectType.String:
                case ObjectType.Boolean:
                case ObjectType.Character:
                case ObjectType.EmptyList:
                case ObjectType.Undefined:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsListType()
        {
            switch (Type)
            {
                case ObjectType.EmptyList:
                case ObjectType.Pair:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsApplicable()
        {
            switch (Type)
            {
                case ObjectType.BuiltinFunction:
                case ObjectType.Closure:
                    return true;
                default:
                    return false;
            }
        }

        public bool IsTrue()
        {
            if (Type != ObjectType.Boolean)
            {
                return true;
            }

            return Value<bool>();
        }

        public bool IsUndefined()
        {
            return Type == ObjectType.Undefined;
        }

        private SchemeObject EvalLambda(Environment env, SchemeObject expr)
        {
            if (expr.Type != ObjectType.Pair)
            {
                throw new SyntaxError($"invalid application {this}");
            }

            var pair = expr.Value<Pair>();
            var firstArg = pair.Car.Value<Pair>();
            if (firstArg.Car.Type != ObjectType.Symbol)
            {
                throw new SyntaxError($"invalid application {this}");
            }

            var sym = firstArg.Car.Value<Symbol>();
            if (sym.Name != "lambda")
            {
                throw new SyntaxError($"invalid application {this}");
            }

            var closure = pair.Car.Eval(env);
            if (closure.Type != ObjectType.Closure)
            {
                throw new InternalException("evaluated value of lambda is not closure");
            }

            return closure.Apply(env, pair.Cdr);
        }

        public SchemeObject Eval(Environment env)
        {
            if (IsSelfEvaluated())
            {
                return this;
            }

            switch (Type)
            {
                case ObjectType.Symbol:
                {
                    var symbol = Value<Symbol>();
                    var value = env.LookUpValue(symbol.Name);
                    if (value == null)
                    {
                        throw new UnboundedVariable(symbol);
                    }

                    return value;
                }
                case ObjectType.Pair:
                {
                    var pair = Value<Pair>();
                    switch (pair.Car.Type)
                    {
                        case ObjectType.Symbol:
                        {
                            var sym = pair.Car.Value<Symbol>();
                            if (!(pair.Cdr.Type == ObjectType.Pair || pair.Cdr.Type == ObjectType.EmptyList))
                            {
                                throw new SyntaxError("malformed function call");
                            }

                            var application = env.LookUpValue(sym.Name);
                            if (application == null)
                            {
                                throw new SyntaxError($"{application} is not an application");
                            }

                            if (application.Type == ObjectType.Pair)
                            {
                                return EvalLambda(env, application);
                            }

                            return application.Apply(env, pair.Cdr);
                        }
                        case ObjectType.Pair:
                            return EvalLambda(env, this);
                        default:
                            throw new SyntaxError($"cannot evaluated: {this}");
                    }
                }
                default:
                    throw new SyntaxError($"cannot evaluated: {this}");
            }
        }

        private static List<SchemeObject> EvalArguments(Environment env, SchemeObject args)
        {
            var ret = new List<SchemeObject>();
            var next = args;
            while (true)
            {
                if (next.Type == ObjectType.EmptyList)
                {
                    break;
                }

                var pair = next.Value<Pair>();
                ret.Add(pair.Car.Eval(env));

                next = pair.Cdr;
            }

            return ret;
        }

        private SchemeObject Apply(Environment env, SchemeObject args)
        {
            switch (Type)
            {
                case ObjectType.SpecialForm:
                {
                    var form = Value<SpecialForm>();
                    List<SchemeObject> unEvaluatedArgs;
                    if (args.Type == ObjectType.EmptyList)
                    {
                        unEvaluatedArgs = new List<SchemeObject>();
                    }
                    else
                    {
                        unEvaluatedArgs = args.Value<Pair>().ToList();
                    }

                    return form.Apply(env, unEvaluatedArgs);
                }
                case ObjectType.BuiltinFunction:
                {
                    var func = Value<BuiltinFunction>();
                    List<SchemeObject> evaluatedArgs = EvalArguments(env, args);
                    return func.Apply(env, evaluatedArgs);
                }
                case ObjectType.Closure:
                {
                    var closure = Value<Closure>();
                    List<SchemeObject> evaluatedArgs = EvalArguments(env, args);
                    return closure.Apply(env, evaluatedArgs);
                }
                default:
                    throw new Exception($"unsupported yet: {Type}: {this}");
            }
        }
    }
}
