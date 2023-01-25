namespace AquariusLang.utils; 

public class Utils {
    public static T[] PushToArray<T>(T[] target, T item) {
        if (target == null)
        {
            //TODO: Return null or throw ArgumentNullException;
        }
        T[] result = new T[target.Length + 1];
        target.CopyTo(result, 0);
        result[target.Length] = item;
        return result;
    }

    public static float? StringToFloat(string literal) {
        bool success = float.TryParse(literal, out float val);
        if (success) {
            return val;
        }

        if (literal[literal.Length - 1] == 'f') {
            string subStr = literal.Remove(literal.Length - 1, 1);
            success = float.TryParse(subStr, out val);
            if (success) {
                return val;
            }
        }
        return null;
    }

    public static double? StringToDouble(string literal) {
        bool success = double.TryParse(literal, out double val);
        if (success) {
            return val;
        }

        if (literal[literal.Length - 1] == 'f') {
            string subStr = literal.Remove(literal.Length - 1, 1);
            success = double.TryParse(subStr, out val);
            if (success) {
                return val;
            }
        }
        return null;
    }
    
    public static bool TryCast<T>(object obj, out T result)
    {
        if (obj is T)
        {
            result = (T)obj;
            return true;
        }

        result = default(T);
        return false;
    }
}