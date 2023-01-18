using AquariusLang.lexer;
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

            for (Token token = lexer.NextToken(); token.Type != TokenType.EOF; token = lexer.NextToken()) {
                Console.WriteLine($"{token}\n");
            }
        }
    }
}