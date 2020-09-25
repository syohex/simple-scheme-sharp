using System;
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
        Symbol
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

        public static SchemeObject CreateSymbol(string name)
        {
            return new SchemeObject(ObjectType.Symbol, name);
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
                {
                    return $"({Value<Pair>()})";
                }
                case ObjectType.Symbol:
                    return Value<string>();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
