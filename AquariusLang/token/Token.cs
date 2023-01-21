namespace AquariusLang.token;

/// <summary>
/// These string constants are used as string members for Token struct. 
/// </summary>
public struct TokenType {
    public const string ILLEGAL = "ILLEGAL"; // ILLEGAL signifies a token/character we don’t know about.
    public const string EOF = "EOF"; // EOF stands for “end of file”.
    // Identifiers + literals
    public const string IDENT = "IDENT"; // add, foobar, x, y, ... etc identifiers.

    public const string INT = "INT";   // 1343456
    // Operators
    public const string ASSIGN   = "=";
    public const string PLUS     = "+";
    public const string MINUS    = "-";
    public const string BANG     = "!";
    public const string ASTERISK = "*";
    public const string SLASH    = "/";
    public const string LT = "<";
    public const string GT = ">";
    public const string EQ     = "==";
    public const string NOT_EQ = "!=";
    // Delimiters
    public const string COMMA     = ",";
    public const string SEMICOLON = ";";
    public const string LPAREN = "(";
    public const string RPAREN = ")";
    public const string LBRACE = "{";
    public const string RBRACE = "}";
    // Keywords
    public const string FUNCTION = "FUNCTION";
    public const string LET      = "LET";
    public const string TRUE     = "TRUE";
    public const string FALSE    = "FALSE";
    public const string IF       = "IF";
    public const string ELSE     = "ELSE";
    public const string RETURN   = "RETURN";

    public const string STRING = "STRING";
    public const string LBRACKET = "[";
    public const string RBRACKET = "]";
    public const string COLON = ":";
}

/// <summary>
/// Utilities for looking up keywords and or identifiers for scanned string literals.
/// </summary>
public struct TokenLookup {
    /// <summary>
    /// keywords table maps corresponded to keywords constants above.
    /// </summary>
    private static readonly Dictionary<string, string> Keywords = new() {
        {"fn",     TokenType.FUNCTION},
        {"let",    TokenType.LET},
        {"true",   TokenType.TRUE},
        {"false",  TokenType.FALSE},
        {"if",     TokenType.IF},
        {"else",   TokenType.ELSE},
        {"return", TokenType.RETURN},
    };

    /// <summary>
    ///     LookupIdent checks the keywords table to see whether the given literal is in fact a keyword.
    /// If it is, it returns the keyword’s TokenType constant. If it isn’t, we just get back token.IDENT,
    /// which is the TokenType for all user-defined identifiers.
    /// </summary>
    /// <param name="literal"></param>
    public static string LookupIdentifier(string literal) {
        return Keywords.ContainsKey(literal) ? Keywords[literal] : TokenType.IDENT;
    }
} 

public struct Token {
    /// <summary>
    /// Example token types: "=", "INT", "<", etc. It's corresponded to constants above in TokenType.
    /// </summary>
    public string Type;

    public string Literal;

    public override string ToString() {
        return $"Type: {Type}, Literal: {Literal}";
    }
}
