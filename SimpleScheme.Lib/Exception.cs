using System;

namespace SimpleScheme.Lib
{
    public class InternalException : Exception
    {
        public InternalException()
        {
        }

        public InternalException(string message) : base(message)
        {
        }

        public InternalException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class UnsupportedDataType : Exception
    {
        public UnsupportedDataType()
        {
        }

        public UnsupportedDataType(string message) : base(message)
        {
        }

        public UnsupportedDataType(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class SyntaxError : Exception
    {
        public SyntaxError()
        {
        }

        public SyntaxError(string message) : base(message)
        {
        }

        public SyntaxError(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
