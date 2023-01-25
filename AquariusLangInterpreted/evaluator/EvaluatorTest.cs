﻿using AquariusLang.ast;
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
    
    struct EvalNumberTest {
        public string input;
        public object expected;
    }
    [Fact]
    public void TestEvalNumberExpression() {
        EvalNumberTest[] tests = {
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
            new () {input = "(7.3f + 2.1f) * 2 + -10.2f", expected = 8.6f},
            new () {input = "1.f / 5.f * 3 + 11", expected = 11.6f},
            new () {input = "(7.3f + 2.1d) * 2 + -10.2f", expected = 8.6},
            new () {input = "1.d / 5.f * 3 + 11", expected = 11.6},
            new () {input = "1.f * 33 + 2", expected = 35.0f},
        };

        foreach (var test in tests) {
            IObject evaluated = testEval(test.input);
            if (test.expected is int _expected) {
                Assert.IsType<IntegerObj>(evaluated);
                Assert.True(testIntegerObject(evaluated, (int?)test.expected));
            } else if (test.expected is float __expected) {
                Assert.IsType<FloatObj>(evaluated);
                Assert.True(testFloatObject(evaluated, (float?)test.expected, 2));
            } else if (test.expected is double ___expected) {
                Assert.IsType<DoubleObj>(evaluated);
                Assert.True(testDoubleObject(evaluated, (double?)test.expected, 2));
            }
        }
    }

    struct NumberPlusMinusMultDivEqualsTest {
        public string input;
        public object expected;
    }
    [Fact]
    public void TestNumberPlusMinusMultDivEquals() {
        NumberPlusMinusMultDivEqualsTest[] tests = {
            new() {input = "let a = 5; a += 10; a;", expected = 15},
            new() {input = "let b = 25; b -= 5; b -= 19; b;", expected = 1},
            new() {input = "let c = 1; c *= 5; c;", expected = 5},
            new() {input = "let d = 20; d /= 4; d;", expected = 5},
            new() {input = "let e = 25.4f; e /= 2.d; e;", expected = 12.7f},
            new() {input = "let f = 18.3d; f *= 3; f;", expected = 54.9},
        };
        foreach (var test in tests) {
            IObject evaluated = testEval(test.input);
            if (test.expected is int _expected) {
                Assert.IsType<IntegerObj>(evaluated);
                Assert.True(testIntegerObject(evaluated, _expected));
            } else if (test.expected is float __expected) {
                Assert.IsType<FloatObj>(evaluated);
                Assert.True(testFloatObject(evaluated, __expected, 2));
            } else if (test.expected is double ___expected) {
                Assert.IsType<DoubleObj>(evaluated);
                Assert.True(testDoubleObject(evaluated, ___expected, 2));
            }
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
            new () {input = "1 > 2.5f", expected = false},
            new () {input = "1 < 1", expected = false},
            new () {input = "1 > 1.1d", expected = false},
            new () {input = "1 == 1", expected = true},
            new () {input = "1 != 1", expected = false},
            new () {input = "1 == 2.f", expected = false},
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
            new () {input = "true && false", expected = false},
            new () {input = "true || false", expected = true},
            new () {input = "false || false", expected = false},
            new () {input = "false && false", expected = false},
            new () {input = "true && true", expected = true},
            new () {input = "true && false", expected = false},
            new () {input = "!5 && false", expected = false},
            new () {input = "123.301f < 123.302f", expected = true},
            new () {input = "0.9d > 0.91d", expected = false},
            new () {input = "1.111f > 1.11d", expected = true},
            new () {input = "1.111f != 1.11d", expected = true},
            new () {input = "1.111f != 1.111d", expected = true},
        };

        foreach (var test in tests) {
            IObject evaluated = testEval(test.input);
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
            new () {input = "!!6.3f", expected = true},
        };
        foreach (var test in tests) {
            IObject evaluated = testEval(test.input);
            Assert.True(testBooleanObject(evaluated, test.expected));
        }
    }

    struct ForLoopTest {
        public string input;
        public object expected;
    }

    [Fact]
    public void TestEvalForLoop() {
        ForLoopTest[] tests = {
            new () {
                input = 
                    @"
                    # This one tests for break statement in for loop.
                    let a = 0;
                    for (let j = 0; j <= 10; j += 2) {
                        a += 2;
                        if (j == 8){
                            a += 10;
                            break;
                        }
                    }
                    a;
                    ",
                expected = 20,
            },
            new () {
                input = 
                    @"
                    # This one tests for return statement in for loop.
                    let a = """";
                    a = fn() {
                        for (let j = 0; j <= 10; j += 2) {
                            a += ""XD!"";
                            if (j == 4){
                                a += ""Wow!"";
                                return a;
                            }
                        }
                    }();
                    a;
                    ",
                expected = "XD!XD!XD!Wow!",
            }
        };
        foreach (ForLoopTest test in tests) {
            IObject result = testEval(test.input);
            if (result.Type() == ObjectType.INTEGER_OBJ) {
                Assert.Equal(test.expected, ((IntegerObj)result).Value);
            } else if (result.Type() == ObjectType.STRING_OBJ) {
                Assert.Equal(test.expected, ((StringObj)result).Value);
            }
        }
    }

    struct IfElseTest {
        public string input;
        public object expected;
    }
    [Fact]
    public void TestIfElifElseExpressions() {
        IfElseTest[] tests = {
            new () {input = "if (true) { 10 }", expected = 10},
            new () {input = "if (false) { 10 }", expected = null},
            new () {input = "if (1) { 10 }", expected = 10},
            new () {input = "if (1 < 2) { 10 }", expected = 10},
            new () {input = "if (1 > 2) { 10 }", expected = null},
            new () {input = "if (1 > 2) { 10 } else { 20.12f }", expected = 20.12f},
            new () {input = "if (1 < 2) { 10 } else { 20 }", expected = 10},
            new () {
                input = @"
                        let a = false;
                        let b = true;

                        let result = fn() {
                            if (a) {
                                return 12;
                            } elif (10 > 12) {
                                return true;
                            } elif (b) {
                                return ""YES!"";
                            } else {
                                return ""Wow!"";
                            }
                        }();

                        result;
                        ",
                expected = "YES!"
            },
        };
        foreach (var test in tests) {
            IObject evaluated = testEval(test.input);
            _testOutputHelper.WriteLine(test.input);
            if (test.expected is int) {
                int integer = (int)test.expected;
                Assert.True(testIntegerObject(evaluated, integer));
            } else if (test.expected is float _expected) {
                Assert.True(testFloatObject(evaluated, _expected, 2));
            } else if (test.expected is string) {
                Assert.True(testStringObject(evaluated, (string?)test.expected));
            } else {
                /*
				 When a conditional doesn’t evaluate to a value it’s supposed to
				 return NULL, e.g.: if (false) { 10 }
			    */
                Assert.True(testNullObject(evaluated));
            }
        }
    }

    struct VarReassignmentTest {
        public string input;
        public int expected;
    }

    [Fact]
    public void TestVarReassignments() {
        VarReassignmentTest[] tests = {
            new () {input = "let a = 0; a = 5; a;", expected = 5},
            new () {
            input = @"
                    let a = ""Wow!"";
                    let func = fn(){for(let a = 5; a < 10; a+=1) {for (let b = 0; b < 5; b+=1){if (a > 6) {return a;}}}}
                    a = func();
                    a;
                    ",
            expected = 7
            },
        };
        foreach (VarReassignmentTest test in tests) {
            IObject result = testEval(test.input);
            Assert.IsType<IntegerObj>(result);
            Assert.True(testIntegerObject(result, test.expected));
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
            new () { // Return from for loop to out of function.
                input = 
                @"
                let func = fn(){
                    for(let a = 5; a < 10; a+=1) {
                        if (a > 6) {
                            return a;
                        }
                    }
                }
                func();
                ",
                expected = 7,
            },
            new () { // Return from nested for loop to out of function.
            input = 
            @"
                let func = fn(){
                    for(let a = 5; a < 10; a+=1) {
                        for (let b = 0; b < 5; b+=1) {
                            if (a > 6) {
                                return a;
                            }
                        }
                    }
                }
                func();
                ",
            expected = 7,
            }
        };

        foreach (var test in tests) {
            IObject evaluated = testEval(test.input);
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
            new() { input = "\"Hello\" - \"World\"", expectedMessage = "Unknown operator: STRING - STRING" },
            new() { input = "{\"name\": \"Monkey\"}[fn(x) { x }];", expectedMessage = "Unusable as hash key: FUNCTION" }, 
        };

        foreach (var test in tests) {
            IObject evaluated = testEval(test.input);
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
    
    
    struct VariableReassignmentTest {
        public string input;
        public object expectedValue;
    }
    [Fact]
    public void TestVariableReassignment() {
        VariableReassignmentTest[] tests = {
            new() { input = "let a = 14; a = a + 1; a;", expectedValue = 15 },
            new() { input = "let b = \"Hello, world!\"; b = \"Wow!\"; b;", expectedValue = "Wow!"},
        };
        foreach (var test in tests) {
            IObject evalResult = testEval(test.input);
            if (evalResult.Type() == ObjectType.INTEGER_OBJ) {
                Assert.True(testIntegerObject(evalResult, (int?)test.expectedValue));
            } else if (evalResult.Type() == ObjectType.STRING_OBJ) {
                Assert.True(testStringObject(evalResult, (string?)test.expectedValue));
            }
        }
    }

    [Fact]
    public void TestFunctionObject() {
        string input = "fn(x) { x + 2; };";
        IObject evaluated = testEval(input);

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

    struct VariableScopeAndOwnershipTest {
        public string input;
        public object expected;
    }
    [Fact]
    public void TestVariableScopeAndOwnership() {
        VariableScopeAndOwnershipTest[] tests = {
            new () {
                input = 
                    @"
                    let a = 5;
                    let func = fn(){let a = 10;}
                    func();
                    a;
                    ",
                expected = 5
            },
            new () {
                input = 
                    @"
                    let a = 5;
                    let func = fn(){a = 10;}
                    func();
                    a;
                    ",
                expected = 10
            },
            new () {
                input = 
                    @"
                    let a = ""Hello"";
                    let func = fn(){
                        a += "" World!"";
                    }
                    func();
                    a;
                    ",
                expected = "Hello World!",
            },
            new () {
                input = 
                    @"
                    let a = 10;
                    
                    let func = fn() {
                        for (let i = 0; i < 5; i+=1) {
                            for (let j = 0; j < 5; j+=1) {
                                a += i;                                
                            }
                        }
                    }
                    func();
                    a;
                    ",
                expected = 60,
            }
        };
        foreach (VariableScopeAndOwnershipTest test in tests) {
            IObject result = testEval(test.input);
            Assert.IsNotType<ErrorObj>(result);
            switch (result.Type()) {
                case ObjectType.INTEGER_OBJ:
                    Assert.True(testIntegerObject(result, (int) test.expected));
                    break;
                case ObjectType.STRING_OBJ:
                    Assert.True(testStringObject(result, (string) test.expected));
                    break;
            }
        }
    }

    [Fact]
    public void TestStringLiteral() {
        string input = "\"Hello World!\"";
        IObject evaluated = testEval(input);
        Assert.IsType<StringObj>(evaluated);
        StringObj stringObj = (StringObj)evaluated;
        Assert.Equal("Hello World!", stringObj.Value);
    }

    [Fact]
    public void TestStringConcatenation() {
        string input = "\"Hello\" + \" \" + \"World!\"";
        IObject evaluated = testEval(input);
        
        Assert.IsType<StringObj>(evaluated);
        StringObj stringObj = (StringObj)evaluated;
        
        Assert.Equal("Hello World!", stringObj.Value);
    }

    struct BuiltinFunctionTest {
        public string input;
        public object expected;
    }

    [Fact]
    public void TestBuiltinFunctions() {
        BuiltinFunctionTest[] tests = {
            new () {input = "len(\"\")", expected = 0},
            new () {input = "len(\"four\")", expected = 4},
            new () {input = "len(\"hello world\")", expected = 11},
            new () {input = "len(1)", expected = "Argument to `len` not supported, got INTEGER"},
            new () {input = "len(\"one\", \"two\")", expected = "Wrong number of arguments. got=2, want=1"},
        };

        foreach (var test in tests) {
            IObject evaluated = testEval(test.input);
            Type evaluatedType = evaluated.GetType();
            if (evaluatedType == typeof(int)) {
                Assert.True(testIntegerObject(evaluated, (int)test.expected));
            } else if (evaluatedType == typeof(string)) {
                Assert.IsType<ErrorObj>(evaluated);

                ErrorObj errorObj = (ErrorObj)evaluated;
                Assert.Equal(test.expected, errorObj.Message);
            }
        }
    }

    [Fact]
    public void TestArrayLiterals() {
        string input = "[1, 2 * 2, 3 + 3]";
        IObject evaluated = testEval(input);
        
        Assert.IsType<ArrayObj>(evaluated);
        ArrayObj result = (ArrayObj)evaluated;
        
        Assert.Equal(3, result.Elements.Length);
        
        Assert.True(testIntegerObject(result.Elements[0], 1));
        Assert.True(testIntegerObject(result.Elements[1], 4));
        Assert.True(testIntegerObject(result.Elements[2], 6));
    }

    struct ArrayIndexTest {
        public string input;
        public object expected;
    }

    [Fact]
    public void TestArrayIndexExpressions() {
        ArrayIndexTest[] tests = {
            new () { input = "[1, 2, 3][0]", expected = 1, },
            new () { input = "[1, 2, 3][1]", expected = 2, },
            new () { input = "[1, 2, 3][2]", expected = 3, },
            new () { input = "let i = 0; [1][i];", expected = 1, },
            new () { input = "[1, 2, 3][1 + 1];", expected = 3, },
            new () { input = "let myArray = [1, 2, 3]; myArray[2];", expected = 3, },
            new () { input = "let myArray = [1, 2, 3]; myArray[0] + myArray[1] + myArray[2];", expected = 6, },
            new () { input = "let myArray = [1, 2, 3]; let i = myArray[0]; myArray[i]", expected = 2, },
            new () { input = "[1, 2, 3][3]", expected = null, },
            new () { input = "[1, 2, 3][-1]", expected = null, },
        };

        foreach (var test in tests) {
            IObject evaluated = testEval(test.input);
            if (test.expected == null) {
                Assert.True(testNullObject(evaluated));
            } else {
                Assert.True(testIntegerObject(evaluated, (int)test.expected));
            }
        }
    }

    [Fact]
    public void TestHashLiterals() {
        string input = @"
        let two = ""two"";
        {
            ""one"": 10 - 9,
            two: 1 + 1,
            ""thr"" + ""ee"": 6 / 2,
            4: 4,
            true: 5,
            false: 6
        }
        ";
        IObject evaluated = testEval(input);
        Assert.IsType<HashObj>(evaluated);
        HashObj result = (HashObj)evaluated;

        Dictionary<HashKey, int> expected = new() {
            { (new StringObj("one")).HashKey(), 1 },
            { (new StringObj("two")).HashKey(), 2 },
            { (new StringObj("three")).HashKey(), 3 },
            { (new IntegerObj(4)).HashKey(), 4 },
            { RepeatedPrimitives.TRUE.HashKey(), 5 },
            { RepeatedPrimitives.FALSE.HashKey(), 6 }
        };
        
        Assert.Equal(expected.Count, result.Pairs.Count);
        
        foreach (var _expected in expected) {
            Assert.True(result.Pairs.TryGetValue(_expected.Key, out HashPair value));
            Assert.True(testIntegerObject(value.Value, _expected.Value));
        }
    }

    struct HashIndexExpressionTest {
        public string input;
        public int? expected;
    }
    [Fact]
    public void TestHashIndexExpressions() {
        HashIndexExpressionTest[] tests = {
            new() {
                input = "{\"foo\": 5}[\"foo\"]",
                expected = 5,
            },
            new() {
                input = "{\"foo\": 5}[\"bar\"]",
                expected = null,
            },
            new() {
                input = "let key = \"foo\"; {\"foo\": 5}[key]",
                expected = 5,
            },
            new() {
                input = "{}[\"foo\"]",
                expected = null,
            },
            new() {
                input = "{5: 5}[5]",
                expected = 5,
            },
            new() {
                input = "{true: 5}[true]",
                expected = 5,
            },
            new() {
                input = "{false: 5}[false]",
                expected = 5,
            },
        };

        foreach (var test in tests) {
            _testOutputHelper.WriteLine($"input: {test.input}, expected: {test.expected}");
            IObject evaluated = testEval(test.input);
            if (test.expected != null) {
                Assert.True(testIntegerObject(evaluated, test.expected));
            } else {
                Assert.True(testNullObject(evaluated));
            }
        }
    }

    private IObject testEval(string input) {
        Lexer lexer = Lexer.NewInstance(input);
        Parser parser = Parser.NewInstance(lexer);
        AbstractSyntaxTree tree = parser.ParseAST();
        Environment environment = Environment.NewEnvironment();
        return Evaluator.Eval(tree, environment);
    }

    private bool testBooleanObject(IObject obj, bool expected) {
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

    private bool testNullObject(IObject obj) {
        if (obj != RepeatedPrimitives.NULL) {
            _testOutputHelper.WriteLine($"object is not NULL. Got={obj}");
            return false;
        }

        return true;
    }

    private bool testIntegerObject(IObject obj, int? expected) {
        if (obj is IntegerObj integerObj) {
            if (integerObj.Value != expected) {
                _testOutputHelper.WriteLine($"Object has wrong value. Got={integerObj.Value}, want={expected}");
                return false;
            }

            return true;
        }
        _testOutputHelper.WriteLine($"Object is not IntegerObj. Got={obj}");
        return false;
    }

    private bool testFloatObject(IObject obj, float? expected, int roundPlace) {
        if (obj is FloatObj floatObj) {
            if (Math.Round(floatObj.Value, roundPlace) != Math.Round((float)expected, roundPlace)) {
                _testOutputHelper.WriteLine($"Object has wrong value. Got={floatObj.Value}, want={expected}");
                return false;
            }

            return true;
        }
        
        _testOutputHelper.WriteLine($"Object is not FloatObj. Got={obj}");
        return false;
    }

    private bool testDoubleObject(IObject obj, double? expected, int roundPlace) {
        if (obj is DoubleObj doubleObj) {
            if (Math.Round(doubleObj.Value, roundPlace) != Math.Round((double)expected, roundPlace)) {
                _testOutputHelper.WriteLine($"Object has wrong value. Got={doubleObj.Value}, want={expected}");
                return false;
            }

            return true;
        }
        
        _testOutputHelper.WriteLine($"Object is not DoubleObj. Got={obj}");
        return false;
    }

    private bool testStringObject(IObject obj, string? expected) {
        if (obj is StringObj stringObj) {
            if (stringObj.Value != expected) {
                _testOutputHelper.WriteLine($"Object has wrong value. Got={stringObj.Value}, want={expected}");
                return false;
            }

            return true;
        }
        _testOutputHelper.WriteLine($"Object is not StringObj. Got={obj}");
        return false;
    }
}