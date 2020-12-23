using System.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

public static class Printer
{
    public static string Print(object input) => 
        input switch {
            string s =>                                 $"\"{PrintString(s)}\"",
            char c =>                                   $"\\{PrintChar(c)}",
            ImmutableList<dynamic> l =>                 $"({PrintEnumerable(l)})",
            ImmutableArray<dynamic> v =>                $"[{PrintEnumerable(v)}]",
            ImmutableDictionary<dynamic, dynamic> m =>  $"{{{PrintEnumerable(m)}}}",
            ImmutableHashSet<dynamic> s =>              $"#{{{PrintEnumerable(s)}}}",
            Symbol s =>                                 s.name,
            Keyword s =>                                $":{s.name}",
            null =>                                     "nil",
            bool b =>                                   b ? "true" : "false",
            Handler h =>                                "Function",
            Closure c =>                                "Function",
            _ =>                                        $"{input}"
        };

    private static string PrintString(string s)
    {
        var sb = new StringBuilder();
        foreach(var ch in s)
            sb.Append(PrintEscape(ch));
        return sb.ToString();
    }

    private static string PrintEscape(char ch) => 
        ch switch
        {
            '\t' => @"\t",
            '\r' => @"\r",
            '\n' => @"\n",
            '\\' => @"\\",
            '"' =>  @"\""",
            _ => ch.ToString()
        };
    
    private static string PrintChar(char ch) => 
        ch switch
        {
            '\t' => "tab",
            '\n' => "newline",
            ' '  => "space",
            _ => ch.ToString()
        };

    private static string PrintEnumerable(IEnumerable<object> l) =>
        string.Join(' ', l.Select(Print));
        
    private static string PrintEnumerable(IEnumerable<KeyValuePair<object, object>> l) =>
        string.Join(' ', l.Select(PrintPair));

    private static string PrintPair(KeyValuePair<object, object> p) => $"{Print(p.Key)} {Print(p.Value)}";
}
