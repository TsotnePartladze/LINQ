using System;
using System.Collections.Generic;

public static class MyEnumerable
{
    public static IEnumerable<TResult> MySelect<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> selector)
    {
        foreach (var item in source)
        {
            yield return selector(item);
        }
    }

    public static IEnumerable<TSource> MyWhere<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                yield return item;
            }
        }
    }

    public static bool MyAny<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                return true;
            }
        }
        return false;
    }

    public static bool MyAll<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        foreach (var item in source)
        {
            if (!predicate(item))
            {
                return false;
            }
        }
        return true;
    }

    public static TSource MyFirstOrDefault<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        foreach (var item in source)
        {
            if (predicate(item))
            {
                return item; //first match
            }
        }
        return default;
    }

    public static IEnumerable<TSource> MyOrderBy<TSource, TKey>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector)
    {
        var list = source.ToList();
        list.Sort((x, y) => Comparer<TKey>.Default.Compare(
            keySelector(x), keySelector(y)));

        foreach (var item in list)
        {
            yield return item;
        }
    }
}

class Program
{
    static void Main()
    {
        List<int> list = new List<int>() { 1, 10, 7, 4, 9, 6, 3, 8, 5, 2 };

        var doubledList = list.MySelect(x => x * 2);
        foreach (var item in doubledList)
        {
            Console.WriteLine(item);
        }

        var moreThan5 = list.MyWhere(x => x > 5);
        foreach (var item in moreThan5)
        {
            Console.WriteLine(item);
        }

        bool hasEven = list.MyAny(x => x % 2 == 0);
        if (hasEven == true)
        {
            Console.WriteLine("Even numbers in the list");
        }

        var allGreaterThan0 = list.MyAll(x => x > 0);
        if (allGreaterThan0 == true)
        {
            Console.WriteLine("Greater than 5");
        }

        var firstGreaterThan5 = list.MyFirstOrDefault(x => x > 5);
        Console.WriteLine(firstGreaterThan5);

        var ordered = list.MyOrderBy(x => x);
        foreach (var item in ordered)
        {
            Console.WriteLine(item);

        }
    }
}