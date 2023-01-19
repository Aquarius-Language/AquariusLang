using AquariusLang.ast;
using AquariusLang.lexer;
using AquariusLang.token;

namespace AquariusLang.parser;


public static class Precedence {
    /// <summary>
    /// These are operator precedence.
    /// Here we use iota to give the following constants incrementing numbers as values.
    /// </summary>
    public enum OperatorPrecedence {
        LOWEST = 1,
        EQUALS = 2,
        LESS_GREATER = 3,
        SUM = 4,
        PRODUCT = 5,
        PREFIX = 6,
        CALL = 7,
    }

    /// <summary>
    /// Mapping operator token types to precedence. This table is useful for helper methods
    /// peekPrecedence() and  curPrecedence().
    /// </summary>
    private static Dictionary<string, OperatorPrecedence> precedencesMap = new() {
        {TokenType.EQ, OperatorPrecedence.EQUALS},
        {TokenType.NOT_EQ, OperatorPrecedence.EQUALS},
        {TokenType.LT, OperatorPrecedence.LESS_GREATER},
        {TokenType.GT, OperatorPrecedence.LESS_GREATER},
        {TokenType.PLUS, OperatorPrecedence.SUM},
        {TokenType.MINUS, OperatorPrecedence.SUM},
        {TokenType.SLASH, OperatorPrecedence.PRODUCT},
        {TokenType.ASTERISK, OperatorPrecedence.PRODUCT},
        {TokenType.LPAREN, OperatorPrecedence.CALL},
    };

    public static int PrecedenceFor(string tokenType) {
        bool hasValue = precedencesMap.TryGetValue(tokenType, out OperatorPrecedence value);
        return hasValue? (int) value : -1;
    }
}

/// <summary>
/// A Pratt parser’s main idea is the association of parsing functions (which Pratt calls “semantic
/// code”) with token types. Whenever this token type is encountered, the parsing functions are
/// called to parse the appropriate expression and return an AST node that represents it. Each
/// token type can have up to two parsing functions associated with it, depending on whether the
/// token is found in a prefix or an infix position.
/// 
/// A prefix operator is an operator “in front of” its operand. Example: --5
/// An infix operator sits between its operands, like this: 5 * 8
/// Monkey language doesn't have postfix operator, for example: foobar++. This is due to simplifying the code.
/// 
/// The infixParseFn takes an argument: another ast.Expression. This argument is “left side” of
/// the infix operator that’s being parsed.
/// 
/// prefixParseFns gets called when we encounter the associated token
/// type in prefix position and infixParseFn gets called when we encounter the token type in infix
/// position.
/// </summary>
public delegate IExpression PrefixParseFn();

public delegate IExpression InfixParseFn(IExpression expression);


public class Parser {
    private Lexer lexer;
    private List<string> errors;
    
    /// <summary>
    /// curToken and peekToken act exactly like the two “pointers” our lexer has: position and peekPosition.
    /// But instead of pointing to a character in the input, they point to the current and the next token.
    /// </summary>
    private Token currToken;
    private Token peekToken;

    /// <summary>
    /// In order for our parser to get the correct prefixParseFn or infixParseFn for the current token type.
    /// </summary>
    private Dictionary<string, PrefixParseFn> prefixParseFns;
    private Dictionary<string, InfixParseFn> infixParseFns;
    
    /// <summary>
    /// Singleton.
    /// </summary>
    /// <returns>Parser instance.</returns>
    public static Parser NewInstance(Lexer lexer) {
        Parser parser = new Parser() { lexer = lexer, errors = new List<string>(), };
        
        // Read two tokens, so curToken and peekToken are both set.
        parser.nextToken();
        parser.nextToken();
    }

    private void nextToken() {
        currToken = peekToken;
        peekToken = lexer.NextToken();
    }

    private bool currTokenIs(string tokenType) {
        return currToken.Type == tokenType;
    }

    private bool peekTokenIs(string tokenType) {
        return peekToken.Type == tokenType;
    }

    /// <summary>
    /// Peek and move to next token only if the next token matches expected.
    /// </summary>
    /// <param name="tokenType"></param>
    /// <returns></returns>
    private bool expectPeek(string tokenType) {
        if (peekTokenIs(tokenType)) {
            nextToken();
            return true;
        }
        peekError(tokenType);
        return false;
    }

    /// <summary>
    /// This function is important for parsers. All errors of incorrect next token are found during this function.
    ///
    /// Errors get printed and appended to errors[] list here.
    ///
    /// This only gets called from expectPeek() function. As all possible errors of incorrect next token
    /// can be caught when peeked during parsing.
    /// </summary>
    /// <param name="tokenType"></param>
    private void peekError(string tokenType) {
        string msg = $"Expected next token to be {tokenType}, got {peekToken.Type} instead.";
        errors.Add(msg);
    }

    private void noPrefixParseFnError(string tokenType) {
        string msg = $"No prefix parse function for {tokenType} found.";
        errors.Add(msg);
    }

    public AbstractSyntaxTree ParseAST() {
        List<IStatement> statements = new List<IStatement>();
        while (!currTokenIs(TokenType.EOF)) {
            IStatement statement = parseStatement();
            if (statement != null) {
                statements.Add(statement);
            }
            nextToken();
        }
        AbstractSyntaxTree ast = new AbstractSyntaxTree(statements.ToArray());
        return ast;
    }

    private IStatement parseStatement() {
        switch (currToken.Type) {
            case TokenType.LET:
                return parseLetStatement();
            case TokenType.RETURN:
                return parseReturnStatement();
            default:
                return parseExpressionStatement();
        }
    }

    private LetStatement parseLetStatement() {
        LetStatement statement = new LetStatement(currToken);
        
        if (!expectPeek(TokenType.IDENT)) {
            return null;
        }

        statement.Name = new Identifier(currToken, currToken.Literal);

        if (!expectPeek(TokenType.ASSIGN)) {
            return null;
        }
        
        nextToken();

        statement.Value = parseExpression((int)Precedence.OperatorPrecedence.LOWEST);

        /*
         * Advance the current token if the next token is semicolon.
		 * (When the scope returns back to ParseProgram(), nextToken() gets called again, so the
		 * semicolon gets skipped in the end.)
         */
        if (peekTokenIs(TokenType.SEMICOLON)) {
            nextToken();
        }

        return statement;
    }

    private ReturnStatement parseReturnStatement() {
        ReturnStatement statement = new ReturnStatement(currToken);
        nextToken();
        statement.ReturnValue = parseExpression((int)Precedence.OperatorPrecedence.LOWEST);
        
        if (peekTokenIs(TokenType.SEMICOLON)) {
            nextToken();
        }

        return statement;
    }

    private ExpressionStatement parseExpressionStatement() {
        ExpressionStatement statement = new ExpressionStatement(currToken);
        /*
         * In parseExpressionStatement we pass the lowest possible precedence to parseExpression, since
		 * we didn’t parse anything yet and we can’t compare precedences.
         */
        statement.Expression = parseExpression((int)Precedence.OperatorPrecedence.LOWEST);
        if (peekTokenIs(TokenType.SEMICOLON)) {
            nextToken();
        }

        return statement;
    }

    private IExpression parseExpression(int precedence) {
        PrefixParseFn prefixParseFn = prefixParseFns[currToken.Type];
        if (prefixParseFn == null) {
            noPrefixParseFnError(currToken.Type);
            return null;
        }

        IExpression leftExp = prefixParseFn();

        while (!peekTokenIs(TokenType.SEMICOLON) && precedence < peekPrecedence()) {
            InfixParseFn infixParseFn = infixParseFns[peekToken.Type];
            if (infixParseFn == null) {
                return leftExp;
            }
            nextToken();
            /*
             * Basically calls either parseInfixExpression() or parseCallExpression().
			 * These two functions are the current only ones bound to infixParseFns.
             */
            leftExp = infixParseFn(leftExp);
        }

        return leftExp;
    }

    /// <summary>
    /// Return lowest precedence if the table didn't find matching token type.
    /// </summary>
    /// <returns></returns>
    private int peekPrecedence() {
        int precedence = Precedence.PrecedenceFor(peekToken.Type);
        return (precedence != -1)? precedence : (int)Precedence.OperatorPrecedence.LOWEST;
    }

    private int currPrecedence() {
        int precedence = Precedence.PrecedenceFor(currToken.Type);
        return (precedence != -1)? precedence : (int)Precedence.OperatorPrecedence.LOWEST;
    }

    /// <summary>
    /// Identifier expression parsing examples:
    ///
    ///     add(foobar, barfoo); // identifiers as arguments in a function call
    ///     foobar + barfoo; // as operands in an infix expression
    ///
    ///     if (foobar) { // as a standalone expression as part of a conditional
    ///         // [...]
    ///     }
    /// </summary>
    /// <returns></returns>
    private IExpression parseIdentifier() {
        return new Identifier(currToken, currToken.Literal);
    }

    private IExpression parseIntegerLiteral() {
        IntegerLiteral integerLiteral = new IntegerLiteral(currToken);
        bool parseSuccess = int.TryParse(currToken.Literal, out int value);
        if (!parseSuccess) {
            string msg = $"Could not parse{currToken.Literal} as integer.";
            errors.Add(msg);
            return null;
        }

        integerLiteral.Value = value;
        return integerLiteral;
    }
    
    

    public List<string> Errors => errors;
}