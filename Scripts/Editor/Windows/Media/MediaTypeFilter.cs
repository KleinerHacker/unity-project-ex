using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Media
{
    public sealed class MediaTypeFilter
    {
        public static MediaTypeFilter[] BuiltinFilters { get; } = new MediaTypeFilter[]
        {
            new MediaTypeFilter("Scenes", new MediaTypeSubFilter("Scenes", "scene")),
            new MediaTypeFilter("Audio", new MediaTypeSubFilter("Audio Clips", nameof(AudioClip)),
                new MediaTypeSubFilter("Audio Mixer", nameof(AudioMixer))),
            new MediaTypeFilter("Images", new MediaTypeSubFilter("Sprites", nameof(Sprite)), new MediaTypeSubFilter("Textures", nameof(Texture))),
        };

        public string Name { get; set; }
        public MediaTypeSubFilter[] Filters { get; set; }

        public string FilterLine => string.Join(' ', Filters.Select(x => x.FilterLine));

        public MediaTypeFilter()
        {
        }

        public MediaTypeFilter(string name, params MediaTypeSubFilter[] filters)
        {
            Name = name;
            Filters = filters;
        }

        private bool Equals(MediaTypeFilter other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is MediaTypeFilter other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    public sealed class MediaTypeSubFilter
    {
        public string Name { get; set; }
        public string Filter { get; set; }
        public string FilterLine => "t:" + Filter;

        public MediaTypeSubFilter()
        {
        }

        public MediaTypeSubFilter(string name, string filter)
        {
            Name = name;
            Filter = filter;
        }

        private bool Equals(MediaTypeSubFilter other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is MediaTypeSubFilter other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}