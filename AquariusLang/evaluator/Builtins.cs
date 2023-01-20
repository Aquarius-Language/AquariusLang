using AquariusLang.Object;

namespace AquariusLang.evaluator; 

public class Builtins {
    public static readonly Dictionary<string, BuiltinObj> builtins = new() {
        {
            "len", new BuiltinObj((args) => {
                if (args.Length != 1) {
                    return Evaluator.NewError($"Wrong number of arguments. Got={args.Length}, want=1");
                }

                Object.Object arg0 = args[0];
                Type arg0Type = arg0.GetType();
                if (arg0Type == typeof(StringObj)) {
                    StringObj arg0StrObj = (StringObj)arg0;
                    return new IntegerObj(arg0StrObj.Value.Length);
                }

                return Evaluator.NewError($"Argument to `len` not supported, got {arg0.Type()}");
            })
        }
    };
}