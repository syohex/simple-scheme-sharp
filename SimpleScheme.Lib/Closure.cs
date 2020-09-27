using System.Collections.Generic;

namespace SimpleScheme.Lib
{
    public class Closure : Callable, IApplication
    {
        private readonly List<SchemeObject> _params;
        private readonly List<SchemeObject> _body;
        private readonly Environment _environment;

        public Closure(string? name, List<SchemeObject> param, List<SchemeObject> body, Environment env)
            : base(name, param.Count, false)
        {
            _params = param;
            _body = body;
            _environment = env.Copy();
        }

        public SchemeObject Apply(Environment env, List<SchemeObject> actualArgs)
        {
            if (_params.Count != actualArgs.Count)
            {
                throw new WrongNumberArguments(this, actualArgs.Count);
            }

            var bindings = new Bindings();
            for (var i = 0; i < _params.Count; ++i)
            {
                bindings.AddBinding(_params[i].Value<Symbol>().Name, actualArgs[i]);
            }

            Frame argFrame = new Frame(bindings);
            env.PushFrame(argFrame);
            env.SetEnvironment(_environment);

            SchemeObject ret = SchemeObject.CreateUndefined();
            foreach (var expr in _body)
            {
                ret = expr.Eval(env);
            }

            env.ResetEnvironment();
            env.PopFrame();
            return ret;
        }
    }
}
