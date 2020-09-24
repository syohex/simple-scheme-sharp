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
        EmptyList
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
            return new SchemeObject(ObjectType.EmptyList, null);
        }

        public T Value<T>()
        {
            return (T) _value;
        }

        public bool Equal(SchemeObject obj)
        {
            if (obj == null)
            {
                return false;
            }

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
