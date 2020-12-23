using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

public static class Reader
{
    public static readonly Keyword Eof = new Keyword("eof");

    public static object Read(string s)
    {
        using var r = new StringReader(s);
        return Read(r);
    }

    public static object Read(TextReader r)
    {
        int ch = ReadNonDiscard(r);
        if (IsEof(ch))
            return Eof;
        var form = ReadForm(r, ch);
        ReadLineDiscards(r);
        return form;
    }

    private static object ReadForm(TextReader r, int ch)
    {
        if (IsEof(ch)) 
            throw new ApplicationException("EOF while reading");

        if (IsDigit(ch)) 
            return ReadNumber(r, ch);

        if ((ch == '+' || ch == '-') && IsDigit(r.Peek()))
            return ReadNumber(r, ch);

        if (Macros.ContainsKey(ch))
            return Macros[ch](r, ch);

        var token = ReadToken(r, ch);

        if (token == "nil")
            return null;
        if (token == "true")
            return true;
        if (token == "false")
            return false;

        return new Symbol(token);
    }

    private delegate object Macro(TextReader r, int initch);

    private static readonly Dictionary<int, Macro> Macros = new Dictionary<int, Macro>
    {
        { '\"', ReadString },
        { '\\', ReadCharacter },
        { '(', ReadList },
        { ')', ReadUnmatchedDelimiter },
        { '[', ReadVector },
        { ']', ReadUnmatchedDelimiter },
        { '{', ReadMap },
        { '}', ReadUnmatchedDelimiter },
        { ':', ReadKeyword },
        { '#', ReadDispatch },
    };

    private static readonly Dictionary<int, Macro> DispatchMacros = new Dictionary<int, Macro>
    {
        { '{', ReadSet },
    };

    private static bool IsMacro(int ch) => Macros.ContainsKey(ch) || ch == ';';
    private static bool IsEof(int ch) => ch == -1;
    private static bool IsWhiteSpace(int ch) => !IsEof(ch) && (char.IsWhiteSpace((char)ch) || ch == ',');
    private static bool IsDigit(int ch) => !IsEof(ch) && char.IsDigit((char)ch);
    public static bool IsNewLine(int ch) => ch == '\n';

    private static object ReadNumber(TextReader r, int initch)
    {
        var s = ReadToken(r, initch);

        if (int.TryParse(s, out var integer))
            return integer;

        if (double.TryParse(s, out var floating))
            return floating;

        throw new ApplicationException($"Invalid number: {s}");
    }

    private static object ReadString(TextReader r, int initch)
    {
        var sb = new StringBuilder();
        for(int ch = r.Read(); ch != '"'; ch = r.Read())
        {
            if(IsEof(ch))
                throw new ApplicationException("EOF while reading string");
            
            if (ch == '\\')
                sb.Append(ReadEscape(r, ch));
            else
                sb.Append((char)ch);
        }
        return sb.ToString();
    }

    private static char ReadEscape(TextReader r, int initch)
    {
        int ch = r.Read();
        if(IsEof(ch))
            throw new ApplicationException("EOF while reading string");
        return ch switch
        {
            't' => '\t',
            'r' => '\r',
            'n' => '\n',
            '\\' => '\\',
            '"' => '"',
            _ => throw new ApplicationException($"Unsupported escape character: {(char)ch}")
        };
    }

    private static object ReadCharacter(TextReader r, int initch)
    {
        var str = ReadToken(r, r.Read());
        return str switch
        {
            string s when s.Length == 1 => str[0],
            "tab" => '\t',
            "newline" => '\n',
            "space" => ' ',
            _ => throw new ApplicationException($"Unsupported character: {str}")
        };
    }

    private static object ReadList(TextReader r, int initch) =>
        ImmutableList.CreateRange<dynamic>(ReadDelimitedList(')', r, initch));

    private static object ReadVector(TextReader r, int initch) =>
        ImmutableArray.CreateRange<dynamic>(ReadDelimitedList(']', r, initch));

    private static IEnumerable<object> ReadDelimitedList(char delim, TextReader r, int initch)
    {
        while(true)
        {
            int ch = ReadNonDiscard(r);
            if (ch == delim)
                break;

            yield return ReadForm(r, ch);
        }
    }

    private static object ReadMap(TextReader r, int initch)
    {
        const int delim = '}';
        var builder = ImmutableDictionary.CreateBuilder<dynamic, dynamic>();
        while(true)
        {
            int chkey = ReadNonDiscard(r);
            if (chkey == delim)
                break;
            var key = ReadForm(r, chkey);

            int chval = ReadNonDiscard(r);
            if (chval == delim)
                throw new ApplicationException("Map literal must contain an even number of forms");
            var value = ReadForm(r, chval);

            builder.Add(key, value);
        }
        return builder.ToImmutable();
    }

    private static string ReadToken(TextReader r, int initch) =>
        ReadUntil(ch => IsWhiteSpace(ch) || IsMacro(ch), r, initch);

    private static object ReadKeyword(TextReader r, int initch) => 
        new Keyword(ReadToken(r, r.Read()));

    private static object ReadUnmatchedDelimiter(TextReader r, int initch) =>
        throw new ApplicationException($"Unmatched delimiter: {(char)initch}");

    private static object ReadDispatch(TextReader r, int initch)
    {
        int ch = r.Read();
        if (IsEof(ch))
            throw new ApplicationException("EOF while reading character");
        if (!DispatchMacros.ContainsKey(ch))
            throw new ApplicationException($"No dispatch macro for: {(char)ch}");

        return DispatchMacros[ch](r, ch);
    }

    private static object ReadSet(TextReader r, int initch) =>
        ImmutableHashSet.CreateRange<dynamic>(ReadDelimitedList('}', r, initch));

    private static int ReadNonDiscard(TextReader r)
    {
        int ch = r.Read();
        while(IsWhiteSpace(ch) || IsComment(r, ch))
            ch = r.Read();
        return ch;
    }

    private static string ReadLineDiscards(TextReader r) =>
        ReadUntil(ch => (!IsWhiteSpace(ch) && !IsComment(r, ch)) || IsNewLine(ch), r);

    private static bool IsComment(TextReader r, int ch)
    {
        if (ch != ';')
            return false;
        ReadUntil(ch => IsNewLine(ch), r);
        return true;
    }

    // Read characters until the condition is met (don't consume the next character)
    private static string ReadUntil(Func<int, bool> until, TextReader r, int? initch = null)
    {
        var sb = new StringBuilder();
        if (initch.HasValue)
            sb.Append((char)initch.Value);

        while(true)
        {
            int ch = r.Peek();
            if (IsEof(ch) || until(ch))
                return sb.ToString();
            sb.Append((char)ch);
            r.Read();
        }
    }
}
