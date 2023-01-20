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
}

public interface Object {
    string Type(); // Corresponds to ObjectType members.
    string Inspect();
}

public class IntegerObj : Object {
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

public class BooleanObj : Object {
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
public class NullObj : Object {
    public string Type() {
        return ObjectType.NULL_OBJ;
    }

    public string Inspect() {
        return "null";
    }
}

public class ReturnValueObj : Object {
    private Object value;

    public ReturnValueObj(Object value) {
        this.value = value;
    }

    public string Type() {
        return ObjectType.RETURN_VALUE_OBJ;
    }

    public string Inspect() {
        return value.Inspect();
    }

    public Object Value {
        get => value;
        set => this.value = value;
    }
}

/// <summary>
/// This is used for internal error handling.
/// </summary>
public class ErrorObj : Object {
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
public class FunctionObj : Object {
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

public class StringObj : Object {
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
