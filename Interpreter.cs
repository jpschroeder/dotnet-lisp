using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public record Env(ImmutableDictionary<Symbol, dynamic> map);

public delegate object Handler(IEnumerable<dynamic> args);
public delegate object Closure(IEnumerable<dynamic> args, Env env);
public delegate (object, Env) SpecialForm(ImmutableList<dynamic> list, Env env);

public static class Interpreter
{
    public static (object, Env) Eval(object form, Env env = null)
    {
        if (env is null)
            env = DefaultEnv;

        return form switch {
            ImmutableList<dynamic> l => EvalList(l, env),
            ImmutableArray<dynamic> v => EvalVector(v, env),
            ImmutableDictionary<dynamic, dynamic> m => EvalMap(m, env),
            ImmutableHashSet<dynamic> s => EvalSet(s, env),
            Symbol s => EvalSymbol(s, env),
            _ => (form, env)
        };
    }

    public static readonly Env DefaultEnv = new Env(
        ImmutableDictionary<Symbol, dynamic>.Empty
            .Add(new Symbol("+"), (Handler)(args => args.Aggregate((s, a) => s + a)))
            .Add(new Symbol("-"), (Handler)(args => args.Aggregate((s, a) => s - a)))
            .Add(new Symbol("*"), (Handler)(args => args.Aggregate((s, a) => s * a)))
            .Add(new Symbol("/"), (Handler)(args => args.Aggregate((s, a) => s / a)))
            .Add(new Symbol(">"), (Handler)(args => IsTrue(args, (x, y) => x > y)))
            .Add(new Symbol(">="), (Handler)(args => IsTrue(args, (x, y) => x >= y)))
            .Add(new Symbol("<"), (Handler)(args => IsTrue(args, (x, y) => x < y)))
            .Add(new Symbol("<="), (Handler)(args => IsTrue(args, (x, y) => x <= y)))
            .Add(new Symbol("do"), (Handler)(args => args.Last()))
            .Add(new Symbol("list"), (Handler)(args => ImmutableList.CreateRange<dynamic>(args)))
            .Add(new Symbol("list?"), (Handler)(args => args.First() is IEnumerable<dynamic>))
            .Add(new Symbol("empty?"), (Handler)(args => !((IEnumerable<dynamic>)args.First()).Any()))
            .Add(new Symbol("count"), (Handler)(args => ((IEnumerable<dynamic>)args.First())?.Count() ?? 0))
            .Add(new Symbol("not"), (Handler)(args => !args.First()))
            .Add(new Symbol("="), (Handler)Equal)
            .Add(new Symbol("println"), (Handler)Println));

    private static object Println(IEnumerable<dynamic> args)
    {
        foreach(var arg in args)
            Console.WriteLine(arg);
        return null;
    }

    private static object Equal(IEnumerable<dynamic> args) => IsTrue(args, (x, y) => 
    {
        if (x is IEnumerable<dynamic> xl && y is IEnumerable<dynamic> yl)
            return xl.SequenceEqual(yl);
        if (x != null && y != null && x.GetType() != y.GetType())
            return false;
        else
            return x == y;
    });

    private static object IsTrue(IEnumerable<dynamic> args, Func<dynamic, dynamic, bool> test)
    {
        var a = args.ToList();
        for(int i = 0; i < a.Count - 1; i++)
            if (!test(a[i], a[i + 1]))
                return false;
        return true;
    }

    private static readonly ImmutableDictionary<Symbol, SpecialForm> SpecialForms = 
        ImmutableDictionary<Symbol, SpecialForm>.Empty
            .Add(new Symbol("def"), EvalDef)
            .Add(new Symbol("let"), EvalLet)
            .Add(new Symbol("fn"), EvalFn)
            .Add(new Symbol("if"), EvalIf);

    private static (object, Env) EvalSymbol(Symbol s, Env env)
    {
        if(!env.map.ContainsKey(s))
            throw new ApplicationException($"Unable to resolve symbol: {s.name} in this context");

        return (env.map[s], env);
    }

    private static (object, Env) EvalList(ImmutableList<dynamic> list, Env env)
    {
        if (list.IsEmpty)
            return (list, env);

        if (list.First() is Symbol s && SpecialForms.ContainsKey(s))
            return SpecialForms[s](list, env);
        
        var (newlist, newenv) = EvalEnumerable(list, env);
        var first = newlist.First();
        var rest = newlist.Skip(1);

        return first switch
        {
            Handler handler => (handler(rest), newenv),
            Closure closure => (closure(rest, newenv), newenv),
            _ =>  throw new ApplicationException("Invalid function")
        };
    }

    private static (object, Env) EvalDef(ImmutableList<dynamic> list, Env env)
    {
        if (list.Count != 3)
            throw new ApplicationException("Invalid number of parameters for def");

        if (list[1] is not Symbol)
            throw new ApplicationException("Invalid def parameter");
        
        return EvalDef(list[1], list[2], env);
    }

    private static (object, Env) EvalDef(dynamic variable, dynamic value, Env env)
    {
        (object form, Env env) context = Eval(value, env);
        return (
            context.form, 
            new Env(context.env.map.SetItem(variable, context.form)));
    }

    private static (object, Env) EvalIf(ImmutableList<dynamic> list, Env env)
    {
        if (list.Count > 4 || list.Count < 3)
            throw new ApplicationException("Invalid number of parameters for if");

        (object form, Env env) test = Eval(list[1], env);

        if ((test.form is bool b && b) ||
            (test.form is IEnumerable<dynamic> o && o is not null))
            return Eval(list[2], test.env);
        else if (list.Count == 4)
            return Eval(list[3], test.env);
        else
            return (null, test.env);
    }

    private static (object, Env) EvalFn(ImmutableList<dynamic> list, Env defEnv)
    {
        if (list.Count < 3)
            throw new ApplicationException("Invalid number of parameters for fn");
        
        if (list[1] is not ImmutableArray<dynamic> bindings)
            throw new ApplicationException("Invalid parameters for fn");

        Closure closure = (a, callEnv) => 
        {
            var args = a.ToList();

            // defEnv -> the environment in which the function was defined
            // callEnv -> the environment in which the function was called
            (object form, Env env) context = (null, new Env(callEnv.map.AddRange(defEnv.map)));

            for(int i = 0; i < bindings.Count(); i++)
            {
                if (bindings[i] is Symbol s && s == new Symbol("&"))
                {
                    // Bind variable length arguments
                    context = EvalDef(bindings[i + 1], args.Skip(i), context.env);
                    break;
                }
                // Bind regular arguments
                context = EvalDef(bindings[i], args[i], context.env);
            }

            return EvalLast(list.Skip(2), context.env);
        };

        return (closure, defEnv);
    }

    private static (object, Env) EvalLet(ImmutableList<dynamic> list, Env env)
    {
        if (list.Count < 3)
            throw new ApplicationException("Invalid number of parameters for let");

        if (list[1] is not ImmutableArray<dynamic> bindings)
            throw new ApplicationException("Invalid let bindings");
        
        if (bindings.Length % 2 != 0)
            throw new ApplicationException("Invalid number of bindings for let");

        (object form, Env env) context = (null, env);
        for(var i = 0; i < bindings.Length; i+=2)
        {
            if (bindings[i] is not Symbol)
                throw new ApplicationException($"Invalid let symbol: {bindings[i]}");
            
            context = EvalDef(bindings[i], bindings[i+1], context.env);
        }

        return (EvalLast(list.Skip(2), context.env), env);
    }

    private static (object, Env) EvalVector(ImmutableArray<dynamic> v, Env env)
    {
        var (result, newenv) = EvalEnumerable(v, env);
        return (ImmutableArray.CreateRange<dynamic>(result), newenv);
    }

    private static (object, Env) EvalSet(ImmutableHashSet<dynamic> s, Env env)
    {
        var (result, newenv) = EvalEnumerable(s, env);
        return (ImmutableHashSet.CreateRange<dynamic>(result), newenv);
    }

    private static (IEnumerable<dynamic>, Env) EvalEnumerable(IEnumerable<dynamic> list, Env env)
    {
        (object form, Env env) context = (null, env);
        var forms = new List<dynamic>();
        foreach(var l in list)
        {
            context = Eval(l, context.env);
            forms.Add(context.form);
        }
        return (forms, context.env);
    }

    private static dynamic EvalLast(IEnumerable<dynamic> list, Env env)
    {
        (object form, Env env) context = (null, env);
        foreach(var l in list)
            context = Eval(l, context.env);
        return context.form;
    }

    private static (ImmutableDictionary<dynamic, dynamic>, Env) EvalMap(ImmutableDictionary<dynamic, dynamic> map, Env env)
    {
        (object form, Env env) context = (null, env);
        var ret = ImmutableDictionary.CreateBuilder<dynamic, dynamic>();
        foreach(var p in map)
        {
            context = Eval(p.Key, context.env);
            var key = context.form;
            context = Eval(p.Value, context.env);
            var value = context.form;
            ret.Add(key, value);
        }
        return (ret.ToImmutable(), context.env);
    }
}
