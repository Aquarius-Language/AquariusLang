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
            Assert.Equal(testIntegerObject(evaluated, test.expected), true);
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
            Assert.Equal(testBooleanObject(evaluated, test.expected), true);
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
            Assert.Equal(testBooleanObject(evaluated, test.expected), true);
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
                Assert.Equal(testIntegerObject(evaluated, integer), true);
            } else {
                /*
				 When a conditional doesn’t evaluate to a value it’s supposed to
				 return NULL, e.g.: if (false) { 10 }
			    */
                Assert.Equal(testNullObject(evaluated), true);
            }
        }
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