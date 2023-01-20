using AquariusLang.ast;
using AquariusLang.Object;
using Environment = AquariusLang.Object.Environment;

namespace AquariusLang.evaluator;

/// <summary>
/// These are for not creating a new object each time returning primitives with repeated values.
/// For example, a boolean with true or false value might appear lots of times. It's better if
/// just create the object once and re-use it in every encounter.
/// </summary>
public struct RepeatedPrimitives {
    public static readonly NullObj NULL = new ();
    public static readonly BooleanObj TRUE = new (true);
    public static readonly BooleanObj FALSE = new(false);
}

public class Evaluator {
    private static Dictionary<Type, int> nodeTypeMap = new() {
        {typeof(AbstractSyntaxTree), ASTMapValue},
        {typeof(BlockStatement), BlockMapValue},
        {typeof(ExpressionStatement), ExpressMapValue},
        {typeof(ReturnStatement), ReturnMapValue},
        {typeof(LetStatement), LetMapValue},
        {typeof(IntegerLiteral), IntMapValue},
        {typeof(BooleanLiteral), BoolMapValue},
        {typeof(PrefixExpression), PrefixMapValue},
        {typeof(InfixExpression), InfixMapValue},
        {typeof(IfExpression), IfMapValue},
        {typeof(Identifier), IdentMapValue},
        {typeof(FunctionLiteral), FuncMapValue},
        {typeof(CallExpression), CallMapValue},
    };

    public static Object.Object Eval(INode node, Environment environment) {
        Type nodeType = node.GetType();
        switch (nodeTypeMap[nodeType]) {
            case ASTMapValue:
                return evalTree((AbstractSyntaxTree)node, environment);
            
            case BlockMapValue:
                return evalBlockStatement((BlockStatement)node, environment);
            
            case ExpressMapValue:
                return Eval(((ExpressionStatement)node).Expression, environment);
            
            case ReturnMapValue:
                Object.Object rtnObj = Eval(((ReturnStatement)node).ReturnValue, environment);
                if (isError(rtnObj)) {
                    return rtnObj;
                }
                return new ReturnValueObj(rtnObj);
            
            case LetMapValue:
                LetStatement letStatement = (LetStatement)node;
                Object.Object letObj = Eval(letStatement.Value, environment);
                if (isError(letObj)) {
                    return letObj;
                }
                environment.Set(letStatement.Name.Value, letObj); // Saving value to variable.
                break;
            
            case IntMapValue:
                return new IntegerObj(((IntegerLiteral)node).Value);
            
            case BoolMapValue:
                return nativeBoolToBoolObj(((BooleanLiteral)node).Value);
            
            case PrefixMapValue:
                Object.Object right = Eval(((PrefixExpression)node).Right, environment);
                if (isError(right)) {
                    return right;
                }
                return evalPrefixExpression(((PrefixExpression)node).Operator, right);
            
            case InfixMapValue:
                InfixExpression _node = (InfixExpression)node;
                Object.Object _left = Eval(_node.Left, environment);
                if (isError(_left)) {
                    return _left;
                }

                Object.Object _right = Eval(_node.Right, environment);
                if (isError(_right)) {
                    return _right;
                }

                return evalInfixExpression(_node.Operator, _left, _right);
            
            case IfMapValue:
                return evalIfExpression((IfExpression)node, environment);
            
            case IdentMapValue:
                return evalIdentifier((Identifier)node, environment);
            
            case FuncMapValue:
                FunctionLiteral functionLiteralNode = (FunctionLiteral)node;
                Identifier[] parameters = functionLiteralNode.Parameters;
                BlockStatement body = functionLiteralNode.Body;
                return new FunctionObj(parameters, body, environment);
            
            case CallMapValue:
                CallExpression callExpressionNode = (CallExpression)node;
                Object.Object function = Eval(callExpressionNode.Function, environment); // function should be of type FunctionObj.
                if (isError(function)) {
                    return function;
                }

                Object.Object[] args = evalExpressions(callExpressionNode.Arguments, environment);
                if (args.Length == 1 && isError(args[0])) {
                    return args[0];
                }

                return applyFunction(function, args);
        }

        return null;
    }

    private static Object.Object evalTree(AbstractSyntaxTree tree, Environment environment) {
        Object.Object result = null;
        
        /*
			Sometimes we have to keep track of object.ReturnValues for longer
		and can’t unwrap their values on the first encounter. That’s the case
		with block statements:

			if (10 > 1) {
				if (10 > 1) {
					return 10;
				}
				return 1;
			}

			This is why this for loop exists. To fix the issue.
			Here we explicitly don’t unwrap the return value and only check the Type() of each evaluation
		result. If it’s object.RETURN_VALUE_OBJ we simply return the *object.ReturnValue, without
		unwrapping its .Value, so it stops execution in a possible outer block statement and bubbles
		up to evalProgram, where it finally gets unwrapped.
	    */
        foreach (var statement in tree.Statements) {
            result = Eval(statement, environment);
            if (result != null) {
                Type resultType = result.GetType();
            
                if (resultType == typeof(ReturnValueObj)) {
                    return ((ReturnValueObj)result).Value;
                } else if (resultType == typeof(ErrorObj)) { // Part of error handling.
                    return result;
                }
            }
        }

        return result;
    }

    private static Object.Object evalBlockStatement(BlockStatement blockStatement, Environment environment) {
        Object.Object result = null;
        foreach (var statement in blockStatement.Statements) {
            result = Eval(statement, environment);
            /*
			    Both return and error encounter stops the current statements' execution.
		    */
            if (result != null) {
                string resultType = result.Type();
                if (resultType == ObjectType.RETURN_VALUE_OBJ || resultType == ObjectType.ERROR_OBJ) {
                    return result;
                }
            }
        }

        return result;
    }

    private static Object.Object evalPrefixExpression(string _operator, Object.Object right) {
        switch (_operator) {
            case "!":
                return evalBangOperatorExpression(right);
            case "-":
                return evalMinusPrefixOperatorExpression(right);
            default:
                return newError($"unknown operator: {_operator}{right.Type()}");
        }
    }

    private static Object.Object evalInfixExpression(string _operator, Object.Object left, Object.Object right) {
        if (left.Type() == ObjectType.INTEGER_OBJ && right.Type() == ObjectType.INTEGER_OBJ) {
            return evalIntegerInfixExpression(_operator, left, right);
        }

        switch (_operator) {
            case "==":
                return nativeBoolToBoolObj(left == right);
            case "!=":
                return nativeBoolToBoolObj(left != right);
        }

        if (left.Type() != right.Type()) {
            return newError($"Type mismatch: {left.Type()} {_operator} {right.Type()}");
        }

        return newError($"Unknown operator: {left.Type()} {_operator} {right.Type()}");
    }

    private static Object.Object evalIfExpression(IfExpression ifExpression, Environment environment) {
        Object.Object condition = Eval(ifExpression.Condition, environment);
        if (isError(condition)) {
            return condition;
        }

        if (isTruthy(condition)) {
            return Eval(ifExpression.Consequence, environment);
        } else if (ifExpression.Alternative != null) {
            return Eval(ifExpression.Alternative, environment);
        } else {
            return RepeatedPrimitives.NULL;
        }
    }

    private static Object.Object[] evalExpressions(IExpression[] expressions, Environment environment) {
        List<Object.Object> result = new List<Object.Object>();
        foreach (var expression in expressions) {
            Object.Object evaluated = Eval(expression, environment);
            if (isError(evaluated)) {
                return new[] { evaluated };
            }
            result.Add(evaluated);
        }

        return result.ToArray();
    }

    private static Object.Object applyFunction(Object.Object fn, Object.Object[] args) {
        if (fn is FunctionObj functionObj) {
            Environment extendedEnv = extendFunctionEnv(functionObj, args);
            Object.Object evaluated = Eval(functionObj.Body, extendedEnv);
            return unwrapReturnValue(evaluated);
        }

        return newError($"Not a function: {fn.Type()}");
    }

    private static Object.Object evalIdentifier(Identifier node, Environment environment) {
        Object.Object val = environment.Get(node.Value, out bool hasVar);
        if (!hasVar) {
            return newError($"Identifier not found: {node.Value}");
        }

        return val;
    }

    private static Object.Object evalMinusPrefixOperatorExpression(Object.Object right) {
        if (right.Type() != ObjectType.INTEGER_OBJ) {
            return newError($"Unknown operator: {right.Type()}");
        }

        int value = ((IntegerObj)right).Value;
        return new IntegerObj(-value);
    }

    private static Object.Object evalIntegerInfixExpression(string _operator, Object.Object left, Object.Object right) {
        int leftVal = ((IntegerObj)left).Value;
        int rightVal = ((IntegerObj)right).Value;
        switch (_operator) {
            case "+":
                return new IntegerObj(leftVal + rightVal);
            case "-":
                return new IntegerObj(leftVal - rightVal);
            case "*":
                return new IntegerObj(leftVal * rightVal);
            case "/":
                return new IntegerObj(leftVal / rightVal);
            case "<":
                return nativeBoolToBoolObj(leftVal < rightVal);
            case ">":
                return nativeBoolToBoolObj(leftVal > rightVal);
            case "==":
                return nativeBoolToBoolObj(leftVal == rightVal);
            case "!=":
                return nativeBoolToBoolObj(leftVal != rightVal);
            default:
                return newError($"Unknown operator: {left.Type()} {_operator} {right.Type()}");
        }
    }

    private static Object.Object evalBangOperatorExpression(Object.Object right) {
        if (right == RepeatedPrimitives.TRUE) {
            return RepeatedPrimitives.FALSE;
        } else if (right == RepeatedPrimitives.FALSE) {
            return RepeatedPrimitives.TRUE;
        } else if (right == RepeatedPrimitives.NULL) {
            return RepeatedPrimitives.TRUE;
        }

        return RepeatedPrimitives.FALSE;
    }

    /// <summary>
    ///     Create a new environment (local scope) for inside the function. Values of variables
    /// from paramters get passed into this newly created environment.
    ///
    ///     Extending the function’s environment and not the current environment also makes it
    /// possible to return closures:
    ///
    /// >> let newAdder = fn(x) { fn(y) { x + y } };
    /// >> let addThree = newAdder(3);
    /// >> addThree(10);
    /// 13
    ///
    ///     newAdder here is a higher-order function. Higher-order functions are functions that either return
    /// other functions or receive them as arguments. In this case newAdder returns another function.
    /// But not just any function: a closure. addThree is bound to the closure that’s returned when calling
    /// newAdder with 3 as the sole argument.
    ///
    ///     When addThree is called it not only has access to the arguments of the call, the y parameter, but
    /// it can also reach the value x was bound to at the time of the newAdder(2) call, the closure addThree
    /// still has access to the environment that was the current environment at the time of its definition.
    /// Which is when the last line of newAdder’s body was evaluated.
    /// </summary>
    /// <param name="fn"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    private static Environment extendFunctionEnv(FunctionObj fn, Object.Object[] args) {
        Environment environment = Environment.NewEnclosedEnvironment(fn.Env);
        
        for (var i = 0; i < fn.Parameters.Length; i++) {
            environment.Set(fn.Parameters[i].Value, args[i]);
        }

        return environment;
    }

    private static Object.Object unwrapReturnValue(Object.Object obj) {
        /*
		 This is to stop executing any further statements after the return expression.
		 Just return back the unwrapped value.
	    */
        if (obj is ReturnValueObj returnValueObj) {
            return returnValueObj.Value;
        }

        return obj;
    }
    
    private static BooleanObj nativeBoolToBoolObj(bool input) {
        return input ? RepeatedPrimitives.TRUE : RepeatedPrimitives.FALSE;
    }

    /// <summary>
    /// Create new *object.Errors and return them when encountering error in script.
    /// </summary>
    /// <param name="msg"></param>
    /// <returns></returns>
    private static ErrorObj newError(string msg) {
        return new ErrorObj(msg);
    }
    
    /// <summary>
    ///     We need to check for errors whenever we call Eval inside of Eval, in order to stop
    /// errors from being passed around and then bubbling up far away from their origin.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static bool isError(Object.Object obj) {
        if (obj != null) {
            return obj.Type() == ObjectType.ERROR_OBJ;
        }

        return false;
    }

    private static bool isTruthy(Object.Object obj) {
        if (obj == RepeatedPrimitives.NULL) {
            return false;
        } else if (obj == RepeatedPrimitives.TRUE) {
            return true;
        } else if (obj == RepeatedPrimitives.FALSE) {
            return false;
        }

        return true;
    }

    private const int ASTMapValue = 0;
    private const int BlockMapValue = 1;
    private const int ExpressMapValue = 2;
    private const int ReturnMapValue = 3;
    private const int LetMapValue = 4;
    private const int IntMapValue = 5;
    private const int BoolMapValue = 6;
    private const int PrefixMapValue = 7;
    private const int InfixMapValue = 8;
    private const int IfMapValue = 9;
    private const int IdentMapValue = 10;
    private const int FuncMapValue = 11;
    private const int CallMapValue = 12;
}