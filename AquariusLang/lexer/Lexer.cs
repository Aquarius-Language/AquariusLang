using AquariusLang.token;

namespace AquariusLang.lexer; 

public class Lexer {
    private string input;     
    private int position;     // current position in input (points to current char)
    private int readPosition; // current reading position in input (after current char)
    private char ch;          // current char under examination

    /// <summary>
    /// Singleton.
    /// </summary>
    /// <param name="input"></param>
    public static Lexer NewInstance(string input) {
        Lexer l = new Lexer() { input = input };
        l.readChar();
        return l;
    }

    public Token NextToken() {
        Token token = new Token() {
            Type = TokenType.ILLEGAL,
            Literal = ""
        };
        
        skipWhitespace();

        switch (ch) {
            case '=':
                if (peekChar() == '=') {
                    char _ch = ch;
                    readChar();
                    // string literal = char.ToString(_ch) + char.ToString(ch);
                    string literal = new string(new[] { _ch, ch });
                    token = newToken(TokenType.EQ, literal);
                } else {
                    token = newToken(TokenType.ASSIGN, ch);
                }
                break;
            case '+':
                token = newToken(TokenType.PLUS, ch);
                break;
            case '-':
                token = newToken(TokenType.MINUS, ch);
                break;
            case '!':
                // Two-character token: '!='.
                if (peekChar() == '=') {
                    char _ch = ch;
                    readChar();
                    string literal = new string(new[] { _ch, ch });
                    token = newToken(TokenType.NOT_EQ, literal);
                } else {
                    token = newToken(TokenType.BANG, ch);
                }
                break;
            case '/':
                token = newToken(TokenType.SLASH, ch);
                break;
            case '*':
                token = newToken(TokenType.ASTERISK, ch);
                break;
            case '<':
                token = newToken(TokenType.LT, ch);
                break;
            case '>':
                token = newToken(TokenType.GT, ch);
                break;
            case ';':
                token = newToken(TokenType.SEMICOLON, ch);
                break;
            case ',':
                token = newToken(TokenType.COMMA, ch);
                break;
            case '{' :
                token = newToken(TokenType.LBRACE, ch);
                break;
            case '}':
                token = newToken(TokenType.RBRACE, ch);
                break;
            case '(':
                token = newToken(TokenType.LPAREN, ch);
                break;
            case ')':
                token = newToken(TokenType.RPAREN, ch);
                break;
            case (char)0:
                token = newToken(TokenType.EOF, "");
                break;
            default:
                if (isLetter(ch)) { // Check if it is identifier token or a keyword token.
                    string literal = readIdentifier();
                    string type = TokenLookup.LookupIdentifier(literal);
                    token = newToken(type, literal);
                    return token;
                } else if (isDigit(ch)) { // Check if is number.
                    string literal = readNumber();
                    token = newToken(TokenType.INT, literal);
                    return token;
                } else {
                    token = newToken(TokenType.ILLEGAL, ch);
                }
                break;
        }

        readChar();
        return token;
    }

    private void skipWhitespace() {
        while (ch is ' ' or '\t' or '\n' or '\r') {
            readChar();
        }
    }

    private void readChar() {
        if (readPosition >= input.Length) {
            ch = (char)0;
        } else {
            ch = input[readPosition];
        }

        position = readPosition;
        readPosition++;
    }

    /// <summary>
    /// peekChar() is really similar to readChar(), except that it doesn’t increment l.position and l.readPosition.
    /// </summary>
    /// <returns></returns>
    private char peekChar() {
        return (readPosition >= input.Length) ? (char)0 : input[readPosition];
    }

    private string readIdentifier() {
        int lastPos = position;
        while (isLetter(ch)) {
            readChar();
        }

        return input.Substring(lastPos, position - lastPos);
    }

    private string readNumber() {
        int lastPos = position;
        while (isDigit(ch)) {
            readChar();
        }

        return input.Substring(lastPos, position - lastPos);
    }

    private bool isLetter(char ch) {
        // '_' is also a valid letter. This makes it possible to use foo_bar as an identifier.
        return ch is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_';
    }

    private bool isDigit(char ch) {
        return ch is >= '0' and <= '9';
    }

    private Token newToken(string tokenType, string literal) {
        return new Token() { Type = tokenType, Literal = literal };
    }
    
    private Token newToken(string tokenType, char ch) {
        return new Token() { Type = tokenType, Literal = char.ToString(ch) };
    }
}