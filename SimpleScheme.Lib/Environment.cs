using System.Collections.Generic;

namespace SimpleScheme.Lib
{
    public class SymbolTable
    {
        private readonly Dictionary<string, SchemeObject> _table;

        public SymbolTable()
        {
            _table = new Dictionary<string, SchemeObject>();
        }

        public SchemeObject? LookUp(string name)
        {
            if (_table.TryGetValue(name, out SchemeObject symbol))
            {
                return symbol;
            }

            return null;
        }

        public void RegisterSymbol(SchemeObject symbol)
        {
            if (symbol.Type != ObjectType.Symbol)
            {
                throw new UnsupportedDataType("passed not symbol type argument");
            }

            _table[symbol.Value<Symbol>().Name] = symbol;
        }
    }

    public class Environment
    {
        private readonly SymbolTable _globalTable;

        public Environment(SymbolTable table)
        {
            _globalTable = table;
        }

        public SchemeObject? LookUp(string name)
        {
            return _globalTable.LookUp(name);
        }
    }
}
