using System;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Audio;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Media
{
    public sealed class MediaFilter
    {
        public static MediaFilter[] BuiltinFilters { get; } = new MediaFilter[]
        {
            new MediaFilter("Scenes", new MediaSubFilter("Scenes", "scene")),
            new MediaFilter("Audio", new MediaSubFilter("Audio Clips", nameof(AudioClip)),
                new MediaSubFilter("Audio Mixer", nameof(AudioMixer))), 
            new MediaFilter("Images", new MediaSubFilter("Sprites", nameof(Sprite)), new MediaSubFilter("Textures", nameof(Texture))),
            new MediaFilter("Scripts", new MediaSubFilter("Assemblies", nameof(AssemblyDefinitionAsset)), new MediaSubFilter("References", nameof(AssemblyDefinitionReferenceAsset)))
        };

        public string Name { get; set; }
        public MediaSubFilter[] Filters { get; set; }

        public string FilterLine => string.Join(' ', Filters.Select(x => x.FilterLine));

        public MediaFilter()
        {
        }

        public MediaFilter(string name, params MediaSubFilter[] filters)
        {
            Name = name;
            Filters = filters;
        }

        private bool Equals(MediaFilter other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is MediaFilter other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    public sealed class MediaSubFilter
    {
        public string Name { get; set; }
        public string Filter { get; set; }
        public MediaFilterType Type { get; set; }
        public string FilterLine => Type.ToFilter() + ":" + Filter;

        public MediaSubFilter()
        {
        }

        public MediaSubFilter(string name, string filter, MediaFilterType type = MediaFilterType.Type)
        {
            Name = name;
            Filter = filter;
            Type = type;
        }

        private bool Equals(MediaSubFilter other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is MediaSubFilter other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }

    public enum MediaFilterType
    {
        Type,
        Extension
    }

    internal static class MediaFilterTypeExtensions
    {
        public static string ToFilter(this MediaFilterType type)
        {
            return type switch
            {
                MediaFilterType.Type => "t",
                MediaFilterType.Extension => "ext",
                _ => throw new NotImplementedException(type.ToString())
            };
        }
    }
}