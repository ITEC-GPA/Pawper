using System.Xml.Linq;
namespace Pawper.Core;

/// <summary>Strict, deliberately bounded LaTeX subset producing editable OMML.</summary>
public sealed class MathParser
{
    public static readonly XNamespace M = "http://schemas.openxmlformats.org/officeDocument/2006/math";
    readonly string input; int pos;
    MathParser(string text)
    {
        input = text;
        if (text.Length > 20000)
            throw new ArgumentException("Formula troppo lunga.");
    }
    static XElement E(string name, params object[] content) => new(M + name, content);
    static XElement T(string text) => E("r", E("t", text));
    static XElement V(string name, string value) => new(M + name, new XAttribute(M + "val", value));
    public static XElement Parse(string text)
    {
        var p = new MathParser(text);
        var result = E("oMath", p.Sequence());
        if (p.pos != text.Length)
            throw p.Error("Carattere inatteso");
        return result;
    }
    FormatException Error(string s) => new($"{s} alla posizione {pos + 1} della formula.");
    void White()
    {
        while (pos < input.Length && char.IsWhiteSpace(input[pos]))
            pos++;
    }
    List<XElement> Sequence(char? end = null, int depth = 0)
    {
        if (depth > 64)
            throw Error("Annidamento eccessivo");
        var list = new List<XElement>();
        while (true)
        {
            White();
            if (pos == input.Length)
            {
                if (end != null)
                    throw Error("Gruppo non chiuso");
                return list;
            }
            if (end != null && input[pos] == end)
            {
                pos++;
                return list;
            }
            if (input[pos] == '}')
                throw Error("Parentesi inattesa");
            XElement atom;
            if (input[pos] == '{')
            {
                pos++;
                atom = E("box", E("e", Sequence('}', depth + 1)));
            }
            else
                atom = Atom(depth);
            XElement? sub = null, sup = null;
            while (true)
            {
                White();
                if (pos >= input.Length || input[pos] is not ('_' or '^'))
                    break;
                char op = input[pos++];
                var part = E(op == '_' ? "sub" : "sup", Argument(depth + 1));
                if (op == '_')
                {
                    if (sub != null)
                        throw Error("Indice duplicato");
                    sub = part;
                }
                else
                {
                    if (sup != null)
                        throw Error("Esponente duplicato");
                    sup = part;
                }
            }
            if (sub != null && sup != null)
                atom = E("sSubSup", E("e", atom), sub, sup);
            else if (sub != null)
                atom = E("sSub", E("e", atom), sub);
            else if (sup != null)
                atom = E("sSup", E("e", atom), sup);
            list.Add(atom);
        }
    }
    object[] Argument(int depth)
    {
        White();
        if (pos >= input.Length)
            throw Error("Argomento mancante");
        if (input[pos] == '{')
        {
            pos++;
            return Sequence('}', depth + 1).Cast<object>().ToArray();
        }
        return [Atom(depth + 1)];
    }
    string RawGroup()
    {
        White();
        if (pos >= input.Length || input[pos++] != '{')
            throw Error("Atteso un gruppo tra graffe");
        int start = pos, depth = 1;
        while (pos < input.Length)
        {
            char c = input[pos++];
            if (c == '{')
                depth++;
            if (c == '}' && --depth == 0)
                return input[start..(pos - 1)];
            if (c != '}')
            { /* counted above */
            }
        }
        throw Error("Gruppo non chiuso");
    }
    XElement Atom(int depth)
    {
        if (depth > 64)
            throw Error("Annidamento eccessivo");
        char c = input[pos++];
        if (c is '^' or '_' or '&' or '$')
            throw Error("Operatore fuori contesto");
        if (c != '\\')
            return T(c.ToString());
        int start = pos;
        while (pos < input.Length && char.IsLetter(input[pos]))
            pos++;
        string cmd = input[start..pos];
        if (cmd.Length == 0)
        {
            if (pos >= input.Length)
                throw Error("Comando incompleto");
            char escaped = input[pos++];
            if ("{},;! %_".Contains(escaped))
                return T(escaped is ',' or ';' or '!' ? " " : escaped.ToString());
            throw Error("Escape non supportato");
        }
        if (Symbols.TryGetValue(cmd, out var symbol))
            return T(symbol);
        switch (cmd)
        {
            case "frac":
                return E("f", E("num", Argument(depth)), E("den", Argument(depth)));
            case "sqrt":
                White();
                object[] degree = [];
                if (pos < input.Length && input[pos] == '[')
                {
                    pos++;
                    degree = Sequence(']', depth + 1).Cast<object>().ToArray();
                }
                return E("rad", E("radPr", V("degHide", degree.Length == 0 ? "1" : "0")), E("deg", degree), E("e", Argument(depth)));
            case "text":
            case "mathrm":
            case "operatorname":
                return E("r", E("rPr", V("nor", "1")), E("t", RawGroup()));
            case "sum":
            case "prod":
            case "int":
                object[] lower = [], upper = [];
                bool gotSub = false, gotSup = false;
                while (true)
                {
                    White();
                    if (pos >= input.Length || input[pos] is not ('_' or '^'))
                        break;
                    char op = input[pos++];
                    if (op == '_')
                    {
                        if (gotSub)
                            throw Error("Indice duplicato");
                        lower = Argument(depth);
                        gotSub = true;
                    }
                    else
                    {
                        if (gotSup)
                            throw Error("Esponente duplicato");
                        upper = Argument(depth);
                        gotSup = true;
                    }
                }
                return E("nary", E("naryPr", V("chr", cmd == "sum" ? "∑" : cmd == "prod" ? "∏" : "∫"), V("limLoc", cmd == "int" ? "subSup" : "undOvr"), V("subHide", gotSub ? "0" : "1"), V("supHide", gotSup ? "0" : "1")), E("sub", lower), E("sup", upper), E("e", Argument(depth)));
            case "begin":
                var env = RawGroup();
                if (env is not ("matrix" or "pmatrix" or "bmatrix"))
                    throw Error("Ambiente non supportato");
                var closing = "\\end{" + env + "}";
                int end = input.IndexOf(closing, pos, StringComparison.Ordinal);
                if (end < 0)
                    throw Error("Matrice non chiusa");
                string body = input[pos..end];
                pos = end + closing.Length;
                var rows = Split(body, "\\\\").Select(row => Split(row, "&").ToArray()).ToArray();
                if (rows.Any(r => r.Length != rows[0].Length))
                    throw Error("Matrice irregolare");
                var matrix = E("m", rows.Select(r => E("mr", r.Select(s => E("e", Parse(s).Elements())))).ToArray());
                return env == "matrix" ? matrix : E("d", E("dPr", V("begChr", env == "pmatrix" ? "(" : "["), V("endChr", env == "pmatrix" ? ")" : "]")), E("e", matrix));
            default:
                throw Error("Comando LaTeX non supportato: \\" + cmd);
        }
    }
    static IEnumerable<string> Split(string text, string separator)
    {
        int depth = 0, begin = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '{')
                depth++;
            if (text[i] == '}')
                depth--;
            if (depth == 0 && text.AsSpan(i).StartsWith(separator))
            {
                yield return text[begin..i];
                i += separator.Length - 1;
                begin = i + 1;
            }
        }
        yield return text[begin..];
    }
    static readonly Dictionary<string, string> Symbols = new() { ["alpha"] = "α", ["beta"] = "β", ["gamma"] = "γ", ["delta"] = "δ", ["epsilon"] = "ε", ["theta"] = "θ", ["lambda"] = "λ", ["mu"] = "μ", ["nu"] = "ν", ["pi"] = "π", ["rho"] = "ρ", ["sigma"] = "σ", ["tau"] = "τ", ["phi"] = "φ", ["omega"] = "ω", ["Delta"] = "Δ", ["Sigma"] = "Σ", ["Omega"] = "Ω", ["Gamma"] = "Γ", ["Phi"] = "Φ", ["Theta"] = "Θ", ["Lambda"] = "Λ", ["Psi"] = "Ψ", ["psi"] = "ψ", ["eta"] = "η", ["zeta"] = "ζ", ["xi"] = "ξ", ["chi"] = "χ", ["kappa"] = "κ", ["upsilon"] = "υ", ["infty"] = "∞", ["times"] = "×", ["cdot"] = "·", ["pm"] = "±", ["leq"] = "≤", ["geq"] = "≥", ["neq"] = "≠", ["approx"] = "≈", ["partial"] = "∂", ["nabla"] = "∇", ["to"] = "→", ["sin"] = "sin", ["cos"] = "cos", ["tan"] = "tan", ["log"] = "log", ["ln"] = "ln", ["exp"] = "exp" };
}
