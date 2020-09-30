using System;
using System.IO;
using System.Reflection;
using SimpleScheme.Lib;

namespace SimpleScheme
{
    static class Program
    {
        static void Repl(Interpreter interpreter)
        {
            Console.WriteLine("Welcome to SimpleScheme");

            var reader = new StreamReader(Console.OpenStandardInput());
            var stdout = Console.OpenStandardOutput();
            while (true)
            {
                Console.Write("> ");
                stdout.Flush();

                var expr = interpreter.Read(reader);
                if (expr == null)
                {
                    Console.WriteLine("Invalid input");
                    continue;
                }

                var value = interpreter.Eval(expr);
                Console.WriteLine(value.ToString());
            }

            // ReSharper disable once FunctionNeverReturns
        }

        static void LoadStdlib(Interpreter interpreter)
        {
            var assembly = Assembly.GetExecutingAssembly();

            var libraries = new[]
            {
                "SimpleScheme.stdlib.stdlib.scm"
            };

            foreach (var library in libraries)
            {
                using var stream = assembly.GetManifestResourceStream(library);
                if (stream == null)
                {
                    throw new Exception($"Cannot load {library}");
                }

                using var r = new StreamReader(stream);
                interpreter.EvalStream(r);
            }
        }

        static void Main(string[] args)
        {
            var interpreter = new Interpreter();
            LoadStdlib(interpreter);
            if (args.Length == 0)
            {
                Repl(interpreter);
                return;
            }

            foreach (var arg in args)
            {
                interpreter.EvalFile(arg);
            }
        }
    }
}
