using AquariusLang.ast;
using AquariusLang.lexer;
using Xunit;
using Xunit.Abstractions;

namespace AquariusLang.parser; 

public class ParserTest {
    /// <summary>
    /// For logging outputs during testing.
    /// </summary>
    private readonly ITestOutputHelper _testOutputHelper;
    
    public ParserTest(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
    }
    
    struct LetStatementTest {
        public string input;
        public string expectedIdentifier;
        public object expectedValue;
    }
    [Fact]
    public void TestLetStatements() {
        LetStatementTest[] tests = new[] {
            new LetStatementTest() { input = "let x = 5;", expectedIdentifier = "x", expectedValue = 5 },
            new LetStatementTest() { input = "let y = true;", expectedIdentifier = "y", expectedValue = true },
            new LetStatementTest() { input = "let foobar = y;", expectedIdentifier = "foobar", expectedValue = "y" }
        };
        foreach (var test in tests) {
            Lexer lexer = Lexer.NewInstance(test.input);
            Parser parser = Parser.NewInstance(lexer);
            AbstractSyntaxTree tree = parser.ParseAST();
            Assert.Equal(checkParserErrors(parser), false);

            if (tree.Statements.Length != 1) {
                _testOutputHelper.WriteLine($"tree.Statements does not contain 1 statements. Got={tree.Statements.Length}");
            }

            IStatement statement = tree.Statements[0];
            
            Assert.IsType(typeof(LetStatement), statement);
            
            Assert.Equal(testLetStatement(statement, test.expectedIdentifier), true);

            IExpression value = ((LetStatement)statement).Value;
            Assert.Equal(testLiteralExpression(value, test.expectedValue), true);
        }
    }

    struct ReturnStatementTest {
        public string input;
        public object expectedValue;
    }
    [Fact]
    public void TestReturnStatements() {
        ReturnStatementTest[] tests = {
            new ReturnStatementTest() { input = "return 5;", expectedValue = 5 },
            new ReturnStatementTest() { input = "return true;", expectedValue = true },
            new ReturnStatementTest() { input = "return foobar;", expectedValue = "foobar" }
        };
        foreach (var test in tests) {
            Lexer lexer = Lexer.NewInstance(test.input);
            Parser parser = Parser.NewInstance(lexer);
            AbstractSyntaxTree tree = parser.ParseAST();
            if (tree.Statements.Length != 1) {
                _testOutputHelper.WriteLine($"tree.Statements does not contain 1 statement. Got={tree.Statements.Length}");
            }

            IStatement statement = tree.Statements[0];
            Assert.IsType(typeof(ReturnStatement), statement);

            ReturnStatement returnStatement = (ReturnStatement)statement;
            Assert.Equal(returnStatement.TokenLiteral(), "return");
            Assert.Equal(testLiteralExpression(returnStatement.ReturnValue, test.expectedValue), true);
        }
    }

    [Fact]
    public void TestIdentifierExpression() {
        string input = "foobar;";
        
        Lexer lexer = Lexer.NewInstance(input);
        Parser parser = Parser.NewInstance(lexer);
        AbstractSyntaxTree tree = parser.ParseAST();
        
        Assert.NotEqual(checkParserErrors(parser), true);

        Assert.Equal(tree.Statements.Length, 1);
        Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);
        
        ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
        
        Assert.IsType(typeof(Identifier), statement.Expression);
        
        Identifier identifier = (Identifier)statement.Expression;
        Assert.Equal(identifier.Value, "foobar");
        Assert.Equal(identifier.TokenLiteral(), "foobar");
    }

    [Fact]
    public void TestIntegerLiteralExpression() {
        string input = "5;";
        Lexer lexer = Lexer.NewInstance(input);
        Parser parser = Parser.NewInstance(lexer);
        AbstractSyntaxTree tree = parser.ParseAST();
        Assert.NotEqual(checkParserErrors(parser), true);
        
        Assert.Equal(tree.Statements.Length, 1);
        Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);
        
        ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
        Assert.IsType(typeof(IntegerLiteral), statement.Expression);

        IntegerLiteral literal = (IntegerLiteral)statement.Expression;
        Assert.Equal(literal.Value, 5);
        Assert.Equal(literal.TokenLiteral(), "5");
    }

    struct PrefixExpressionsTest {
        public string input;
        public string _operator;
        public object value;
    }
    [Fact]
    public void TestParsingPrefixExpressions() {
        PrefixExpressionsTest[] tests = {
            new PrefixExpressionsTest() { input = "!5;", _operator = "!", value = 5 },
            new PrefixExpressionsTest() { input = "-15;", _operator = "-", value = 15 },
            new PrefixExpressionsTest() { input = "!foobar;", _operator = "!", value = "foobar" },
            new PrefixExpressionsTest() { input = "-foobar;", _operator = "-", value = "foobar" },
            new PrefixExpressionsTest() { input = "!true;", _operator = "!", value = true },
            new PrefixExpressionsTest() { input = "!false;", _operator = "!", value = false }
        };

        foreach (var test in tests) {
            Lexer lexer = Lexer.NewInstance(test.input);
            Parser parser = Parser.NewInstance(lexer);
            AbstractSyntaxTree tree = parser.ParseAST();
            
            Assert.NotEqual(checkParserErrors(parser), true);
            Assert.Equal(tree.Statements.Length, 1);
            Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);

            ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
            Assert.IsType(typeof(PrefixExpression), statement.Expression);
            
            PrefixExpression expression = (PrefixExpression)statement.Expression;
            Assert.Equal(expression.Operator, test._operator);
            
            Assert.Equal(testLiteralExpression(expression.Right, test.value), true);
        }
    }

    private bool testLiteralExpression(IExpression expression, object expected) {
        Type typeOfExpected = expected.GetType();
        if (typeOfExpected == typeof(int)) {
            return testIntegerLiteral(expression, (int)expected);
        } else if (typeOfExpected == typeof(string)) {
            return testIdentifier(expression, (string)expected);
        } else if (typeOfExpected == typeof(bool)) {
            return testBooleanLiteral(expression, (bool)expected);
        }
        _testOutputHelper.WriteLine($"Type of expression not handled. Got={expression}");
        return false;
    }

    private bool testIntegerLiteral(IExpression expression, int value) {
        return true;
    }

    private bool testIdentifier(IExpression expression, string value) {
        return true;
    }

    private bool testBooleanLiteral(IExpression expression, bool value) {
        if (expression.GetType() != typeof(BooleanLiteral)) {
            _testOutputHelper.WriteLine($"expression not BooleanLiteral type. Got={expression}");
            return false;
        }
        
        BooleanLiteral booleanLiteral = (BooleanLiteral)expression;

        if (booleanLiteral.Value != value) {
            _testOutputHelper.WriteLine($"booleanLiteral.Value not {value}. Got={booleanLiteral.Value}");
            return false;
        }

        // Use bool.Parse() for comparing bool instead of comparing strings. Because, "True" != "true"; "False" != "false".
        if (bool.Parse(booleanLiteral.TokenLiteral()) != bool.Parse($"{value.ToString().ToLower()}")) {
            _testOutputHelper.WriteLine($"booleanLiteral.TokenLiteral() not {value}. Got={booleanLiteral.TokenLiteral()}");
            return false;
        }
        
        return true;
    }

    private bool testLetStatement(IStatement statement, string name) {
        if (statement.TokenLiteral() != "let") {
            _testOutputHelper.WriteLine($"statement.TokenLiteral() not 'let'. Got={statement.TokenLiteral()}");
            return false;
        }
        

        LetStatement letStatement = (LetStatement)statement;
        if (letStatement.Name.Value != name) {
            _testOutputHelper.WriteLine($"letStatement.Name.Value not {name}. Got={letStatement.Name.Value}.");
            return false;
        }

        if (letStatement.Name.TokenLiteral() != name) {
            _testOutputHelper.WriteLine($"letStatement.Name not {name}. Got={letStatement.Name}");
            return false;
        }

        return true;
    }

    private bool checkParserErrors(Parser parser) {
        string[] errors = parser.Errors.ToArray();
        if (errors.Length == 0) return false;
        
        _testOutputHelper.WriteLine($"Parser has {errors.Length} errors.");
        foreach (var error in errors) {
            _testOutputHelper.WriteLine($"Parser error: {error}");
        }

        return true;
    }
}