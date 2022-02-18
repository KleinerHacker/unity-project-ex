namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Media
{
    public sealed class MediaScope
    {
        public static MediaScope[] BuiltinScopes { get; } = new[]
        {
            new MediaScope("Project", "Assets")
        };
        
        public string Name { get; set; }
        public string[] Paths { get; set; }

        public MediaScope()
        {
        }

        public MediaScope(string name, params string[] paths)
        {
            Name = name;
            Paths = paths;
        }

        private bool Equals(MediaScope other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is MediaScope other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}