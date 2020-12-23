using System;

public record Keyword(string name);
public record Symbol(string name);

public static class Lisp
{
    public static void Repl()
    {
        Action prompt = Console.IsInputRedirected ? Noop : Prompt;
        prompt();
        var env = Interpreter.DefaultEnv;
        while(true)
        {
            try
            {
                var form = Reader.Read(Console.In);
                var (result, newenv) = Interpreter.Eval(form, env);
                var output = Printer.Print(result);

                env = newenv;
                Console.WriteLine(output);
                if (result is Keyword c && c == Reader.Eof)
                    break;
                if (Reader.IsNewLine(Console.In.Peek()))
                    prompt();
            }
            catch(ApplicationException e)
            {
                Console.WriteLine($"Error: {e.Message}");
                prompt();
            }
        }
    }

    public static void Prompt() => Console.Write("user=> ");
    public static void Noop() { }
}
