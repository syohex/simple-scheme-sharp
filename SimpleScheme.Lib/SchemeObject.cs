using System;
using System.Collections.Generic;
using System.Globalization;

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
        SpecialForm
    }

    public class Pair
    {
        public SchemeObject Car { get; }
        public SchemeObject Cdr { get; }

        public Pair(SchemeObject car, SchemeObject cdr)
        {
            Car = car;
            Cdr = cdr;
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
        public SchemeObject Value { get; }

        public Symbol(string name, SchemeObject value)
        {
            Name = name;
            Value = value;
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
            return CreateSymbol(name, CreateEmptyList());
        }

        internal static SchemeObject CreateSymbol(string name, SchemeObject value)
        {
            return new SchemeObject(ObjectType.Symbol, new Symbol(name, value));
        }

        public static SchemeObject CreateSpecialForm(SpecialForm form)
        {
            return new SchemeObject(ObjectType.SpecialForm, form);
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
                case ObjectType.EmptyList:
                case ObjectType.Symbol:
                    return this == obj;
                default:
                    throw new ArgumentOutOfRangeException();
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
                    return true;
                default:
                    return false;
            }
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
                    var obj = env.LookUp(symbol.Name);
                    if (obj == null)
                    {
                        throw new UnboundedVariable(symbol);
                    }

                    return obj.Value<Symbol>().Value;
                }
                case ObjectType.Pair:
                {
                    var pair = Value<Pair>();
                    if (pair.Car.Type != ObjectType.Symbol)
                    {
                        throw new Exception("unsupported error: " + pair.Car.Type);
                    }

                    var sym = pair.Car.Value<Symbol>();

                    if (pair.Cdr.Type != ObjectType.Pair)
                    {
                        throw new SyntaxError("malformed function call");
                    }

                    return sym.Value.Apply(env, pair.Cdr);
                }
                default:
                    throw new Exception($"unsupported yet: {Type}");
            }
        }

        private SchemeObject Apply(Environment env, SchemeObject args)
        {
            switch (Type)
            {
                case ObjectType.SpecialForm:
                {
                    SpecialForm form = Value<SpecialForm>();
                    var unEvaluatedArgs = args.Value<Pair>().ToList();
                    return form.Apply(env, unEvaluatedArgs);
                }
                default:
                    throw new Exception($"unsupported yet: {Type}");
            }
        }
    }
}
