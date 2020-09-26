using System.Collections.Generic;
using System.Linq;

namespace SimpleScheme.Lib
{
    public class SymbolTable
    {
        private readonly Dictionary<SchemeObject, SchemeObject> _table;

        public SymbolTable()
        {
            _table = new Dictionary<SchemeObject, SchemeObject>();
        }

        public SchemeObject? LookUpSymbol(string name)
        {
            foreach (KeyValuePair<SchemeObject, SchemeObject> entry in _table)
            {
                if (entry.Key.Value<Symbol>().Name == name)
                {
                    return entry.Key;
                }
            }

            return null;
        }

        public SchemeObject? LookUpValue(string name)
        {
            foreach (KeyValuePair<SchemeObject, SchemeObject> entry in _table)
            {
                if (entry.Key.Value<Symbol>().Name == name)
                {
                    return entry.Value;
                }
            }

            return null;
        }

        public void RegisterValue(SchemeObject name, SchemeObject value)
        {
            _table[name] = value;
        }

        public SchemeObject Intern(string name)
        {
            SchemeObject? obj = LookUpSymbol(name);
            if (obj != null)
            {
                return obj;
            }

            SchemeObject sym = SchemeObject.CreateSymbol(name);
            RegisterValue(sym, SchemeObject.CreateUndefined());
            return sym;
        }
    }

    internal class BindPair
    {
        public string Name { get; }
        public SchemeObject Value { get; set; }

        public BindPair(string name, SchemeObject value)
        {
            Name = name;
            Value = value;
        }
    }

    public class Bindings
    {
        private List<BindPair> _bindings;

        public Bindings()
        {
            _bindings = new List<BindPair>();
        }

        public void AddBinding(string name, SchemeObject obj)
        {
            _bindings.Add(new BindPair(name, obj));
        }

        public SchemeObject? Lookup(string name)
        {
            foreach (BindPair binding in _bindings)
            {
                if (name == binding.Name)
                {
                    return binding.Value;
                }
            }

            return null;
        }

        public bool UpdateIfExists(string name, SchemeObject value)
        {
            foreach (BindPair binding in _bindings)
            {
                if (name == binding.Name)
                {
                    binding.Value = value;
                    return true;
                }
            }

            return false;
        }
    }

    public class Frame
    {
        private readonly List<Bindings> _bindings;

        public Frame()
        {
            _bindings = new List<Bindings>();
        }

        public Frame(Bindings bindings)
        {
            _bindings = new List<Bindings>(){bindings};
        }

        public void AddBindings(Bindings bindings)
        {
            _bindings.Add(bindings);
        }

        public void AddNewBinding(string name, SchemeObject value)
        {
            if (_bindings.Count == 0)
            {
                throw new InternalException("no bindings");
            }

            _bindings.First().AddBinding(name, value);
        }

        public SchemeObject? LookUpValue(string name)
        {
            foreach (var bindings in _bindings)
            {
                var value = bindings.Lookup(name);
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }
        public bool UpdateIfExists(string name, SchemeObject value)
        {
            return _bindings.Any(bindings => bindings.UpdateIfExists(name, value));
        }
    }

    public class Environment
    {
        private readonly SymbolTable _globalTable;
        private List<Frame> Frames { get; set; }
        private Environment? _environment;

        public Environment(SymbolTable table)
        {
            _globalTable = table;
            Frames = new List<Frame>();
            _environment = null;
        }

        public SchemeObject Intern(string name)
        {
            return _globalTable.Intern(name);
        }

        public Environment Copy()
        {
            return new Environment(_globalTable) {Frames = new List<Frame>()};
        }

        public void PushFrame(Frame frame)
        {
            Frames.Insert(0, frame);
        }

        public void PopFrame()
        {
            Frames.RemoveAt(0);
        }

        public void SetEnvironment(Environment otherEnv)
        {
            _environment = otherEnv;
        }

        public void ResetEnvironment()
        {
            _environment = null;
        }

        private SchemeObject? LookUpValueFromFrames(string name)
        {
            foreach (var frame in Frames)
            {
                var value = frame.LookUpValue(name);
                if (value != null)
                {
                    return value;
                }
            }

            return null;
        }

        public SchemeObject? LookUpValue(string name)
        {
            var obj = LookUpValueFromFrames(name);
            if (obj != null)
            {
                return obj;
            }

            if (_environment != null)
            {
                obj = _environment.LookUpValueFromFrames(name);
                if (obj != null)
                {
                    return obj;
                }
            }

            return _globalTable.LookUpValue(name);
        }

        public SchemeObject Define(SchemeObject symbol, SchemeObject value)
        {
            var sym = symbol.Value<Symbol>();
            if (Frames.Count == 0) // define as global variable
            {
                _globalTable.RegisterValue(symbol, value);
                return symbol;
            }

            // define as local variable
            Frames.First().AddNewBinding(sym.Name, value);
            return symbol;
        }

        public SchemeObject Set(SchemeObject symbol, SchemeObject value)
        {
            var sym = symbol.Value<Symbol>();
            if (Frames.Any(frame => frame.UpdateIfExists(sym.Name, value)))
            {
                return value;
            }

            if (_globalTable.LookUpSymbol(sym.Name) == null)
            {
                throw new SymbolNotDefined(sym.Name);
            }

            _globalTable.RegisterValue(symbol, value);
            return value;
        }
    }
}
