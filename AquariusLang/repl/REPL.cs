using AquariusLang.ast;
using AquariusLang.lexer;
using AquariusLang.parser;
using AquariusLang.token;

namespace AquariusLang.repl; 

/// <summary>
/// Read, Evaluate, Print, Loop.
/// </summary>
public class REPL {
    const string PROMPT = ">> ";
        
    public static void Start() {
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
            
            Console.WriteLine(tree.String());
            Console.WriteLine();
        }
    }

    private static void printParserErrors(string[] errors) {
        Console.WriteLine("Parser errors:");
        foreach (var error in errors) {
            Console.WriteLine($"\t{error}");
        }
    }
}