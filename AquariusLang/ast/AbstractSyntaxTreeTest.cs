using AquariusLang.token;
using Xunit;
using Xunit.Abstractions;

namespace AquariusLang.ast; 

public class AbstractSyntaxTreeTest {
    /// <summary>
    /// For logging outputs during testing.
    /// </summary>
    private readonly ITestOutputHelper _testOutputHelper;
    
    public AbstractSyntaxTreeTest(ITestOutputHelper testOutputHelper) {
        _testOutputHelper = testOutputHelper;
    }
    
    [Fact]
    public void TestString() {
        AbstractSyntaxTree abstractSyntaxTree = new AbstractSyntaxTree(new[] {
            new LetStatement(
                new Token() { Type = TokenType.LET, Literal = "let", },
                new Identifier(new Token() { Type = TokenType.IDENT, Literal = "myVar", }, "myVar"),
                new Identifier(new Token() { Type = TokenType.IDENT, Literal = "anotherVar", }, "anotherVar"))
        });
        
        _testOutputHelper.WriteLine(abstractSyntaxTree.String());
        _testOutputHelper.WriteLine("let myVar = anotherVar;");
        
        Assert.Equal(abstractSyntaxTree.String(), "let myVar = anotherVar;");
    }
}