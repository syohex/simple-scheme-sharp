using System;

namespace SimpleScheme.Lib
{
    public class InternalException : Exception
    {
        public InternalException(string message) : base(message)
        {
        }
    }

    public class UnsupportedDataType : Exception
    {
        public UnsupportedDataType(string message) : base(message)
        {
        }
    }

    public class SyntaxError : Exception
    {
        public SyntaxError(string message) : base(message)
        {
        }
    }

    public class WrongNumberArguments : Exception
    {
        public WrongNumberArguments(Callable form, int got) : base(ExceptionMessage(form, got))
        {
        }

        private static string ExceptionMessage(Callable form, int got)
        {
            if (form.Variadic)
            {
                return $"wrong number of arguments for #<special {form.Name} (required >= {form.Arity}, got {got})>";
            }

            return $"wrong number of arguments for #<special {form.Name} (required {form.Arity}, got {got})>";
        }
    }

    public class UnboundedVariable : Exception
    {
        public UnboundedVariable(Symbol symbol) : base(ExceptionMessage(symbol))
        {
        }

        private static string ExceptionMessage(Symbol symbol)
        {
            return $"unbound variable: {symbol.Name}";
        }
    }

    public class SymbolNotDefined : Exception
    {
        public SymbolNotDefined(string name) : base($"symbol '{name}' is not defined")
        {
        }
    }

    public class WrongTypeArgument : Exception
    {
        public WrongTypeArgument(Callable func, SchemeObject arg) : base(ExceptionMessage(func, arg))
        {
        }

        private static string ExceptionMessage(Callable func, SchemeObject arg)
        {
            return $"function {func.Name} does not support {arg.Type} argument type";
        }
    }

    public class RuntimeException : Exception
    {
        public RuntimeException(string message) : base(message)
        {
        }
    }
}
