namespace BlessingStudio.Wrap.Extensions;

public static class RandomExtensions {
    private static readonly Random _random = Random.Shared;

    public static T Choose<T>(this IList<T> list) {
        int index = _random.Next(0, list.Count);
        return list[index];
    }
}