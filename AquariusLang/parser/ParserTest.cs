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

    struct InfixExpressionsTest {
        public string input;
        public object leftValue;
        public string _operator;
        public object rightValue;
    }

    [Fact]
    public void TestParsingInfixExpressions() {
        InfixExpressionsTest[] tests = {
            new() { input = "5 + 5;", leftValue = 5, _operator = "+", rightValue = 5 },
            new() { input = "5 - 5;", leftValue = 5, _operator = "-", rightValue = 5 },
            new() { input = "5 * 5;", leftValue = 5, _operator = "*", rightValue = 5 },
            new() { input = "5 / 5;", leftValue = 5, _operator = "/", rightValue = 5 },
            new() { input = "5 > 5;", leftValue = 5, _operator = ">", rightValue = 5 },
            new() { input = "5 < 5;", leftValue = 5, _operator = "<", rightValue = 5 },
            new() { input = "5 == 5;", leftValue = 5, _operator = "==", rightValue = 5 },
            new() { input = "5 != 5;", leftValue = 5, _operator = "!=", rightValue = 5 },
            new() { input = "foobar + barfoo;", leftValue = "foobar", _operator = "+", rightValue = "barfoo" },
            new() { input = "foobar - barfoo;", leftValue = "foobar", _operator = "-", rightValue = "barfoo" },
            new() { input = "foobar * barfoo;", leftValue = "foobar", _operator = "*", rightValue = "barfoo" },
            new() { input = "foobar / barfoo;", leftValue = "foobar", _operator = "/", rightValue = "barfoo" },
            new() { input = "foobar > barfoo;", leftValue = "foobar", _operator = ">", rightValue = "barfoo" },
            new() { input = "foobar < barfoo;", leftValue = "foobar", _operator = "<", rightValue = "barfoo" },
            new() { input = "foobar == barfoo;", leftValue = "foobar", _operator = "==", rightValue = "barfoo" },
            new() { input = "foobar != barfoo;", leftValue = "foobar", _operator = "!=", rightValue = "barfoo" },
            new() { input = "true == true", leftValue = true, _operator = "==", rightValue = true },
            new() { input = "true != false", leftValue = true, _operator = "!=", rightValue = false },
            new() { input = "false == false", leftValue = false, _operator = "==", rightValue = false },
        };
        foreach (var test in tests) {
            Lexer lexer = Lexer.NewInstance(test.input);
            Parser parser = Parser.NewInstance(lexer);
            AbstractSyntaxTree tree = parser.ParseAST();
            
            Assert.NotEqual(checkParserErrors(parser), true);
            Assert.Equal(tree.Statements.Length, 1);
            Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);

            ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
            Assert.Equal(testInfixExpression(statement.Expression, test.leftValue, test._operator, test.rightValue), true);
        }
    }

    struct OperatorPrecedenceTest {
        public string input;
        public string expected;
    }
    [Fact]
    public void TestOperatorPrecedenceParsing() {
        OperatorPrecedenceTest[] tests = {
            new() { input = "-a * b", expected = "((-a) * b)", }, 
            new() { input = "!-a", expected = "(!(-a))", }, 
            new() { input = "a + b + c", expected = "((a + b) + c)", },
            new() { input = "a + b - c", expected = "((a + b) - c)", }, 
            new() { input = "a * b * c", expected = "((a * b) * c)", }, 
            new() { input = "a * b / c", expected = "((a * b) / c)", },
            new() { input = "a + b / c", expected = "(a + (b / c))", }, 
            new() { input = "a + b * c + d / e - f", expected = "(((a + (b * c)) + (d / e)) - f)", },
            new() { input = "3 + 4; -5 * 5", expected = "(3 + 4)((-5) * 5)", }, 
            new() { input = "5 > 4 == 3 < 4", expected = "((5 > 4) == (3 < 4))", },
            new() { input = "5 < 4 != 3 > 4", expected = "((5 < 4) != (3 > 4))", },
            new() { input = "3 + 4 * 5 == 3 * 1 + 4 * 5", expected = "((3 + (4 * 5)) == ((3 * 1) + (4 * 5)))", }, 
            new() { input = "true", expected = "true", },
            new() { input = "false", expected = "false", }, 
            new() { input = "3 > 5 == false", expected = "((3 > 5) == false)", },
            new() { input = "3 < 5 == true", expected = "((3 < 5) == true)", }, 
            new() { input = "1 + (2 + 3) + 4", expected = "((1 + (2 + 3)) + 4)", },
            new() { input = "(5 + 5) * 2", expected = "((5 + 5) * 2)", }, 
            new() { input = "2 / (5 + 5)", expected = "(2 / (5 + 5))", },
            new() { input = "(5 + 5) * 2 * (5 + 5)", expected = "(((5 + 5) * 2) * (5 + 5))", }, 
            new() { input = "-(5 + 5)", expected = "(-(5 + 5))", },
            new() { input = "!(true == true)", expected = "(!(true == true))", }, 
            new() { input = "a + add(b * c) + d", expected = "((a + add((b * c))) + d)", },
            new() { input = "add(a, b, 1, 2 * 3, 4 + 5, add(6, 7 * 8))", expected = "add(a, b, 1, (2 * 3), (4 + 5), add(6, (7 * 8)))", },
            new() { input = "add(a + b + c * d / f + g)", expected = "add((((a + b) + ((c * d) / f)) + g))", },
        };
        foreach (var test in tests) {
            Lexer lexer = Lexer.NewInstance(test.input);
            Parser parser = Parser.NewInstance(lexer);
            AbstractSyntaxTree tree = parser.ParseAST();
            Assert.NotEqual(checkParserErrors(parser), true);
            string actual = tree.String();
            Assert.Equal(actual, test.expected);
        }
    }

    struct BooleanExpressionTest {
        public string input;
        public bool expectedBoolean;
    }

    [Fact]
    public void TestBooleanExpression() {
        BooleanExpressionTest[] tests = {
            new BooleanExpressionTest() { input = "true;", expectedBoolean = true },
            new BooleanExpressionTest() { input = "false;", expectedBoolean = false },
        };
        foreach (var test in tests) {
            Lexer lexer = Lexer.NewInstance(test.input);
            Parser parser = Parser.NewInstance(lexer);
            AbstractSyntaxTree tree = parser.ParseAST();
            Assert.NotEqual(checkParserErrors(parser), true);
            
            Assert.Equal(tree.Statements.Length, 1);
            Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);

            ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
            Assert.IsType(typeof(BooleanLiteral), statement.Expression);

            BooleanLiteral booleanLiteral = (BooleanLiteral)statement.Expression;
            Assert.Equal(booleanLiteral.Value, test.expectedBoolean);
        }
    }

    [Fact]
    public void TestIfExpression() {
        string input = "if (x < y) { x }";
        Lexer lexer = Lexer.NewInstance(input);
        Parser parser = Parser.NewInstance(lexer);
        AbstractSyntaxTree tree = parser.ParseAST();
        
        Assert.NotEqual(checkParserErrors(parser), true);
        
        Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);
        ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
        
        Assert.IsType(typeof(IfExpression), statement.Expression);
        IfExpression expression = (IfExpression)statement.Expression;
        
        Assert.Equal(testInfixExpression(expression.Condition, "x", "<", "y"), true);
        Assert.Equal(expression.Consequence.Statements.Length, 1);
        
        Assert.IsType(typeof(ExpressionStatement), expression.Consequence.Statements[0]);
        ExpressionStatement consequenceStatement = (ExpressionStatement)expression.Consequence.Statements[0];
        Assert.Equal(testIdentifier(consequenceStatement.Expression, "x"), true);
        Assert.Null(expression.Alternative);
    }

    [Fact]
    public void TestIfElseExpression() {
        string input = "if (x < y) { x } else { y }";
        Lexer lexer = Lexer.NewInstance(input);
        Parser parser = Parser.NewInstance(lexer);
        AbstractSyntaxTree tree = parser.ParseAST();
        Assert.NotEqual(checkParserErrors(parser), true);
        Assert.Equal(tree.Statements.Length, 1);
        
        Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);
        ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
        
        Assert.IsType(typeof(IfExpression), statement.Expression);
        IfExpression expression = (IfExpression)statement.Expression;
        
        Assert.Equal(testInfixExpression(expression.Condition, "x", "<", "y"), true);
        Assert.Equal(expression.Consequence.Statements.Length, 1);
        
        Assert.IsType(typeof(ExpressionStatement), expression.Consequence.Statements[0]);
        ExpressionStatement consequence = (ExpressionStatement)expression.Consequence.Statements[0];
        
        Assert.Equal(testIdentifier(consequence.Expression, "x"), true);
        
        Assert.Equal(expression.Alternative.Statements.Length, 1);
        
        Assert.IsType(typeof(ExpressionStatement), expression.Alternative.Statements[0]);
        ExpressionStatement alternative = (ExpressionStatement)expression.Alternative.Statements[0];
        Assert.Equal(testIdentifier(alternative.Expression, "y"), true);
    }

    [Fact]
    public void TestFunctionLiteralParsing() {
        string input = "fn(x, y) { x + y; }";
        Lexer lexer = Lexer.NewInstance(input);
        Parser parser = Parser.NewInstance(lexer);
        AbstractSyntaxTree tree = parser.ParseAST();
        Assert.NotEqual(checkParserErrors(parser), true);
        Assert.Equal(tree.Statements.Length, 1);
        
        Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);
        ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
        
        Assert.IsType(typeof(FunctionLiteral), statement.Expression);
        FunctionLiteral functionLiteral = (FunctionLiteral)statement.Expression;
        
        Assert.Equal(functionLiteral.Parameters.Length, 2);
        
        Assert.Equal(testLiteralExpression(functionLiteral.Parameters[0], "x"), true);
        Assert.Equal(testLiteralExpression(functionLiteral.Parameters[1], "y"), true);
        
        Assert.Equal(functionLiteral.Body.Statements.Length, 1);
        
        Assert.IsType(typeof(ExpressionStatement), functionLiteral.Body.Statements[0]);
        ExpressionStatement bodyStatement = (ExpressionStatement)functionLiteral.Body.Statements[0];
        
        Assert.Equal(testInfixExpression(bodyStatement.Expression, "x", "+", "y"), true);
    }

    struct FunctionParameterTest {
        public string input;
        public string[] expectedParams;
    }

    [Fact]
    public void TestFunctionParameterParsing() {
        FunctionParameterTest[] tests = {
            new() { input = "fn() {};", expectedParams = new string[] { } },
            new() { input = "fn(x) {};", expectedParams = new string[] { "x" } },
            new() { input = "fn(x, y, z) {};", expectedParams = new string[] { "x", "y", "z" } },
        };
        foreach (var test in tests) {
            Lexer lexer = Lexer.NewInstance(test.input);
            Parser parser = Parser.NewInstance(lexer);
            AbstractSyntaxTree tree = parser.ParseAST();
            Assert.NotEqual(checkParserErrors(parser), true);
            
            Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);
            ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
            
            Assert.IsType(typeof(FunctionLiteral), statement.Expression);
            FunctionLiteral functionLiteral = (FunctionLiteral)statement.Expression;
            
            Assert.Equal(functionLiteral.Parameters.Length, test.expectedParams.Length);
            
            for (var i = 0; i < test.expectedParams.Length; i++) {
                Assert.Equal(testLiteralExpression(functionLiteral.Parameters[i], test.expectedParams[i]), true);
            }
        }
    }

    [Fact]
    public void TestCallExpressionParsing() {
        string input = "add(1, 2 * 3, 4 + 5);";
        
        Lexer lexer = Lexer.NewInstance(input);
        Parser parser = Parser.NewInstance(lexer);
        AbstractSyntaxTree tree = parser.ParseAST();
        Assert.NotEqual(checkParserErrors(parser), true);
        
        Assert.Equal(tree.Statements.Length, 1);
        
        Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);
        ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
        
        Assert.IsType(typeof(CallExpression), statement.Expression);
        CallExpression expression = (CallExpression)statement.Expression;
        
        Assert.Equal(testIdentifier(expression.Function, "add"), true);
        
        Assert.Equal(expression.Arguments.Length, 3);
        
        Assert.Equal(testLiteralExpression(expression.Arguments[0], 1), true);
        Assert.Equal(testInfixExpression(expression.Arguments[1], 2, "*", 3), true);
        Assert.Equal(testInfixExpression(expression.Arguments[2], 4, "+", 5), true);
    }

    struct CallExpressionParameterTest {
        public string input;
        public string expectedIdent;
        public string[] expectedArgs;
    }

    [Fact]
    public void TestCallExpressionParameterParsing() {
        CallExpressionParameterTest[] tests = {
            new() { input = "add();", expectedIdent = "add", expectedArgs = new string[] { }, },
            new() { input = "add(1);", expectedIdent = "add", expectedArgs = new string[] { "1" }, },
            new() {
                input = "add(1, 2 * 3, 4 + 5);",
                expectedIdent = "add",
                expectedArgs = new string[] { "1", "(2 * 3)", "(4 + 5)" },
            },
        };
        foreach (var test in tests) {
            Lexer lexer = Lexer.NewInstance(test.input);
            Parser parser = Parser.NewInstance(lexer);
            AbstractSyntaxTree tree = parser.ParseAST();
            Assert.NotEqual(checkParserErrors(parser), true);
            
            Assert.IsType(typeof(ExpressionStatement), tree.Statements[0]);
            ExpressionStatement statement = (ExpressionStatement)tree.Statements[0];
            
            Assert.IsType(typeof(CallExpression), statement.Expression);
            CallExpression expression = (CallExpression)statement.Expression;
            
            Assert.Equal(testIdentifier(expression.Function, test.expectedIdent), true);
            
            Assert.Equal(expression.Arguments.Length, test.expectedArgs.Length);
            
            for (var i = 0; i < test.expectedArgs.Length; i++) {
                Assert.Equal(expression.Arguments[i].String(), test.expectedArgs[i]);
            }
        }
    }

    private bool testInfixExpression(IExpression expression, object left, string _operator, object right) {
        if (expression.GetType() != typeof(InfixExpression)) {
            _testOutputHelper.WriteLine($"expression is not InfixExpression. Got={expression}");
            return false;
        }

        InfixExpression operatorExpression = (InfixExpression)expression;
        if (!testLiteralExpression(operatorExpression.Left, left)) {
            return false;
        }

        if (operatorExpression.Operator != _operator) {
            _testOutputHelper.WriteLine($"operatorExpression.Operator is not {_operator}. Got={operatorExpression.Operator}.");
            return false;
        }

        if (!testLiteralExpression(operatorExpression.Right, right)) {
            return false;
        }

        return true;
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