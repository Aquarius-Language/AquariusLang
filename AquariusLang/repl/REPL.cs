using AquariusLang.ast;
using AquariusLang.evaluator;
using AquariusLang.lexer;
using AquariusLang.Object;
using AquariusLang.parser;
using AquariusLang.token;
using Environment = AquariusLang.Object.Environment;

namespace AquariusLang.repl; 

/// <summary>
/// Read, Evaluate, Print, Loop.
/// </summary>
public class REPL {
    const string PROMPT = ">> ";
        
    public static void Start() {
        Environment environment = Environment.NewEnvironment();
        
        while (true) {
            Console.Write(PROMPT);
            
            string? line = Console.ReadLine();
            Lexer lexer = Lexer.NewInstance(line);
            Parser parser = Parser.NewInstance(lexer);
            AbstractSyntaxTree tree = parser.ParseAST();

            if (parser.Errors.Count != 0) {
                printParserErrors(parser.Errors.ToArray());
                continue;
            }

            IObject evaluated = Evaluator.Eval(tree, environment);
            if (evaluated != null) {
                Console.WriteLine(evaluated.Inspect());
            }
        }
    }

    private static void printParserErrors(string[] errors) {
        Console.WriteLine("Parser errors:");
        foreach (var error in errors) {
            Console.WriteLine($"\t{error}");
        }
    }
}