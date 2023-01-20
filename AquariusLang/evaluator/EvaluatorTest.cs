using AquariusLang.ast;
using AquariusLang.lexer;
using AquariusLang.Object;
using AquariusLang.parser;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Environment = AquariusLang.Object.Environment;

namespace AquariusLang.evaluator; 

public class EvaluatorTest {
    /// <summary>
    /// For logging outputs during testing.
    /// </summary>
    private readonly ITestOutputHelper _testOutputHelper;

    public EvaluatorTest(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
    }
    
    struct EvalIntegerTest {
        public string input;
        public int expected;
    }
    [Fact]
    public void TestEvalIntegerExpression() {
        EvalIntegerTest[] tests = {
            new () {input = "5", expected = 5},
            new () {input = "10", expected = 10},
            new () {input = "-5", expected = -5},
            new () {input = "-10", expected = -10},
            new () {input = "5 + 5 + 5 + 5 - 10", expected = 10},
            new () {input = "2 * 2 * 2 * 2 * 2", expected = 32},
            new () {input = "-50 + 100 + -50", expected = 0},
            new () {input = "5 * 2 + 10", expected = 20},
            new () {input = "5 + 2 * 10", expected = 25},
            new () {input = "20 + 2 * -10", expected = 0},
            new () {input = "50 / 2 * 2 + 10", expected = 60},
            new () {input = "2 * (5 + 10)", expected = 30},
            new () {input = "3 * 3 * 3 + 10", expected = 37},
            new () {input = "3 * (3 * 3) + 10", expected = 37},
            new () {input = "(5 + 10 * 2 + 15 / 3) * 2 + -10", expected = 50},
        };

        foreach (var test in tests) {
            Object.Object evaluated = testEval(test.input);
            Assert.True(testIntegerObject(evaluated, test.expected));
        }
    }

    struct EvalBooleanTest {
        public string input;
        public bool expected;
    }

    [Fact]
    public void TestEvalBooleanExpression() {
        EvalBooleanTest[] tests = {
            new () {input = "true", expected = true},
            new () {input = "false", expected = false},
            new () {input = "1 < 2", expected = true},
            new () {input = "1 > 2", expected = false},
            new () {input = "1 < 1", expected = false},
            new () {input = "1 > 1", expected = false},
            new () {input = "1 == 1", expected = true},
            new () {input = "1 != 1", expected = false},
            new () {input = "1 == 2", expected = false},
            new () {input = "1 != 2", expected = true},
            new () {input = "true == true", expected = true},
            new () {input = "false == false", expected = true},
            new () {input = "true == false", expected = false},
            new () {input = "true != false", expected = true},
            new () {input = "false != true", expected = true},
            new () {input = "(1 < 2) == true", expected = true},
            new () {input = "(1 < 2) == false", expected = false},
            new () {input = "(1 > 2) == true", expected = false},
            new () {input = "(1 > 2) == false", expected = true},
        };

        foreach (var test in tests) {
            Object.Object evaluated = testEval(test.input);
            Assert.True(testBooleanObject(evaluated, test.expected));
        }
    }

    [Fact]
    public void TestBangOperator() {
        EvalBooleanTest[] tests = {
            new () {input = "!true", expected = false},
            new () {input = "!false", expected = true},
            new () {input = "!5", expected = false},
            new () {input = "!!true", expected = true},
            new () {input = "!!false", expected = false},
            new () {input = "!!5", expected = true},
        };
        foreach (var test in tests) {
            Object.Object evaluated = testEval(test.input);
            Assert.True(testBooleanObject(evaluated, test.expected));
        }
    }

    struct IfElseTest {
        public string input;
        public object expected;
    }
    [Fact]
    public void TestIfElseExpressions() {
        IfElseTest[] tests = {
            new () {input = "if (true) { 10 }", expected = 10},
            new () {input = "if (false) { 10 }", expected = null},
            new () {input = "if (1) { 10 }", expected = 10},
            new () {input = "if (1 < 2) { 10 }", expected = 10},
            new () {input = "if (1 > 2) { 10 }", expected = null},
            new () {input = "if (1 > 2) { 10 } else { 20 }", expected = 20},
            new () {input = "if (1 < 2) { 10 } else { 20 }", expected = 10},
        };
        foreach (var test in tests) {
            Object.Object evaluated = testEval(test.input);
            _testOutputHelper.WriteLine(test.input);
            if (test.expected is int) {
                int integer = (int)test.expected;
                Assert.True(testIntegerObject(evaluated, integer));
            } else {
                /*
				 When a conditional doesn’t evaluate to a value it’s supposed to
				 return NULL, e.g.: if (false) { 10 }
			    */
                Assert.True(testNullObject(evaluated));
            }
        }
    }

    struct ReturnStatementTest {
        public string input;
        public int expected;
    }
    [Fact]
    public void TestReturnStatements() {
        ReturnStatementTest[] tests = {
            new () {input = "return 10;", expected = 10},
            new () {input = "return 10; 9;", expected = 10},
            new () {input = "return 2 * 5; 9;", expected = 10},
            new () {input = "9; return 2 * 5; 9;", expected = 10},
            new () {input = "if (10 > 1) { return 10; }", expected = 10},
            new () {
                input = @"
                if (10 > 1) {
                    if (10 > 1) {
                        return 10;
                    }
                    return 1;
                }
                ",
                expected = 10,
            },
            new () {
                input = 
                @"
                let f = fn(x) {
                    return x;
                    x + 10;
                };
                f(10);",
                expected = 10,
            },
            new () {
                input = 
                @"
                let f = fn(x) {
                    let result = x + 10;
                    return result;
                    return 10;
                };
                f(10);",
                expected = 20,
            },
        };

        foreach (var test in tests) {
            Object.Object evaluated = testEval(test.input);
            Assert.True(testIntegerObject(evaluated, test.expected));
        }
    }

    struct ErrorHandlingTest {
        public string input;
        public string expectedMessage;
    }

    [Fact]
    public void TestErrorHandling() {
        ErrorHandlingTest[] tests = {
            new() { input = "5 + true;", expectedMessage = "Type mismatch: INTEGER + BOOLEAN", },
            new() { input = "5 + true; 5;", expectedMessage = "Type mismatch: INTEGER + BOOLEAN", },
            new() { input = "-true", expectedMessage = "Unknown operator: -BOOLEAN", },
            new() { input = "true + false;", expectedMessage = "Unknown operator: BOOLEAN + BOOLEAN", },
            new() { input = "true + false + true + false;", expectedMessage = "Unknown operator: BOOLEAN + BOOLEAN", },
            new() { input = "5; true + false; 5", expectedMessage = "Unknown operator: BOOLEAN + BOOLEAN", },
            new() { input = "if (10 > 1) { true + false; }", expectedMessage = "Unknown operator: BOOLEAN + BOOLEAN", },
            new() {
                input = @"
                if (10 > 1) {
                    if (10 > 1) {
                        return true + false;
                    }

                    return 1;
                }
                ",
                expectedMessage = "Unknown operator: BOOLEAN + BOOLEAN",
            },
            new() { input = "foobar", expectedMessage = "Identifier not found: foobar", },
        };

        foreach (var test in tests) {
            Object.Object evaluated = testEval(test.input);
            Assert.IsType<ErrorObj>(evaluated);

            ErrorObj errorObj = (ErrorObj)evaluated;
            
            Assert.Equal(test.expectedMessage, errorObj.Message);
        }
    }

    struct LetStatementTest {
        public string input;
        public int expected;
    }

    [Fact]
    public void TestLetStatements() {
        LetStatementTest[] tests = {
            new () {input = "let a = 5; a;", expected = 5},
            new () {input = "let a = 5 * 5; a;", expected = 25},
            new () {input = "let a = 5; let b = a; b;", expected = 5},
            new () {input = "let a = 5; let b = a; let c = a + b + 5; c;", expected = 15},
        };

        foreach (var test in tests) {
            Assert.True(testIntegerObject(testEval(test.input), test.expected));
        }
    }

    [Fact]
    public void TestFunctionObject() {
        string input = "fn(x) { x + 2; };";
        Object.Object evaluated = testEval(input);

        Assert.IsType<FunctionObj>(evaluated);
        FunctionObj functionObj = (FunctionObj)evaluated;
        
        Assert.Single(functionObj.Parameters);
        
        Assert.Equal("x", functionObj.Parameters[0].String());
        
        Assert.Equal("(x + 2)", functionObj.Body.String());
    }

    struct FunctionApplicationTest {
        public string input;
        public int expected;
    }
    [Fact]
    public void TestFunctionApplication() {
        FunctionApplicationTest[] tests = {
            new() { input = "let identity = fn(x) { x; }; identity(5);", expected = 5 },
            new() { input = "let identity = fn(x) { return x; }; identity(5);", expected = 5 },
            new() { input = "let double = fn(x) { x * 2; }; double(5);", expected = 10 },
            new() { input = "let add = fn(x, y) { x + y; }; add(5, 5);", expected = 10 },
            new() { input = "let add = fn(x, y) { x + y; }; add(5 + 5, add(5, 5));", expected = 20 },
            new() { input = "fn(x) { x; }(5)", expected = 5 },
        };

        foreach (var test in tests) {
            Assert.True(testIntegerObject(testEval(test.input), test.expected));
        }
    }

    [Fact]
    public void TestEnclosingEnvironment() {
        string input = @"
            let first = 10;
            let second = 10;
            let third = 10;

            let ourFunction = fn(first) {
              let second = 20;

              first + second + third;
            };

            ourFunction(20) + first + second;
        ";
        Assert.True(testIntegerObject(testEval(input), 70));
    }

    private Object.Object testEval(string input) {
        Lexer lexer = Lexer.NewInstance(input);
        Parser parser = Parser.NewInstance(lexer);
        AbstractSyntaxTree tree = parser.ParseAST();
        Environment environment = Environment.NewEnvironment();
        return Evaluator.Eval(tree, environment);
    }

    private bool testBooleanObject(Object.Object obj, bool expected) {
        if (obj is BooleanObj booleanObj) {
            if (booleanObj.Value != expected) {
                _testOutputHelper.WriteLine($"obj has wrong value. Got={booleanObj.Value}, want={expected}.");
                return false;
            }

            return true;
        }

        _testOutputHelper.WriteLine($"obj is not BooleanObj. Got={obj}");
        return false;
    }

    private bool testNullObject(Object.Object obj) {
        if (obj != RepeatedPrimitives.NULL) {
            _testOutputHelper.WriteLine($"object is not NULL. Got={obj}");
            return false;
        }

        return true;
    }

    private bool testIntegerObject(Object.Object obj, int expected) {
        if (obj is IntegerObj integerObj) {
            if (integerObj.Value != expected) {
                _testOutputHelper.WriteLine($"Object has wrong value. Got={integerObj.Value}, want={expected}");
                return false;
            }

            return true;
        }
        _testOutputHelper.WriteLine($"Object is not BooleanObj. Got={obj}");
        return false;
    }
}