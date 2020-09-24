namespace SimpleScheme.Lib
{
    public enum ObjectType
    {
        Fixnum,
        Float,
        String,
        Boolean,
        Character
    }

    public class SchemeObject
    {
        private readonly object _value;

        private SchemeObject(ObjectType type, object value)
        {
            Type = type;
            _value = value;
        }

        public ObjectType Type { get; }

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

        public T Value<T>()
        {
            return (T)_value;
        }
    }
}
