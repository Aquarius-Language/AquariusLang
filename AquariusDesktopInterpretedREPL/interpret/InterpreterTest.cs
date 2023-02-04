using AquariusLang.Object;
using Xunit;
using Xunit.Abstractions;

namespace AquariusREPL.interpret; 

public class InterpreterTest {
    /// <summary>
    /// For logging outputs during testing.
    /// </summary>
    private readonly ITestOutputHelper _testOutputHelper;

    public InterpreterTest(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void TestForLoop() {
        IObject evaluated = Interpreter.Interpret("../../../examples/for_loop.aqua");
        Assert.IsType<IntegerObj>(evaluated);
        Assert.Equal(-20, ((IntegerObj)evaluated).Value);
    }

    [Fact]
    public void TestNestedFunc() {
        IObject evaluated = Interpreter.Interpret("../../../examples/nested_func.aqua");
        Assert.IsType<IntegerObj>(evaluated);
        Assert.Equal(59, ((IntegerObj)evaluated).Value);
    }

    [Fact]
    public void TestNumOperationsCasting() {
        IObject evaluated = Interpreter.Interpret("../../../examples/num_operations_casting.aqua");
        Assert.IsType<ArrayObj>(evaluated);
        ArrayObj evaluatedArr = (ArrayObj)evaluated;
        Assert.True(testArrayObjEquals(evaluatedArr.Elements, new IObject[]{new IntegerObj(-20), new FloatObj(20.38f)}));
    }

    private bool testArrayObjEquals(IObject[] a, IObject[] b) {
        if (a.Length != b.Length) return false;
        bool same = true;
        for (var i = 0; i < a.Length; i++) {
            if (a[i].GetType() != b[i].GetType()) {
                _testOutputHelper.WriteLine($"Not same type: ${a[i].GetType()}, ${b[i].GetType()}");
            }
            if (a[i].Inspect() != b[i].Inspect()) {
                same = false;
                _testOutputHelper.WriteLine($"Not same value: ${a[i].Inspect()}, ${b[i].Inspect()}");
                break;
            } 
        }

        return same;
    }
}