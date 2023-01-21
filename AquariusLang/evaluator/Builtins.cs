using AquariusLang.Object;

namespace AquariusLang.evaluator;

public class Builtins {
    public static readonly Dictionary<string, BuiltinObj> builtins = new() {
        {
            "len", new BuiltinObj(args => {
                if (args.Length != 1)
                    return Evaluator.NewError($"Wrong number of arguments. Got={args.Length}, want=1");

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

                return Evaluator.NewError($"Argument to `len` not supported, got {arg0.Type()}");
            })
        }, {
            "last", new BuiltinObj(args => {
                if (args.Length != 1)
                    return Evaluator.NewError($"Wrong number of arguments. Got{args.Length}, want 1.");

                if (args[0].Type() != ObjectType.ARRAY_OBJ)
                    return Evaluator.NewError($"Argument to `last` must be ARRAY, got {args[0].Type()}");

                var array = (ArrayObj)args[0];
                var length = array.Elements.Length;

                return length > 0 ? array.Elements[length - 1] : RepeatedPrimitives.NULL;
            })
        }, {
           "rest", new BuiltinObj(args => {
               if (args.Length != 1)
                   return Evaluator.NewError($"Wrong number of arguments. Got{args.Length}, want 1.");
               if (args[0].Type() != ObjectType.ARRAY_OBJ)
                   return Evaluator.NewError($"Argument to `rest` must be ARRAY, got {args[0].Type()}");
               ArrayObj arrayObj = (ArrayObj)args[0];
               int length = arrayObj.Elements.Length;
               if (length > 0) {
                   IObject[] newElements = arrayObj.Elements.Skip(1).ToArray();
                   return new ArrayObj(newElements);
               }

               return RepeatedPrimitives.NULL;
           }) 
        }, {
           "push", new BuiltinObj(args => {
               if (args.Length != 2)
                   return Evaluator.NewError($"Wrong number of arguments. Got{args.Length}, want 2.");

               if (args[0].Type() != ObjectType.ARRAY_OBJ)
                   return Evaluator.NewError($"Argument to `push` must be ARRAY, got {args[0].Type()}");
               
               ArrayObj arrayObj = (ArrayObj)args[0];
               IObject[] newElements = pushToArray(arrayObj.Elements, args[1]);

               return new ArrayObj(newElements);
           }) 
        }
    };
    
    private static T[] pushToArray<T>(T[] target, T item)
    {
        if (target == null)
        {
            //TODO: Return null or throw ArgumentNullException;
        }
        T[] result = new T[target.Length + 1];
        target.CopyTo(result, 0);
        result[target.Length] = item;
        return result;
    }
}