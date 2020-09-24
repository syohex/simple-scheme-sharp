namespace SimpleScheme.Lib
{
    public enum ObjectType
    {
        Fixnum,
        Float,
        String
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

        public T Value<T>()
        {
            return (T)_value;
        }
    }
}
