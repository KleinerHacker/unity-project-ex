using System.Linq;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Media
{
    public sealed class MediaScope
    {
        public static MediaScope[] BuiltinScopes { get; } = new[]
        {
            new MediaScope("Project", new MediaSubScope("Assets", "Assets")),
            new MediaScope("Test", new MediaSubScope("Animation Extensions", "Assets/animation"), new MediaSubScope("Extensions", "Assets/extension"))
        };
        
        public string Name { get; set; }
        public MediaSubScope[] SubScopes { get; set; }
        public string[] Paths => SubScopes.Select(x => x.Path).ToArray();

        public MediaScope()
        {
        }

        public MediaScope(string name, params MediaSubScope[] subScopes)
        {
            Name = name;
            SubScopes = subScopes;
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

    public sealed class MediaSubScope
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public MediaSubScope()
        {
        }

        public MediaSubScope(string name, string path)
        {
            Name = name;
            Path = path;
        }

        private bool Equals(MediaSubScope other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is MediaSubScope other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}