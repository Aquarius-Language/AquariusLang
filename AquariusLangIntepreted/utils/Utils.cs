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