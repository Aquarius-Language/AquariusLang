﻿using System.Text;
using AquariusLang.token;

namespace AquariusLang.ast;

/// <summary>
/// The base Node interface.
/// </summary>
public interface INode {
    string TokenLiteral();  // Returns the literal value of the token it’s associated with. TokenLiteral() will be used only for debugging and testing.
    string String(); // This is for easier printing debugging.
}

/// <summary>
/// All statement nodes implement this.
///
///     statementNode() and expressionNode() are not strictly necessary but help us by guiding the
/// compiler and possibly causing it to throw errors when we use a Statement where an Expression
/// should’ve been used, and vice versa.
///
/// Statement doesn't produce value. e.g. let x = 5 doesn't produce a value.
/// </summary>
public interface IStatement : INode {
    void StatementNode();
}

/// <summary>
/// All expression nodes implement this.
///
/// Expression produces value.
/// </summary>
public interface IExpression : INode {
    void ExpressionNode();
}

/*
 * Statements below.
 */

/// <summary>
///     Let statement should have Three fields: one for the identifier, one for
/// the expression that produces the value in the let statement (Every expression
/// is valid after the equal sign: let x = 5 * 5 is as valid as let y = add(2, 2) * 5 / 10;)
/// , and one for the token.
/// </summary>
public class LetStatement : IStatement {
    private Token token; // the token.LET token
    private Identifier name;
    private IExpression value; // The expression that produces the value.

    public LetStatement(Token token) {
        this.token = token;
    }

    public LetStatement(Token token, Identifier name, IExpression value) {
        this.token = token;
        this.name = name;
        this.value = value;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        StringBuilder builder = new StringBuilder();
        builder.Append(TokenLiteral() + " ")
            .Append(name.String())
            .Append(" = ");
        
        if (value != null) {
            builder.Append(value.String());
        }

        builder.Append(';');

        return builder.ToString();
    }

    public void StatementNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public Identifier Name {
        get => name;
        set => name = value;
    }

    public IExpression Value {
        get => value;
        set => this.value = value;
    }
}

public class ReturnStatement : IStatement {
    private Token token;
    private IExpression returnValue;

    public ReturnStatement(Token token) {
        this.token = token;
    }

    public ReturnStatement(Token token, IExpression returnValue) {
        this.token = token;
        this.returnValue = returnValue;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        StringBuilder builder = new StringBuilder();

        builder.Append(TokenLiteral() + " ");
        if (returnValue != null) {
            builder.Append(returnValue.String());
        }

        builder.Append(';');

        return builder.ToString();
    }

    public void StatementNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public IExpression ReturnValue {
        get => returnValue;
        set => returnValue = value;
    }
}

/// <summary>
/// let x = 5; // Let statement.
/// x + 10; // Expression statement.
/// </summary>
public class ExpressionStatement : IStatement {
    private Token token;
    private IExpression expression;

    public ExpressionStatement(Token token) {
        this.token = token;
    }

    public ExpressionStatement(Token token, IExpression expression) {
        this.token = token;
        this.expression = expression;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        if (expression != null) {
            return expression.String();
        }

        return "";
    }

    public void StatementNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public IExpression Expression {
        get => expression;
        set => expression = value;
    }
}

public class BlockStatement : IStatement {
    private Token token;
    private IStatement[] statements;

    public BlockStatement(Token token) {
        this.token = token;
    }

    public BlockStatement(Token token, IStatement[] statements) {
        this.token = token;
        this.statements = statements;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        StringBuilder builder = new StringBuilder();

        foreach (var statement in statements) {
            builder.Append(statement.String());
        }

        return builder.ToString();
    }

    public void StatementNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public IStatement[] Statements {
        get => statements;
        set => statements = value;
    }
}


/*
 * Expressions below.
 */

/// <summary>
///     But the identifier in a let statement doesn’t produce a value, right? So why is it an
/// Expression? It’s to keep things simple. Identifiers in other parts of a Monkey program do
/// produce values, e.g.: let x = valueProducingIdentifier;.
/// </summary>
public class Identifier : IExpression {
    private Token token; // the token.IDENT token
    private string value;

    public Identifier(Token token, string value) {
        this.token = token;
        this.value = value;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public void ExpressionNode() {
    }
    
    public string String() {
        return value;
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public string Value {
        get => value;
        set => this.value = value;
    }
}

public class BooleanLiteral : IExpression {
    private Token token;
    private bool value;

    public BooleanLiteral(Token token, bool value) {
        this.token = token;
        this.value = value;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        return token.Literal;
    }

    public void ExpressionNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public bool Value {
        get => value;
        set => this.value = value;
    }
}

public class IntegerLiteral : IExpression {
    private Token token;
    private int value;

    public IntegerLiteral(Token token) {
        this.token = token;
    }

    public IntegerLiteral(Token token, int value) {
        this.token = token;
        this.value = value;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        return token.Literal;
    }

    public void ExpressionNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public int Value {
        get => value;
        set => this.value = value;
    }
}

public class PrefixExpression : IExpression {
    private Token token; // The prefix token, e.g. !
    private string _operator;
    private IExpression right;

    public PrefixExpression(Token token) {
        this.token = token;
    }

    public PrefixExpression(Token token, string _operator) {
        this.token = token;
        this._operator = _operator;
    }

    public PrefixExpression(Token token, string _operator, IExpression right) {
        this.token = token;
        this._operator = _operator;
        this.right = right;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        StringBuilder builder = new StringBuilder()
            .Append('(')
            .Append(_operator)
            .Append(right.String())
            .Append(')');
        return builder.ToString();
    }

    public void ExpressionNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public string Operator {
        get => _operator;
        set => _operator = value;
    }

    public IExpression Right {
        get => right;
        set => right = value;
    }
}

public class InfixExpression : IExpression {
    private Token token; // The operator token, e.g. +
    private IExpression left;
    private string _operator;
    private IExpression right;

    public InfixExpression(Token token) {
        this.token = token;
    }

    public InfixExpression(Token token, IExpression left) {
        this.token = token;
        this.left = left;
    }

    public InfixExpression(Token token, IExpression left, string _operator) {
        this.token = token;
        this.left = left;
        this._operator = _operator;
    }

    public InfixExpression(Token token, IExpression left, string _operator, IExpression right) {
        this.token = token;
        this.left = left;
        this._operator = _operator;
        this.right = right;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        StringBuilder builder = new StringBuilder()
            .Append('(')
            .Append(left.String())
            .Append(" " + _operator + " ")
            .Append(right.String())
            .Append(')');
        return builder.ToString();
    }

    public void ExpressionNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public IExpression Left {
        get => left;
        set => left = value;
    }

    public string Operator {
        get => _operator;
        set => _operator = value;
    }

    public IExpression Right {
        get => right;
        set => right = value;
    }
}

class IfExpression : IExpression {
    private Token token;  // The 'if' token.
    private IExpression condition;
    private BlockStatement consequence; // Series of statements under "if".
    private BlockStatement alternative; // Series of statements under "else".

    public IfExpression(Token token) {
        this.token = token;
    }

    public IfExpression(Token token, IExpression condition, BlockStatement consequence) {
        this.token = token;
        this.condition = condition;
        this.consequence = consequence;
    }

    public IfExpression(Token token, IExpression condition, BlockStatement consequence, BlockStatement alternative) {
        this.token = token;
        this.condition = condition;
        this.consequence = consequence;
        this.alternative = alternative;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        StringBuilder builder = new StringBuilder()
            .Append("if")
            .Append(condition.String())
            .Append(' ')
            .Append(consequence.String());

        if (alternative != null) {
            builder.Append("else ")
                .Append(alternative.String());
        }

        return builder.ToString();
    }

    public void ExpressionNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public IExpression Condition {
        get => condition;
        set => condition = value ?? throw new ArgumentNullException(nameof(value));
    }

    public BlockStatement Consequence {
        get => consequence;
        set => consequence = value ?? throw new ArgumentNullException(nameof(value));
    }

    public BlockStatement Alternative {
        get => alternative;
        set => alternative = value ?? throw new ArgumentNullException(nameof(value));
    }
}

class FunctionLiteral : IExpression {
    private Token token; // The 'fn' token.
    private Identifier[] parameters;
    private BlockStatement body;

    public FunctionLiteral(Token token) {
        this.token = token;
    }

    public FunctionLiteral(Token token, Identifier[] parameters, BlockStatement body) {
        this.token = token;
        this.parameters = parameters;
        this.body = body;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        StringBuilder builder = new StringBuilder();

        List<string> _params = new List<string>(); 
        foreach (var parameter in parameters) {
            _params.Add(parameter.String());
        }
        
        builder.Append(TokenLiteral()).
            Append('(')
            .Append(string.Join(", ", _params))
            .Append(") ")
            .Append(body.String());
        
        return builder.ToString();
    }

    public void ExpressionNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public Identifier[] Parameters {
        get => parameters;
        set => parameters = value;
    }

    public BlockStatement Body {
        get => body;
        set => body = value;
    }
}

class CallExpression : IExpression {
    private Token token; // The '(' token.
    private IExpression function; // Identifier or FunctionLiteral.
    private IExpression[] arguments;

    public CallExpression(Token token) {
        this.token = token;
    }

    public CallExpression(Token token, IExpression function) {
        this.token = token;
        this.function = function;
    }

    public CallExpression(Token token, IExpression function, IExpression[] arguments) {
        this.token = token;
        this.function = function;
        this.arguments = arguments;
    }

    public string TokenLiteral() {
        return token.Literal;
    }

    public string String() {
        StringBuilder builder = new StringBuilder();

        List<string> args = new List<string>();
        foreach (var arg in arguments) {
            args.Add(arg.String());
        }
        
        builder.Append(function.String())
            .Append('(')
            .Append(string.Join(", ", args))
            .Append(')');
        
        return builder.ToString();
    }

    public void ExpressionNode() {
    }

    public Token Token {
        get => token;
        set => token = value;
    }

    public IExpression Function {
        get => function;
        set => function = value;
    }

    public IExpression[] Arguments {
        get => arguments;
        set => arguments = value;
    }
}

/// <summary>
///     This AbstractSyntaxTree node is going to be the root node of every AST our parser
/// produces. Every valid Monkey program is a series of statements. These statements are slice
/// of AST nodes that implement the IStatement interface.
/// </summary>
public class AbstractSyntaxTree : INode {
    private IStatement[] statements;

    public AbstractSyntaxTree(IStatement[] statements) {
        this.statements = statements;
    }

    public string TokenLiteral() {
        return (statements.Length > 0) ? statements[0].TokenLiteral() : "";
    }

    public string String() {
        StringBuilder builder = new StringBuilder();
        foreach (var statement in statements) {
            builder.Append(statement.String());
        }

        return builder.ToString();
    }

    public IStatement[] Statements {
        get => statements;
        set => statements = value;
    }
}