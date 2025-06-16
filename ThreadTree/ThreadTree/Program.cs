namespace ThreadTree
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
        }
    }
    public class Tree<T>
    {
        private readonly Func<T, T, int> _comparer;

        public Tree(Func<T, T, int> comparer)
        {
            _comparer = comparer;
        }

        private class Node
        {
            public T Value;
            public Node Left;
            public Node Right;
        }

        private volatile Node _root;

        public void Add(T value)
        {
            var newNode = new Node { Value = value };

            while (true)
            {
                if (_root == null)
                {
                    if (Interlocked.CompareExchange(ref _root, newNode, null) == null)
                        return;
                    continue;
                }

                Node current = _root;
                while (true)
                {
                    int cmp = _comparer(value, current.Value);

                    if (cmp == 0) return;

                    ref Node child = ref (cmp < 0 ? ref current.Left : ref current.Right);
                    if (child == null)
                    {
                        if (Interlocked.CompareExchange(ref child, newNode, null) == null)
                            return;
                        break;
                    }
                    current = child;
                }
            }
        }

        public bool Contains(T value)
        {
            Node current = _root;

            while (current != null)
            {
                int cmp = _comparer(value, current.Value);
                if (cmp == 0) return true;
                current = cmp < 0 ? current.Left : current.Right;
            }

            return false;
        }
    }
}
