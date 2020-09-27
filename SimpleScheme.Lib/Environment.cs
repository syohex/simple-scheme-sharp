using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('(');
            foreach (var bind in _bindings)
            {
                sb.Append($"({bind.Name} . {bind.Value})");
            }

            sb.Append(')');
            return sb.ToString();
        }
    }

    public class Frame
    {
        private readonly List<Bindings> _bindings;

        public Frame(Bindings bindings)
        {
            _bindings = new List<Bindings>() {bindings};
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

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('(');
            foreach (var binding in _bindings)
            {
                sb.Append($"  {binding}\n");
            }

            sb.Append(")\n");
            return sb.ToString();
        }
    }

    public class Environment
    {
        public SymbolTable GlobalTable { get; }
        private List<Frame> Frames { get; set; }
        private Environment? _environment;

        public Environment(SymbolTable table)
        {
            GlobalTable = table;
            Frames = new List<Frame>();
            _environment = null;
        }

        public void Dump()
        {
            Console.WriteLine("Frames");
            for (var i = 0; i < Frames.Count; ++i)
            {
                Console.WriteLine($"Scope {i}");
                Console.WriteLine(Frames[i]);
            }
        }

        public SchemeObject Intern(string name)
        {
            return GlobalTable.Intern(name);
        }

        public Environment Copy()
        {
            return new Environment(GlobalTable) {Frames = new List<Frame>(Frames)};
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

            return GlobalTable.LookUpValue(name);
        }

        public SchemeObject Define(SchemeObject symbol, SchemeObject value)
        {
            var sym = symbol.Value<Symbol>();
            if (Frames.Count == 0) // define as global variable
            {
                GlobalTable.RegisterValue(symbol, value);
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

            if (GlobalTable.LookUpSymbol(sym.Name) == null)
            {
                throw new SymbolNotDefined(sym.Name);
            }

            GlobalTable.RegisterValue(symbol, value);
            return value;
        }
    }
}
