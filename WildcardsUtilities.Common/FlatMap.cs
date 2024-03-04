namespace WildcardsUtilities.Common;

public delegate IEnumerable<TResult> FlatMap<TItem, TResult>
(
    IEnumerable<TItem> items,
    Func<TItem, IEnumerable<TResult>> selector
);

public static class FlatMap
{
    public static IEnumerable<TResult> ForEach<TItem, TResult>
    (
        IEnumerable<TItem> items,
        Func<TItem, IEnumerable<TResult>> selector
    )
    {
        foreach (var item in items)
            selector(item);

        return [];
    }
}
