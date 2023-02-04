using AquariusLang.Object;
using AquariusLang.utils;

namespace AquariusLang.evaluator;

public class Builtins {
    protected Dictionary<string, BuiltinObj> builtins;

    public Builtins(Dictionary<string, BuiltinObj> builtins) {
        this.builtins = builtins;
    }

    public Builtins() {
        builtins = new Dictionary<string, BuiltinObj>();
    }
    
    protected static ErrorObj newError(string msg) {
        return new ErrorObj(msg);
    }

    public Dictionary<string, BuiltinObj> _Builtins => builtins;
}