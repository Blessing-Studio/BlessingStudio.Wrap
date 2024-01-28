using BlessingStudio.Wrap.Protocol.Attributes;
using System.Reflection;

namespace BlessingStudio.Wrap.Utils;

public static class ReflectionUtils
{
    public static List<Tuple<FieldAttribute, FieldInfo>> FindFields(Type type)
    {
        FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
        List<Tuple<FieldAttribute, FieldInfo>> toReturn = new();
        foreach (FieldInfo field in fieldInfos)
        {
            FieldAttribute? fieldAttribute = field.GetCustomAttribute<FieldAttribute>();
            if (fieldAttribute != null)
            {
                toReturn.Add(Tuple.Create(fieldAttribute, field));
            }
        }
        toReturn.Sort((left, right) =>
        {
            return left.Item1.index < right.Item1.index ? -1 : 1;
        });
        return toReturn;
    }
    public static List<Tuple<FieldAttribute, FieldInfo>> FindFields<T>()
    {
        return FindFields(typeof(T));
    }
}
