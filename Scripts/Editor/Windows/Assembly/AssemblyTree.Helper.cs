namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed partial class AssemblyTree
    {
        private sealed class Counter
        {
            public int Counting { get; }

            public Counter()
            {
            }

            private Counter(int counting)
            {
                Counting = counting;
            }

            public static implicit operator int(Counter c)
            {
                return c.Counting;
            }

            public static Counter operator ++(Counter c)
            {
                var newValue = c.Counting + 1;
                return new Counter(newValue);
            }
        }
    }
}