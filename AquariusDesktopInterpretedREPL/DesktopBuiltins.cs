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
                    if (args.Length != 1)
                        return newError($"Wrong number of arguments. Got={args.Length}, want=1");

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
                    if (args.Length != 1)
                        return newError($"Wrong number of arguments. Got{args.Length}, want 1.");

                    if (args[0].Type() != ObjectType.ARRAY_OBJ)
                        return newError($"Argument to `last` must be ARRAY, got {args[0].Type()}");

                    var array = (ArrayObj)args[0];
                    var length = array.Elements.Length;

                    return length > 0 ? array.Elements[length - 1] : RepeatedPrimitives.NULL;
                })
            }, {
                "rest", new BuiltinObj(args => {
                    if (args.Length != 1)
                        return newError($"Wrong number of arguments. Got{args.Length}, want 1.");
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
                    if (args.Length != 2)
                        return newError($"Wrong number of arguments. Got{args.Length}, want 2.");

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
                    if (args.Length != 1) {
                        return newError($"Wrong number of arguments. Got{args.Length}, want 1.");
                    }
                    
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
                   if (args.Length != 0) {
                       return newError($"Wrong number of arguments. Got{args.Length}, want 0.");
                   }

                   return new BooleanObj(OperatingSystem.IsWindows());
               }) 
            }, {
                "isOSLinux", new BuiltinObj(args => {
                    if (args.Length != 0) {
                        return newError($"Wrong number of arguments. Got{args.Length}, want 0.");
                    }

                    return new BooleanObj(OperatingSystem.IsLinux());
                }) 
            }, {
                "isOSMacOS", new BuiltinObj(args => {
                    if (args.Length != 0) {
                        return newError($"Wrong number of arguments. Got{args.Length}, want 0.");
                    }
                    
                    return new BooleanObj(OperatingSystem.IsMacOS());
                }) 
            }, {
                "execFile", new BuiltinObj(args => {
                    if (args.Length != 2) {
                        return newError($"Wrong number of arguments. Got{args.Length}, want 2.");
                    }
                    
                    StringBuilder builder = new StringBuilder();
                    ArrayObj args1Arr = (ArrayObj)args[1];
                    foreach (var args1ArrElement in args1Arr.Elements) {
                        builder.Append(((StringObj)args1ArrElement).Value).Append(' ');
                    }

                    string arguments = builder.ToString();

                    Process p = new Process();
                    p.StartInfo.FileName = ((StringObj)args[0]).Value;
                    p.StartInfo.Arguments = arguments;
                    return new BooleanObj(p.Start());
                }) 
            }
        };
    }
}