namespace AquariusLang.Object; 

public class Environment {
    private Dictionary<string, Object> store;
    private Environment outer;

    public Environment NewEnclosedEnvironment(Environment outer) {
        Environment environment = new Environment();
        environment.outer = outer;
        return environment;
    }

    public Environment NewEnvironment() {
        Environment environment = new Environment() { store = new Dictionary<string, Object>(), outer = null };
        return environment;
    }

    /// <summary>
    /// Recursively finding variables from outter scope if the current scope doesn't have the variable.
    /// Search until no more outter scopes are available.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Object Get(string name, out bool hasVar) {
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

    public Object Set(string name, Object val) {
        store[name] = val;
        return val;
    }
}