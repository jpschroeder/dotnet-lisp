using System;
using NUnit.Framework;

public class ReaderTests
{
    [TestCase("1", "1")]
    [TestCase("7", "7")]
    [TestCase("  7   ", "7")]
    [TestCase("-123", "-123")]
    public void Numbers(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("+", "+")]
    [TestCase("abc", "abc")]
    [TestCase("   abc   ", "abc")]
    [TestCase("abc5", "abc5")]
    [TestCase("abc-def", "abc-def")]
    public void Symbols(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("-", "-")]
    [TestCase("-abc", "-abc")]
    [TestCase("->>", "->>")]
    public void Dashes(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("(+ 1 2)", "(+ 1 2)")]
    [TestCase("()", "()")]
    [TestCase("( )", "()")]
    [TestCase("(nil)", "(nil)")]
    [TestCase("((3 4))", "((3 4))")]
    [TestCase("(+ 1 (+ 2 3))", "(+ 1 (+ 2 3))")]
    [TestCase("  ( +   1   (+   2 3   )   )  ", "(+ 1 (+ 2 3))")]
    [TestCase("(* 1 2)", "(* 1 2)")]
    [TestCase("(** 1 2)", "(** 1 2)")]
    [TestCase("(* -3 6)", "(* -3 6)")]
    [TestCase("(()())", "(() ())")]
    public void Lists(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("(1 2, 3,,,,),,", "(1 2 3)")]
    public void Commas(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("nil", "nil")]
    [TestCase("true", "true")]
    [TestCase("false", "false")]
    public void NilTrueFalse(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));
        
    [TestCase("\"abc\"", "\"abc\"")]
    [TestCase("   \"abc\"   ", "\"abc\"")]
    [TestCase("\"abc (with parens)\"", "\"abc (with parens)\"")]
    [TestCase("\"abc\\\"def\"", "\"abc\\\"def\"")]
    [TestCase("\"\"", "\"\"")]
    [TestCase("\"\\\\\"", "\"\\\\\"")]
    [TestCase("\"\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\"", "\"\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\"")]
    [TestCase("\"&\"", "\"&\"")]
    [TestCase("\"'\"", "\"'\"")]
    [TestCase("\"(\"", "\"(\"")]
    [TestCase("\")\"", "\")\"")]
    [TestCase("\"*\"", "\"*\"")]
    [TestCase("\"+\"", "\"+\"")]
    [TestCase("\",\"", "\",\"")]
    [TestCase("\"-\"", "\"-\"")]
    [TestCase("\"/\"", "\"/\"")]
    [TestCase("\":\"", "\":\"")]
    [TestCase("\";\"", "\";\"")]
    [TestCase("\"<\"", "\"<\"")]
    [TestCase("\"=\"", "\"=\"")]
    [TestCase("\">\"", "\">\"")]
    [TestCase("\"?\"", "\"?\"")]
    [TestCase("\"@\"", "\"@\"")]
    [TestCase("\"[\"", "\"[\"")]
    [TestCase("\"]\"", "\"]\"")]
    [TestCase("\"^\"", "\"^\"")]
    [TestCase("\"_\"", "\"_\"")]
    [TestCase("\"`\"", "\"`\"")]
    [TestCase("\"{\"", "\"{\"")]
    [TestCase("\"}\"", "\"}\"")]
    [TestCase("\"~\"", "\"~\"")]

    [TestCase("\"\\n\"", "\"\\n\"")]
    [TestCase("\"#\"", "\"#\"")]
    [TestCase("\"$\"", "\"$\"")]
    [TestCase("\"%\"", "\"%\"")]
    [TestCase("\".\"", "\".\"")]
    [TestCase("\"\\\\\"", "\"\\\\\"")]
    [TestCase("\"|\"", "\"|\"")]
    public void Strings(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("\\a", "\\a")]
    [TestCase("\\8", "\\8")]
    [TestCase("\\newline", "\\newline")]
    [TestCase("\\tab", "\\tab")]
    [TestCase("\\space", "\\space")]
    public void Characters(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("[1 2")]
    [TestCase("\"abc")]
    [TestCase("\"")]
    [TestCase("\"\\\"")]
    [TestCase(@"""\\\\\\\\\\\\\\\\\\\""")]
    [TestCase("(1 \"abc")]
    [TestCase("(1 \"abc\"")]
    public void Errors(string input) =>
        Assert.Throws<ApplicationException>(() => Reader.Read(input));

    [TestCase(":kw", ":kw")]
    [TestCase("(:kw1 :kw2 :kw3)", "(:kw1 :kw2 :kw3)")]
    public void Keywords(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("[+ 1 2]", "[+ 1 2]")]
    [TestCase("[]", "[]")]
    [TestCase("[ ]", "[]")]
    [TestCase("[[3 4]]", "[[3 4]]")]
    [TestCase("[+ 1 [+ 2 3]]", "[+ 1 [+ 2 3]]")]
    [TestCase("  [ +   1   [+   2 3   ]   ]  ", "[+ 1 [+ 2 3]]")]
    [TestCase("([])", "([])")]
    public void Vectors(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("{}", "{}")]
    [TestCase("{ }", "{}")]
    [TestCase("{\"abc\" 1}", "{\"abc\" 1}")]
    [TestCase("{\"a\" {\"b\" 2}}", "{\"a\" {\"b\" 2}}")]
    [TestCase("{\"a\" {\"b\" {\"c\" 3}}}", "{\"a\" {\"b\" {\"c\" 3}}}")]
    [TestCase("{  \"a\"  {\"b\"   {  \"cde\"     3   }  }}", "{\"a\" {\"b\" {\"cde\" 3}}}")]
    //[TestCase("{\"a1\" 1 \"a2\" 2 \"a3\" 3}", "{\"a([1-3])\" \\1 \"a(?!\\1)([1-3])\" \\2 \"a(?!\\1)(?!\\2)([1-3])\" \\3}")]
    [TestCase("{  :a  {:b   {  :cde     3   }  }}", "{:a {:b {:cde 3}}}")]
    [TestCase("{\"1\" 1}", "{\"1\" 1}")]
    [TestCase("({})", "({})")]
    public void Maps(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));

    [TestCase("1 ; comment after expression", "1")]
    [TestCase("1; comment after expression", "1")]
    [TestCase("1;!", "1")]
    [TestCase("1;\"", "1")]
    [TestCase("1;#", "1")]
    [TestCase("1;$", "1")]
    [TestCase("1;%", "1")]
    [TestCase("1;'", "1")]
    [TestCase("1;\\", "1")]
    [TestCase("1;\\\\", "1")]
    [TestCase("1;\\\\\\", "1")]
    [TestCase("1;`", "1")]
    public void Comments(string input, string output) =>
        Assert.AreEqual(output, Printer.Print(Reader.Read(input)));
}

public class EvalTests
{
    [TestCase("(+ 1 2)", "3")]
    [TestCase("(+ 5 (* 2 3))", "11")]
    [TestCase("(- (+ 5 (* 2 3)) 3)", "8")]
    [TestCase("(/ (- (+ 5 (* 2 3)) 3) 4)", "2")]
    [TestCase("(/ (- (+ 515 (* 87 311)) 302) 27)", "1010")]
    [TestCase("(* -3 6)", "-18")]
    [TestCase("(/ (- (+ 515 (* -87 311)) 296) 27)", "-994")]
    public void Arithmetic(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(abc 1 2 3)")]
    public void Errors(string input) =>
        Assert.Throws<ApplicationException>(() => Rep(input));

    [TestCase("()", "()")]
    [TestCase("[]", "[]")]
    [TestCase("{}", "{}")]
    public void Empty(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("[1 2 (+ 1 2)]", "[1 2 3]")]
    [TestCase("{\"a\" (+ 7 8)}", "{\"a\" 15}")]
    [TestCase("{:a (+ 7 8)}", "{:a 15}")]
    public void Collections(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    public static string Rep(string input)
    {
        var (result, _) = Interpreter.Eval(Reader.Read(input));
        return Printer.Print(result);
    }
}

public class EnvTests
{
    private Env _env = Interpreter.DefaultEnv;

    [TestCase("(def x 3)", "3")]
    [TestCase("x", "3")]
    [TestCase("(def x 4)", "4")]
    [TestCase("x", "4")]
    [TestCase("(def y (+ 1 7))", "8")]
    [TestCase("y", "8")]
    [TestCase("(+ 1 (+ y 1))", "10")]
    public void Def(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(def mynum 111)", "111")]
    [TestCase("(def MYNUM 222)", "222")]
    [TestCase("mynum", "111")]
    [TestCase("MYNUM", "222")]
    public void CaseSensitive(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [Test]
    public void Errors()
    {
        Assert.AreEqual("123", Rep("(def w 123)"));
        Assert.Throws<ApplicationException>(() => Rep("(def w (abc))"));
        Assert.AreEqual("123", Rep("w"));
    }

    [TestCase("(def x 4)", "4")]
    [TestCase("(let [z 9] z)", "9")]
    [TestCase("(let [x 9] x)", "9")]
    [TestCase("x", "4")]
    [TestCase("(let [z (+ 2 3)] (+ 1 z))", "6")]
    [TestCase("(let [p (+ 2 3) q (+ 2 p)] (+ p q))", "12")]
    [TestCase("(def y (let [z 7] z))", "7")]
    [TestCase("y", "7")]
    public void Let(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(def a 4)", "4")]
    [TestCase("(let [q 9] q)", "9")]
    [TestCase("(let [q 9] a)", "4")]
    [TestCase("(let [z 2] (let [q 9] a))", "4")]
    public void Outer(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(let [p (+ 2 3) q (+ 2 p)] (+ p q))", "12")]
    [TestCase("(let [a 5 b 6] [3 4 a [b 7] 8])", "[3 4 5 [6 7] 8]")]
    public void Vectors(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    public string Rep(string input)
    {
        var (result, newenv) = Interpreter.Eval(Reader.Read(input), _env);
        _env = newenv;
        return Printer.Print(result);
    }
}

public class FormTests
{
    private Env _env = Interpreter.DefaultEnv;

    [TestCase("(list)", "()")]
    [TestCase("(list? (list))", "true")]
    [TestCase("(empty? (list))", "true")]
    [TestCase("(empty? (list 1))", "false")]
    [TestCase("(list 1 2 3)", "(1 2 3)")]
    [TestCase("(count (list 1 2 3))", "3")]
    [TestCase("(count (list))", "0")]
    [TestCase("(count nil)", "0")]
    [TestCase("(if (> (count (list 1 2 3)) 3) 89 78)", "78")]
    [TestCase("(if (>= (count (list 1 2 3)) 3) 89 78)", "89")]
    public void ListFuncs(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(if true 7 8)", "7")]
    [TestCase("(if false 7 8)", "8")]
    [TestCase("(if false 7 false)", "false")]
    [TestCase("(if true (+ 1 7) (+ 1 8))", "8")]
    [TestCase("(if false (+ 1 7) (+ 1 8))", "9")]
    [TestCase("(if nil 7 8)", "8")]
    [TestCase("(if 0 7 8)", "8")]
    [TestCase("(if (list) 7 8)", "7")]
    [TestCase("(if (list 1 2 3) 7 8)", "7")]
    [TestCase("(= (list) nil)", "false")]
    public void If(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(if false (+ 1 7))", "nil")]
    [TestCase("(if nil 8)", "nil")]
    [TestCase("(if nil 8 7)", "7")]
    [TestCase("(if true (+ 1 7))", "8")]
    public void OnewayIf(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(= 2 1)", "false")]
    [TestCase("(= 1 1)", "true")]
    [TestCase("(= 1 2)", "false")]
    [TestCase("(= 1 (+ 1 1))", "false")]
    [TestCase("(= 2 (+ 1 1))", "true")]
    [TestCase("(= nil 1)", "false")]
    [TestCase("(= nil nil)", "true")]
    [TestCase("(> 2 1)", "true")]
    [TestCase("(> 1 1)", "false")]
    [TestCase("(> 1 2)", "false")]
    [TestCase("(>= 2 1)", "true")]
    [TestCase("(>= 1 1)", "true")]
    [TestCase("(>= 1 2)", "false")]
    [TestCase("(< 2 1)", "false")]
    [TestCase("(< 1 1)", "false")]
    [TestCase("(< 1 2)", "true")]
    [TestCase("(<= 2 1)", "false")]
    [TestCase("(<= 1 1)", "true")]
    [TestCase("(<= 1 2)", "true")]
    public void Conditionals(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(= 1 1)", "true")]
    [TestCase("(= 0 0)", "true")]
    [TestCase("(= 1 0)", "false")]
    [TestCase("(= true true)", "true")]
    [TestCase("(= false false)", "true")]
    [TestCase("(= nil nil)", "true")]
    [TestCase("(= (list) (list))", "true")]
    [TestCase("(= (list 1 2) (list 1 2))", "true")]
    [TestCase("(= (list 1) (list))", "false")]
    [TestCase("(= (list) (list 1))", "false")]
    [TestCase("(= 0 (list))", "false")]
    [TestCase("(= (list) 0)", "false")]
    [TestCase("(= (list nil) (list))", "false")]
    public void Equality(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(+ 1 2)", "3")]
    [TestCase("((fn [a b] (+ b a)) 3 4)", "7")]
    [TestCase("((fn [] 4))", "4")]
    [TestCase("((fn [f x] (f x)) (fn [a] (+ 1 a)) 7)", "8")]
    public void Functions(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(((fn [a] (fn [b] (+ a b))) 5) 7)", "12")]
    [TestCase("(def gen-plus5 (fn [] (fn [b] (+ 5 b))))", "Function")]
    [TestCase("(def plus5 (gen-plus5))", "Function")]
    [TestCase("(plus5 7)", "12")]
    [TestCase("(def gen-plusX (fn [x] (fn [b] (+ x b))))", "Function")]
    [TestCase("(def plus7 (gen-plusX 7))", "Function")]
    [TestCase("(plus7 8)", "15")]
    public void Closures(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(do (def a 6) 7 (+ a 8))", "14")]
    [TestCase("(def DO (fn [a] 7))", "Function")]
    [TestCase("(DO 3)", "7")]
    public void Do(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(def sumdown (fn [N] (if (> N 0) (+ N (sumdown  (- N 1))) 0)))", "Function")]
    [TestCase("(sumdown 1)", "1")]
    [TestCase("(sumdown 2)", "3")]
    [TestCase("(sumdown 6)", "21")]
    [TestCase("(def fib (fn [N] (if (= N 0) 1 (if (= N 1) 1 (+ (fib (- N 1)) (fib (- N 2)))))))", "Function")]
    [TestCase("(fib 1)", "1")]
    [TestCase("(fib 2)", "2")]
    [TestCase("(fib 4)", "5")]
    [TestCase("(let [cst (fn [n] (if (= n 0) nil (cst (- n 1))))] (cst 1))", "nil")]
    [TestCase("(let [f (fn [n] (if (= n 0) 0 (g (- n 1)))) g (fn [n] (f n))] (f 2))", "0")]
    public void Recursive(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(if \"\" 7 8)", "8")]
    [TestCase("(= \"\" \"\")", "true")]
    [TestCase("(= \"abc\" \"abc\")", "true")]
    [TestCase("(= \"abc\" \"\")", "false")]
    [TestCase("(= \"\" \"abc\")", "false")]
    [TestCase("(= \"abc\" \"def\")", "false")]
    [TestCase("(= \"abc\" \"ABC\")", "false")]
    [TestCase("(= (list) \"\")", "false")]
    [TestCase("(= \"\" (list))", "false")]
    public void StringEquality(string input, string output) =>
        Assert.AreEqual(output, Rep(input));
        
    [TestCase("((fn [& more] (count more)) 1 2 3)", "3")]
    [TestCase("((fn [& more] (list? more)) 1 2 3)", "true")]
    [TestCase("((fn [& more] (count more)) 1)", "1")]
    [TestCase("((fn [& more] (count more)) )", "0")]
    [TestCase("((fn [& more] (list? more)) )", "true")]
    [TestCase("((fn [a & more] (count more)) 1 2 3)", "2")]
    [TestCase("((fn [a & more] (count more)) 1)", "0")]
    [TestCase("((fn [a & more] (list? more)) 1)", "true")]
    public void VariableLengthArguments(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    [TestCase("(not false)", "true")]
    [TestCase("(not true)", "false")]
    public void Not(string input, string output) =>
        Assert.AreEqual(output, Rep(input));

    public string Rep(string input)
    {
        var (result, newenv) = Interpreter.Eval(Reader.Read(input), _env);
        _env = newenv;
        return Printer.Print(result);
    }
}
