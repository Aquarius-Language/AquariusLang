namespace AquariusLang.Object; 

public class Environment {
    private Dictionary<string, IObject> store;
    private Environment outer;


    public static Environment NewEnclosedEnvironment(Environment outer) {
        Environment environment = new Environment() { store = new Dictionary<string, IObject>(), outer = outer };
        return environment;
    }

    public static Environment NewEnvironment() {
        Environment environment = new Environment() { store = new Dictionary<string, IObject>(), outer = null };
        return environment;
    }

    /// <summary>
    /// Recursively finding variables from outter scope if the current scope doesn't have the variable.
    /// Search until no more outter scopes are available.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public IObject Get(string name, out bool hasVar) {
        if (!store.ContainsKey(name)) {
            Environment _outer = outer;
            while (_outer != null) {
                if (_outer.store.ContainsKey(name)) {
                    hasVar = true;
                    return _outer.store[name];
                }
                _outer = _outer.outer;
            }
        } else {
            hasVar = true;
            return store[name];
        }

        hasVar = false;
        return null;
    }

    public IObject Set(string name, IObject val) {
        store[name] = val;
        return val;
    }
}