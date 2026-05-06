using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class MyEnumerable
{
    public static IEnumerable<TResult> MySelect<TSource, TResult>(
        this IEnumerable<TSource> source,
        Func<TSource, TResult> selector)
    {
        return new MySelectIterator<TSource, TResult>(source, selector);
    }

    public static IEnumerable<TSource> MyWhere<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        return new MyWhereIterator<TSource>(source, predicate);
    }

    public static bool MyAny<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        using (var enumerator = source.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (predicate(enumerator.Current))
                {
                    return true;
                }
            }
        }
        return false; //no match
    }

    public static bool MyAll<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        using (var enumerator = source.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                if (!predicate(enumerator.Current))
                {
                    return false; //fail
                }
            }
        }
        return true; //all match
    }


    public static TSource MyFirstOrDefault<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, bool> predicate)
    {
        using (var enumerator = source.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                if (predicate(item))
                {
                    return item; //first match
                }
            }
        }
        return default;
    }

    public static IEnumerable<TSource> MyOrderBy<TSource, TKey>(
    this IEnumerable<TSource> source,
    Func<TSource, TKey> keySelector)
    {
        return new MyOrderByIterator<TSource, TKey>(source, keySelector);
    }

    private class MySelectIterator<TSource, TResult>
      : IEnumerable<TResult>, IEnumerator<TResult>, IEnumerator
    {
        private int _bookmark;
        private TResult _current;
        private IEnumerable<TSource> _source;
        private Func<TSource, TResult> _selector;
        private IEnumerator<TSource> _enumerator;

        public MySelectIterator(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {

            _bookmark = 0;
            _source = source;
            _selector = selector;          
        }

        public TResult Current => _current;
        object IEnumerator.Current => _current;

        public bool MoveNext()
        {
            switch (_bookmark)
            {
                case 0: //first call
                    _enumerator = _source.GetEnumerator();
                    _bookmark = 1;
                    goto case 1;

                case 1: //loop
                    if (_enumerator.MoveNext())
                    {
                        _current = _selector(_enumerator.Current);
                        _bookmark = 1;
                        return true;
                    }

                    goto case -1;

                case -1: //stop
                    _bookmark = -1;
                    _enumerator?.Dispose();
                    return false;

                default: return false;


            }
         }

        public void Dispose()
        {
            if (_bookmark != -1)
            {
                _bookmark = -1;
                _enumerator?.Dispose();
            }
        }

        public void Reset() => throw new NotSupportedException();

        //reiteration
        public IEnumerator<TResult> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }

    private class MyWhereIterator<TSource>
    : IEnumerable<TSource>, IEnumerator<TSource>, IEnumerator
    {
        private int _bookmark;
        private TSource _current;
        private IEnumerable<TSource> _source;
        private Func<TSource, bool> _predicate;
        private IEnumerator<TSource> _enumerator;

        public MyWhereIterator(IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            _bookmark = 0;
            _source = source;
            _predicate = predicate;
        }

        public TSource Current => _current;
        object IEnumerator.Current => _current;

        public bool MoveNext()
        {
            switch (_bookmark)
            {
                case 0:
                    _enumerator = _source.GetEnumerator();
                    _bookmark = 1;
                    goto case 1;

                case 1:
                    while (_enumerator.MoveNext())
                    {
                        var item = _enumerator.Current;
                        if (_predicate(item))   // only if predicate pass
                        {
                            _current = item;    //no selector
                            _bookmark = 1;
                            return true;
                        }
                        // predicate fail = loop continues, no return
                    }
                    goto case -1;

                case -1:
                    _enumerator?.Dispose();
                    return false;

                default:
                    return false;
            }
        }

        public void Dispose()
        {
            if (_bookmark != -1)
            {
                _bookmark = -1;
                _enumerator?.Dispose();
            }
        }

        public void Reset() => throw new NotSupportedException();

        public IEnumerator<TSource> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }

    private class MyOrderByIterator<TSource, TKey>
    : IEnumerable<TSource>, IEnumerator<TSource>, IEnumerator
    {
        private int _bookmark;
        private TSource _current;
        private IEnumerable<TSource> _source;
        private Func<TSource, TKey> _keySelector;
        private IEnumerator<TSource> _enumerator;

        public MyOrderByIterator(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            _bookmark = 0;
            _source = source;
            _keySelector = keySelector;
        }

        public TSource Current => _current;
        object IEnumerator.Current => _current;

        public bool MoveNext()
        {
            switch (_bookmark)
            {
                case 0: //first call, sort
                    var list = _source.ToList();
                    list.Sort((x, y) => Comparer<TKey>.Default.Compare(
                        _keySelector(x), _keySelector(y)));
                    _enumerator = list.GetEnumerator();
                    _bookmark = 1;
                    goto case 1;

                case 1:
                    if (_enumerator.MoveNext())
                    {
                        _current = _enumerator.Current;
                        _bookmark = 1;
                        return true;
                    }
                    goto case -1;

                case -1:
                    _enumerator?.Dispose();
                    return false;

                default:
                    return false;
            }
        }

        public void Dispose()
        {
            if (_bookmark != -1)
            {
                _bookmark = -1;
                _enumerator?.Dispose();
            }
        }

        public void Reset() => throw new NotSupportedException();

        public IEnumerator<TSource> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
 
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
