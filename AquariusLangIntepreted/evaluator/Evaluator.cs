using AquariusLang.ast;
using AquariusLang.Object;
using AquariusLang.utils;
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
    private static Dictionary<Type, NodeTypeMapValue> nodeTypeMap = new() {
        {typeof(AbstractSyntaxTree), NodeTypeMapValue.ASTMapValue},
        {typeof(BlockStatement), NodeTypeMapValue.BlockMapValue},
        {typeof(ExpressionStatement), NodeTypeMapValue.ExpressMapValue},
        {typeof(ReturnStatement), NodeTypeMapValue.ReturnMapValue},
        {typeof(LetStatement), NodeTypeMapValue.LetMapValue},
        {typeof(IntegerLiteral), NodeTypeMapValue.IntMapValue},
        {typeof(BooleanLiteral), NodeTypeMapValue.BoolMapValue},
        {typeof(ForLoopLiteral), NodeTypeMapValue.ForMapValue},
        {typeof(PrefixExpression), NodeTypeMapValue.PrefixMapValue},
        {typeof(InfixExpression), NodeTypeMapValue.InfixMapValue},
        {typeof(IfExpression), NodeTypeMapValue.IfMapValue},
        {typeof(Identifier), NodeTypeMapValue.IdentMapValue},
        {typeof(FunctionLiteral), NodeTypeMapValue.FuncMapValue},
        {typeof(CallExpression), NodeTypeMapValue.CallMapValue},
        {typeof(StringLiteral), NodeTypeMapValue.StringMapValue},
        {typeof(ArrayLiteral), NodeTypeMapValue.ArrayMapValue},
        {typeof(IndexExpression), NodeTypeMapValue.IndexMapValue},
        {typeof(HashLiteral), NodeTypeMapValue.HashMapValue},
    };

    public static IObject Eval(INode node, Environment environment) {
        Type nodeType = node.GetType();
        switch (nodeTypeMap[nodeType]) {
            case NodeTypeMapValue.ASTMapValue:
                return evalTree((AbstractSyntaxTree)node, environment);
            
            case NodeTypeMapValue.BlockMapValue:
                return evalBlockStatement((BlockStatement)node, environment);
            
            case NodeTypeMapValue.ExpressMapValue:
                return Eval(((ExpressionStatement)node).Expression, environment);
            
            case NodeTypeMapValue.ReturnMapValue:
                IObject rtnObj = Eval(((ReturnStatement)node).ReturnValue, environment);
                if (isError(rtnObj)) {
                    return rtnObj;
                }
                return new ReturnValueObj(rtnObj);
            
            case NodeTypeMapValue.LetMapValue:
                LetStatement letStatement = (LetStatement)node;
                IObject letObj = Eval(letStatement.Value, environment);
                if (isError(letObj)) {
                    return letObj;
                }
                environment.Set(letStatement.Name.Value, letObj); // Saving value to variable.
                break;
            
            case NodeTypeMapValue.IntMapValue:
                return new IntegerObj(((IntegerLiteral)node).Value);
            
            case NodeTypeMapValue.BoolMapValue:
                return nativeBoolToBoolObj(((BooleanLiteral)node).Value);
            
            case NodeTypeMapValue.PrefixMapValue:
                IObject right = Eval(((PrefixExpression)node).Right, environment);
                if (isError(right)) {
                    return right;
                }
                return evalPrefixExpression(((PrefixExpression)node).Operator, right);
            
            case NodeTypeMapValue.InfixMapValue:
                InfixExpression _node = (InfixExpression)node;
                
                IObject _left = Eval(_node.Left, environment);
                if (isError(_left)) {
                    return _left;
                }


                IObject _right = Eval(_node.Right, environment);
                if (isError(_right)) {
                    return _right;
                }
                
                switch (_node.Operator) {
                    case "=":
                        if (_node.Left is Identifier leftIdent) {
                            environment.Set(leftIdent.Value, _right);
                        } else {
                            return NewError($"Left operand for = operator not identifier. Got={_node.Left.GetType()}");
                        }
                        break;
                    case "+=":
                        if (_node.Left is Identifier _leftIdent) {
                            if (_right.Type() == ObjectType.INTEGER_OBJ) {
                                IObject identVal = evalIdentifier(_leftIdent, environment);
                                if (identVal.Type() == ObjectType.INTEGER_OBJ) {
                                    environment.Set(_leftIdent.Value, new IntegerObj(((IntegerObj)identVal).Value + ((IntegerObj)_right).Value));
                                } else {
                                    return NewError($"Incorrect left operand type for INT +=: {identVal.Type()}");
                                }
                            } else if (_right.Type() == ObjectType.STRING_OBJ) {
                                // TODO string concatenation.
                                return NewError("String as left operand type for += string concatenation not implemented yet.");
                            }
                        } else {
                            return NewError($"Left operand for += operator not identifier. Got={_node.Left.GetType()}");
                        }
                        break;
                    default:
                        return evalInfixExpression(_node.Operator, _left, _right);
                }
                
                break;

            case NodeTypeMapValue.IfMapValue:
                return evalIfExpression((IfExpression)node, environment);
            
            case NodeTypeMapValue.IdentMapValue:
                return evalIdentifier((Identifier)node, environment);
            
            case NodeTypeMapValue.FuncMapValue:
                FunctionLiteral functionLiteralNode = (FunctionLiteral)node;
                Identifier[] parameters = functionLiteralNode.Parameters;
                BlockStatement body = functionLiteralNode.Body;
                return new FunctionObj(parameters, body, environment);
            
            case NodeTypeMapValue.CallMapValue:
                CallExpression callExpressionNode = (CallExpression)node;
                IObject function = Eval(callExpressionNode.Function, environment); // function should be of type FunctionObj.
                if (isError(function)) {
                    return function;
                }

                IObject[] args = evalExpressions(callExpressionNode.Arguments, environment);
                if (args.Length == 1 && isError(args[0])) {
                    return args[0];
                }

                return applyFunction(function, args);
            
            case NodeTypeMapValue.StringMapValue:
                return new StringObj(((StringLiteral)node).Value);
            
            case NodeTypeMapValue.ArrayMapValue:
                IObject[] elements = evalExpressions(((ArrayLiteral)node).Elements, environment);
                if (elements.Length == 1 && isError(elements[0])) {
                    return elements[0];
                }

                return new ArrayObj(elements);
            
            case NodeTypeMapValue.IndexMapValue:
                IndexExpression indexNode = (IndexExpression)node;
                IObject left = Eval(indexNode.Left, environment);
                if (isError(left)) {
                    return left;
                }

                IObject index = Eval(indexNode.Index, environment);
                if (isError(index)) {
                    return index;
                }

                return evalIndexExpression(left, index);
            
            case NodeTypeMapValue.HashMapValue:
                return evalHashLiteral((HashLiteral)node, environment);
            
            case NodeTypeMapValue.ForMapValue:
                return evalForLoopLiteral((ForLoopLiteral)node, environment);
        }

        return null;
    }

    private static IObject evalTree(AbstractSyntaxTree tree, Environment environment) {
        IObject result = null;
        
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

    private static IObject evalBlockStatement(BlockStatement blockStatement, Environment environment) {
        IObject result = null;
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

    private static IObject evalPrefixExpression(string _operator, IObject right) {
        switch (_operator) {
            case "!":
                return evalBangOperatorExpression(right);
            case "-":
                return evalMinusPrefixOperatorExpression(right);
            default:
                return NewError($"Unknown operator: {_operator}{right.Type()}");
        }
    }

    private static IObject evalInfixExpression(string _operator, IObject left, IObject right) {
        if (left.Type() == ObjectType.INTEGER_OBJ && right.Type() == ObjectType.INTEGER_OBJ) {
            return evalIntegerInfixExpression(_operator, left, right);
        }
        
        switch (_operator) {
            case "==":
                return nativeBoolToBoolObj(left == right);
            case "!=":
                return nativeBoolToBoolObj(left != right);
        }

        if (left.Type() == ObjectType.STRING_OBJ && right.Type() == ObjectType.STRING_OBJ) {
            return evalStringInfixExpression(_operator, left, right);
        }

        if (left.Type() != right.Type()) {
            return NewError($"Type mismatch: {left.Type()} {_operator} {right.Type()}");
        }

        return NewError($"Unknown operator: {left.Type()} {_operator} {right.Type()}");
    }

    private static IObject evalStringInfixExpression(string _operator, IObject left, IObject right) {
        if (_operator != "+") {
            return NewError($"Unknown operator: {left.Type()} {_operator} {right.Type()}");
        }
        string leftVal = ((StringObj)left).Value;
        string rightVal = ((StringObj)right).Value;
        return new StringObj(leftVal + rightVal);
    }

    private static IObject evalIfExpression(IfExpression ifExpression, Environment environment) {
        IObject condition = Eval(ifExpression.Condition, environment);
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

    private static IObject[] evalExpressions(IExpression[] expressions, Environment environment) {
        List<IObject> result = new List<IObject>();
        foreach (var expression in expressions) {
            IObject evaluated = Eval(expression, environment);
            if (isError(evaluated)) {
                return new[] { evaluated };
            }
            result.Add(evaluated);
        }

        return result.ToArray();
    }

    private static IObject applyFunction(IObject fn, IObject[] args) {
        if (fn is FunctionObj functionObj) {
            Environment extendedEnv = extendFunctionEnv(functionObj, args);
            IObject evaluated = Eval(functionObj.Body, extendedEnv);
            return unwrapReturnValue(evaluated);
        } else if (fn is BuiltinObj builtinObj) {
            return builtinObj.Fn(args);
        }

        return NewError($"Not a function: {fn.Type()}");
    }
    
    // private static void reassignVariable()

    private static IObject evalIdentifier(Identifier node, Environment environment) {
        // Looking up built-in functions.
        bool hasVal = Builtins.builtins.TryGetValue(node.Value, out BuiltinObj value);
        if (hasVal) {
            return value;
        }
        
        IObject val = environment.Get(node.Value, out bool hasVar);
        if (!hasVar) {
            return NewError($"Identifier not found: {node.Value}");
        }

        return val;
    }

    private static IObject evalMinusPrefixOperatorExpression(IObject right) {
        if (right.Type() != ObjectType.INTEGER_OBJ) {
            return NewError($"Unknown operator: -{right.Type()}");
        }

        int value = ((IntegerObj)right).Value;
        return new IntegerObj(-value);
    }

    private static IObject evalIntegerInfixExpression(string _operator, IObject left, IObject right) {
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
                return NewError($"Unknown operator: {left.Type()} {_operator} {right.Type()}");
        }
    }

    private static IObject evalBangOperatorExpression(IObject right) {
        if (right == RepeatedPrimitives.TRUE) {
            return RepeatedPrimitives.FALSE;
        } else if (right == RepeatedPrimitives.FALSE) {
            return RepeatedPrimitives.TRUE;
        } else if (right == RepeatedPrimitives.NULL) {
            return RepeatedPrimitives.TRUE;
        }

        return RepeatedPrimitives.FALSE;
    }

    private static IObject evalIndexExpression(IObject left, IObject index) {
        if (left.Type() == ObjectType.ARRAY_OBJ && index.Type() == ObjectType.INTEGER_OBJ) {
            return evalArrayIndexExpression(left, index);
        } else if (left.Type() == ObjectType.HASH_OBJ) {
            return evalHashIndexExpression(left, index);
        }

        return NewError($"Index operator not supported: {left.Type()}");
    }

    private static IObject evalArrayIndexExpression(IObject array, IObject index) {
        ArrayObj arrayObj = (ArrayObj)array;
        int idx = ((IntegerObj)index).Value;
        int max = arrayObj.Elements.Length - 1;
        if (idx < 0 || idx > max) {
            return RepeatedPrimitives.NULL;
        }

        return arrayObj.Elements[idx];
    }

    private static IObject evalHashIndexExpression(IObject hash, IObject index) {
        HashObj hashObj = (HashObj)hash; 
        bool ok = Utils.TryCast(index, out IHashable key);
        if (!ok) {
            return NewError($"Unusable as hash key: {index.Type()}");
        }

        ok = hashObj.Pairs.TryGetValue(key.HashKey(), out HashPair pair);

        if (!ok) {
            return RepeatedPrimitives.NULL;
        }

        return pair.Value;
    }

    private static IObject evalHashLiteral(HashLiteral node, Environment environment) {
        Dictionary<HashKey, HashPair> pairs = new Dictionary<HashKey, HashPair>();
        foreach (var pair in node.Pairs) {
            IObject key = Eval(pair.Key, environment);
            if (isError(key)) {
                return key;
            }

            bool usable = Utils.TryCast(key, out IHashable hashKey);
            if (!usable) {
                return NewError($"Unusable as hash key: {key.Type()}");
            }

            IObject value = Eval(pair.Value, environment);
            if (isError(value)) {
                return value;
            }

            HashKey hashed = hashKey.HashKey();
            pairs[hashed] = new HashPair(key, value);
        }

        return new HashObj(pairs);
    }

    private static IObject evalForLoopLiteral(ForLoopLiteral node, Environment environment) {
        Environment enclosedEnvironment = Environment.NewEnclosedEnvironment(environment);
        LetStatement letStatement = node.DeclareStatement;
        IExpression conditionalExpression = node.ConditionalExpression;
        IStatement valueChangeStatement = node.ValueChangeStatement;
        BlockStatement blockStatement = node.Body;

        Eval(letStatement, enclosedEnvironment);

        while (true) {
            IObject evalConditionResult = Eval(conditionalExpression, enclosedEnvironment);
            if (evalConditionResult.Type() == ObjectType.BOOLEAN_OBJ) {
                if (!((BooleanObj)evalConditionResult).Value) {
                    break;
                }

                Eval(blockStatement, enclosedEnvironment);
                Eval(valueChangeStatement, enclosedEnvironment);
            } else {
                NewError($"Expected bool from for loop conditionals. Got={evalConditionResult.Type()}");
            }
        }
        
        // foreach (LetStatement letStatement in letStatements) {
        //     Console.WriteLine(letStatement.String());
        //     Eval(letStatement, enclosedEnvironment);
        // }
        // foreach (IExpression conditionalExpression in conditionalExpressions) {
        //     Console.WriteLine(conditionalExpression.GetType());
        // }
        // while (true) {
        //     IObject[] evalConditionResults = evalExpressions(conditionalExpressions, enclosedEnvironment);
        //     foreach (IObject evalConditionResult in evalConditionResults) {
        //         if (evalConditionResult is BooleanLiteral evalConditionResultBool) {
        //             if (!evalConditionResultBool.Value) {
        //                 break;
        //             }
        //         } else {
        //             return NewError($"For loop condition not bool result: {evalConditionResult.Inspect()}");
        //         }
        //     }
        //
        //     Eval(blockStatement, enclosedEnvironment);
        //     
        //     foreach (IStatement valueChangeStatement in valueChangeStatements) {
        //         Eval(valueChangeStatement, enclosedEnvironment);
        //     }
        // }
        return null;
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
    private static Environment extendFunctionEnv(FunctionObj fn, IObject[] args) {
        Environment environment = Environment.NewEnclosedEnvironment(fn.Env);
        
        for (var i = 0; i < fn.Parameters.Length; i++) {
            environment.Set(fn.Parameters[i].Value, args[i]);
        }

        return environment;
    }

    private static IObject unwrapReturnValue(IObject obj) {
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
    public static ErrorObj NewError(string msg) {
        return new ErrorObj(msg);
    }
    
    /// <summary>
    ///     We need to check for errors whenever we call Eval inside of Eval, in order to stop
    /// errors from being passed around and then bubbling up far away from their origin.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static bool isError(IObject obj) {
        if (obj != null) {
            return obj.Type() == ObjectType.ERROR_OBJ;
        }

        return false;
    }

    private static bool isTruthy(IObject obj) {
        if (obj == RepeatedPrimitives.NULL) {
            return false;
        } else if (obj == RepeatedPrimitives.TRUE) {
            return true;
        } else if (obj == RepeatedPrimitives.FALSE) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// For use to compare types using switch through the help of dictionary and constant enum values.
    /// </summary>
    private enum NodeTypeMapValue {
        ASTMapValue,
        BlockMapValue,
        ExpressMapValue,
        ReturnMapValue,
        LetMapValue,
        IntMapValue,
        BoolMapValue,
        PrefixMapValue,
        InfixMapValue,
        IfMapValue,
        IdentMapValue,
        FuncMapValue,
        CallMapValue,
        StringMapValue,
        ArrayMapValue,
        IndexMapValue,
        HashMapValue,
        ForMapValue,
    }
}