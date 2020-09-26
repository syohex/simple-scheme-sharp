using System.Collections.Generic;
using System.Linq;

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

    internal class BindPair
    {
        public string Name { get; }
        public SchemeObject Value { get; }

        public BindPair(string name, SchemeObject value)
        {
            Name = name;
            Value = value;
        }
    }

    public class Frame
    {
        private List<BindPair> _bindings;

        public Frame()
        {
            _bindings = new List<BindPair>();
        }

        public void AddBinding(string name, SchemeObject obj)
        {
            _bindings.Add(new BindPair(name, obj));
        }

        public SchemeObject? LookUp(string name)
        {
            foreach (var binding in _bindings)
            {
                if (name == binding.Name)
                {
                    return binding.Value;
                }
            }

            return null;
        }
    }

    public class Environment
    {
        private readonly SymbolTable _globalTable;
        private List<Frame> _frames;

        public Environment(SymbolTable table)
        {
            _globalTable = table;
            _frames = new List<Frame>();
        }

        public void PushFrame(Frame frame)
        {
            _frames.Add(frame);
        }

        public void PopFrame(Frame frame)
        {
            _frames.RemoveAt(0);
        }

        public SchemeObject? LookUp(string name)
        {
            foreach (var frame in _frames)
            {
                var value = frame.LookUp(name);
                if (value != null)
                {
                    return value;
                }
            }

            return _globalTable.LookUp(name);
        }

        public SchemeObject Define(SchemeObject symbol, SchemeObject value)
        {
            var sym = symbol.Value<Symbol>();
            if (_frames.Count == 0) // define as global variable
            {
                sym.Value = value;
                _globalTable.RegisterSymbol(symbol);
                return symbol;
            }

            // define as local variable
            _frames.First().AddBinding(sym.Name, value);
            return symbol;
        }

        public SchemeObject Set(SchemeObject symbol, SchemeObject value)
        {
            var sym = symbol.Value<Symbol>();
            foreach (var frame in _frames)
            {
                var obj = frame.LookUp(sym.Name);
                if (obj != null)
                {
                    var s = obj.Value<Symbol>();
                    s.Value = value;
                    return value;
                }
            }

            var globalObj = _globalTable.LookUp(sym.Name);
            if (globalObj == null)
            {
                throw new SymbolNotDefined(sym.Name);
            }

            globalObj.Value<Symbol>().Value = value;
            return value;
        }
    }
}
