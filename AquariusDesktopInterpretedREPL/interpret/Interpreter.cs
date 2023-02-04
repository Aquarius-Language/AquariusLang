using AquariusLang.ast;
using AquariusLang.evaluator;
using AquariusLang.lexer;
using AquariusLang.Object;
using AquariusLang.parser;
using Environment = AquariusLang.Object.Environment;

namespace AquariusREPL.interpret; 

public class Interpreter {
    const string PROMPT = ">> ";

    /// <summary>
    /// Read, Evaluate, Print, Loop.
    /// </summary>
    public static void REPL() {
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

            Evaluator evaluator = Evaluator.NewInstance(new DesktopBuiltins());
            IObject evaluated = evaluator.Eval(tree, environment);
            
            /*
             * Note: C#'s null shouldn't be printed out; but NullObj needs to be printed out.
             */
            if (evaluated != null) {
                Console.WriteLine(evaluated.Inspect());
            }
        }
    }

    /// <summary>
    /// Interpret given filename.
    /// </summary>
    /// <param name="fileName">Path of file.</param>
    public static IObject Interpret(string fileName) {
        Environment environment = Environment.NewEnvironment();
        
        string contents = File.ReadAllText(fileName);
        Lexer lexer = Lexer.NewInstance(contents);
        Parser parser = Parser.NewInstance(lexer);
        AbstractSyntaxTree tree = parser.ParseAST();
        if (parser.Errors.Count != 0) {
            printParserErrors(parser.Errors.ToArray());
            return null;
        }
        Evaluator evaluator = Evaluator.NewInstance(new DesktopBuiltins());
        IObject evaluated = evaluator.Eval(tree, environment);
        if (evaluated != null) {
            Console.WriteLine(evaluated.Inspect());
        }

        return evaluated;
    }

    private static void printParserErrors(string[] errors) {
        Console.WriteLine("Parser errors:");
        foreach (var error in errors) {
            Console.WriteLine($"\t{error}");
        }
    }
}