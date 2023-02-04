using System.Diagnostics;
using System.Text;
using AquariusLang.ast;
using AquariusLang.evaluator;
using AquariusLang.lexer;
using AquariusLang.Object;
using AquariusLang.parser;
using AquariusLang.utils;
using Environment = AquariusLang.Object.Environment;

namespace AquariusREPL;

public class DesktopBuiltins : Builtins {
    public DesktopBuiltins() {
        builtins = new Dictionary<string, BuiltinObj> {
            {
                "len", new BuiltinObj(args => {
                    ErrorObj argsCountMatch = checkArgsCount("len", 1, args.Length);
                    if (argsCountMatch != null) return argsCountMatch;

                    var arg0 = args[0];
                    var arg0Type = arg0.GetType();
                    if (arg0Type == typeof(StringObj)) {
                        var arg0StrObj = (StringObj)arg0;
                        return new IntegerObj(arg0StrObj.Value.Length);
                    }

                    if (arg0Type == typeof(ArrayObj)) {
                        var arg0ArrObj = (ArrayObj)arg0;
                        return new IntegerObj(arg0ArrObj.Elements.Length);
                    }

                    return newError($"Argument to `len` not supported, got {arg0.Type()}");
                })
            }, {
                "last", new BuiltinObj(args => {
                    ErrorObj argsCountMatch = checkArgsCount("last", 1, args.Length);
                    if (argsCountMatch != null) return argsCountMatch;
                    
                    if (args[0].Type() != ObjectType.ARRAY_OBJ)
                        return newError($"Argument to `last` must be ARRAY, got {args[0].Type()}");

                    var array = (ArrayObj)args[0];
                    var length = array.Elements.Length;

                    return length > 0 ? array.Elements[length - 1] : RepeatedPrimitives.NULL;
                })
            }, {
                "rest", new BuiltinObj(args => {
                    ErrorObj argsCountMatch = checkArgsCount("rest", 1, args.Length);
                    if (argsCountMatch != null) return argsCountMatch;
                    
                    if (args[0].Type() != ObjectType.ARRAY_OBJ)
                        return newError($"Argument to `rest` must be ARRAY, got {args[0].Type()}");
                    var arrayObj = (ArrayObj)args[0];
                    var length = arrayObj.Elements.Length;
                    if (length > 0) {
                        var newElements = arrayObj.Elements.Skip(1).ToArray();
                        return new ArrayObj(newElements);
                    }

                    return RepeatedPrimitives.NULL;
                })
            }, {
                "push", new BuiltinObj(args => {
                    ErrorObj argsCountMatch = checkArgsCount("push", 2, args.Length);
                    if (argsCountMatch != null) return argsCountMatch;
                    
                    if (args[0].Type() != ObjectType.ARRAY_OBJ)
                        return newError($"Argument to `push` must be ARRAY, got {args[0].Type()}");

                    var arrayObj = (ArrayObj)args[0];
                    var newElements = Utils.PushToArray(arrayObj.Elements, args[1]);

                    return new ArrayObj(newElements);
                })
            }, {
                "print", new BuiltinObj(args => {
                    for (var i = 0; i < args.Length; i++) {
                        Console.Write(args[i].Inspect());
                        if (i < args.Length - 1) Console.Write(" ");
                    }

                    Console.WriteLine();

                    return null;
                })
            }, { 
                "import", new BuiltinObj(args => {
                    ErrorObj argsCountMatch = checkArgsCount("import", 1, args.Length);
                    if (argsCountMatch != null) return argsCountMatch;
                    
                    if (args[0] is StringObj stringObj) {
                        try {
                            String fileStr = File.ReadAllText(stringObj.Value);
                            Lexer lexer = Lexer.NewInstance(fileStr);
                            Parser parser = Parser.NewInstance(lexer);
                            AbstractSyntaxTree tree = parser.ParseAST();
                            Evaluator evaluator = Evaluator.NewInstance(new DesktopBuiltins());
                            Environment moduleEnv = Environment.NewEnvironment();
                            evaluator.Eval(tree, moduleEnv);
                            return new ModuleObj(moduleEnv);
                        } catch (FileNotFoundException e) {
                            Console.WriteLine($"Exception error: Could not find file '${e.FileName}'");
                        }
                    } else {
                        return newError($"Argument 0 in built-in function 'import' not STRING.");
                    }

                    return null;
                }) 
            }, {
               "isOSWindows", new BuiltinObj(args => {
                   ErrorObj argsCountMatch = checkArgsCount("isOSWindows", 0, args.Length);
                   if (argsCountMatch != null) return argsCountMatch;

                   return new BooleanObj(OperatingSystem.IsWindows());
               }) 
            }, {
                "isOSLinux", new BuiltinObj(args => {
                    ErrorObj argsCountMatch = checkArgsCount("isOSLinux", 0, args.Length);
                    if (argsCountMatch != null) return argsCountMatch;

                    return new BooleanObj(OperatingSystem.IsLinux());
                }) 
            }, {
                "isOSMacOS", new BuiltinObj(args => {
                    ErrorObj argsCountMatch = checkArgsCount("isOSMacOS", 0, args.Length);
                    if (argsCountMatch != null) return argsCountMatch;
                    
                    return new BooleanObj(OperatingSystem.IsMacOS());
                }) 
            }, {
                "execFile", new BuiltinObj(args => { // Executes file synchronously.
                    ErrorObj argsCountMatch = checkArgsCount("execFile", 2, args.Length);
                    if (argsCountMatch != null) return argsCountMatch;
                    
                    StringBuilder builder = new StringBuilder();
                    ArrayObj args1Arr = (ArrayObj)args[1];
                    foreach (var args1ArrElement in args1Arr.Elements) {
                        builder.Append(((StringObj)args1ArrElement).Value).Append(' ');
                    }

                    string arguments = builder.ToString();

                    Process p = new Process();
                    p.StartInfo.FileName = ((StringObj)args[0]).Value;
                    p.StartInfo.Arguments = arguments;

                    bool started = p.Start();
                    if (!started) {
                        return RepeatedPrimitives.FALSE;
                    }

                    p.WaitForExit();
                    
                    return new BooleanObj(true);
                }) 
            }
        };
    }

    private ErrorObj checkArgsCount(string funcName, int expected, int actual) {
        if (expected != actual) {
            return newError($"Wrong number of arguments for '${funcName}'. Got{actual}, want ${expected}.");
        }

        return null;
    }
}