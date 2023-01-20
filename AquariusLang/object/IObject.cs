using System.Text;
using AquariusLang.ast;

namespace AquariusLang.Object;

public struct ObjectType {
    public const string NULL_OBJ  = "NULL";
    public const string ERROR_OBJ = "ERROR";
    public const string INTEGER_OBJ = "INTEGER";
    public const string BOOLEAN_OBJ = "BOOLEAN";
    public const string RETURN_VALUE_OBJ = "RETURN_VALUE";
    public const string FUNCTION_OBJ = "FUNCTION";
    public const string STRING_OBJ = "STRING";
    public const string BUILTIN_OBJ = "BUILTIN";
    public const string ARRAY_OBJ = "ARRAY";
}

public interface IObject {
    string Type(); // Corresponds to ObjectType members.
    string Inspect();
}

public class IntegerObj : IObject {
    private int value;

    public IntegerObj(int value) {
        this.value = value;
    }

    public string Type() {
        return ObjectType.INTEGER_OBJ;
    }

    public string Inspect() {
        return value.ToString();
    }

    public int Value {
        get => value;
        set => this.value = value;
    }
}

public class BooleanObj : IObject {
    private bool value;

    public BooleanObj(bool value) {
        this.value = value;
    }

    public string Type() {
        return ObjectType.BOOLEAN_OBJ;
    }

    public string Inspect() {
        return value.ToString();
    }

    public bool Value {
        get => value;
        set => this.value = value;
    }
}

/// <summary>
/// Null doesn’t wrap any value. It represents the absence of any value.
/// </summary>
public class NullObj : IObject {
    public string Type() {
        return ObjectType.NULL_OBJ;
    }

    public string Inspect() {
        return "null";
    }
}

public class ReturnValueObj : IObject {
    private IObject value;

    public ReturnValueObj(IObject value) {
        this.value = value;
    }

    public string Type() {
        return ObjectType.RETURN_VALUE_OBJ;
    }

    public string Inspect() {
        return value.Inspect();
    }

    public IObject Value {
        get => value;
        set => this.value = value;
    }
}

/// <summary>
/// This is used for internal error handling.
/// </summary>
public class ErrorObj : IObject {
    private string message;

    public ErrorObj(string message) {
        this.message = message;
    }

    public string Type() {
        return ObjectType.ERROR_OBJ;
    }

    public string Inspect() {
        return "ERROR: " + message;
    }

    public string Message {
        get => message;
        set => message = value;
    }
}

/// <summary>
///     A function also has Env, a field that holds a pointer to an object.Environment,
/// because functions in Monkey carry their own environment with them. That allows for
/// closures, which “close over” the environment they’re defined in and can later access
/// it.
/// </summary>
public class FunctionObj : IObject {
    private Identifier[] parameters;
    private BlockStatement body;
    private Environment env;

    public FunctionObj(Identifier[] parameters, BlockStatement body, Environment env) {
        this.parameters = parameters;
        this.body = body;
        this.env = env;
    }

    public string Type() {
        return ObjectType.FUNCTION_OBJ;
    }

    public string Inspect() {
        StringBuilder builder = new StringBuilder();
        
        List<string> parameters = new();
        foreach (var parameter in parameters) {
            parameters.Add(parameter);
        }
        
        builder.Append("fn")
            .Append('(')
            .Append(string.Join(", ", parameters))
            .Append(") {\n")
            .Append(body.String())
            .Append("\n}");

        return builder.ToString();
    }

    public Identifier[] Parameters {
        get => parameters;
        set => parameters = value;
    }

    public BlockStatement Body {
        get => body;
        set => body = value;
    }

    public Environment Env {
        get => env;
        set => env = value;
    }
}

public class StringObj : IObject {
    private string value;

    public StringObj(string value) {
        this.value = value;
    }

    public string Type() {
        return ObjectType.STRING_OBJ;
    }

    public string Inspect() {
        return value;
    }

    public string Value {
        get => value;
        set => this.value = value;
    }
}

public delegate IObject BuiltinFunction(IObject[] args);

public class BuiltinObj : IObject {
    private BuiltinFunction fn;

    public BuiltinObj(BuiltinFunction fn) {
        this.fn = fn;
    }

    public string Type() {
        return ObjectType.BUILTIN_OBJ;
    }

    public string Inspect() {
        return "builtin function";
    }

    public BuiltinFunction Fn {
        get => fn;
        set => fn = value ?? throw new ArgumentNullException(nameof(value));
    }
}

public class ArrayObj : IObject {
    public string Type() {
        throw new NotImplementedException();
    }

    public string Inspect() {
        throw new NotImplementedException();
    }
}
