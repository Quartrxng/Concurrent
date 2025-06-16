namespace Concurrent_Dictionary
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
    public class CumConcurrentDictionary<TKey, TValue>
    {
        private volatile Dictionary<TKey, TValue> _dictionary;
        private readonly IEqualityComparer<TKey> _comparer;

        public CumConcurrentDictionary(IEqualityComparer<TKey> comparer = null)
        {
            _dictionary = new Dictionary<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default);
            _comparer = _dictionary.Comparer;
        }

        public int Count => _dictionary.Count;

        public bool TryAdd(TKey key, TValue value)
        {
            while (true)
            {
                var current = _dictionary;
                if (current.ContainsKey(key))
                    return false;

                var copy = new Dictionary<TKey, TValue>(current, _comparer);
                copy.Add(key, value);

                if (Interlocked.CompareExchange(ref _dictionary, copy, current) == current)
                    return true;
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public bool TryRemove(TKey key, out TValue value)
        {
            while (true)
            {
                var current = _dictionary;
                if (!current.TryGetValue(key, out value))
                    return false;

                var copy = new Dictionary<TKey, TValue>(current, _comparer);
                copy.Remove(key);

                if (Interlocked.CompareExchange(ref _dictionary, copy, current) == current)
                    return true;
            }
        }

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            while (true)
            {
                if (TryGetValue(key, out var value))
                    return value;

                value = valueFactory(key);
                if (TryAdd(key, value))
                    return value;
            }
        }

        public Dictionary<TKey, TValue> GetAll()
        {
            return new Dictionary<TKey, TValue>(_dictionary);
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> AsEnumerable()
        {
            foreach (var pair in _dictionary)
            {
                yield return pair;
            }
        }
    }
}
