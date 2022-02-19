using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using AnimatorController = UnityEditor.Animations.AnimatorController;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Media
{
    public sealed class MediaFilter
    {
        public static MediaFilter[] BuiltinFilters { get; } = new[]
        {
            new MediaFilter("Scenes", new MediaSubFilter("Scenes", "scene"), new MediaSubFilter("Lightning", nameof(LightingSettings)),
                new MediaSubFilter("Lightning Mapping", nameof(LightmapParameters))),
            new MediaFilter("Audio", new MediaSubFilter("Audio Clips", nameof(AudioClip)),
                new MediaSubFilter("Audio Mixer", nameof(AudioMixer))),
            new MediaFilter("Images", new MediaSubFilter("Sprites", nameof(Sprite)), new MediaSubFilter("Textures (2D)", nameof(Texture2D)),
                new MediaSubFilter("Textures (3D)", nameof(Texture3D)), new MediaSubFilter("Cube Maps", nameof(Cubemap)), new MediaSubFilter("Render Textures", nameof(RenderTexture)),
                new MediaSubFilter("Materials", nameof(Material))),
            new MediaFilter("Animations", new MediaSubFilter("Animations", nameof(AnimationClip)), new MediaSubFilter("Animators", nameof(AnimatorController))),
            new MediaFilter("Physics", new MediaSubFilter("Physics 3D", nameof(PhysicMaterial)), new MediaSubFilter("Physics 2D", nameof(PhysicsMaterial2D))),
            new MediaFilter("Graphics", new MediaSubFilter("Shader", nameof(Shader)), new MediaSubFilter("Render Pipelines", nameof(RenderPipelineAsset)),
                #if URP
                new MediaSubFilter("Render Data", nameof(ScriptableRendererData)),
                #endif
                new MediaSubFilter("Post Processing", nameof(VolumeProfile))),
            new MediaFilter("Scripts", new MediaSubFilter("Assemblies", nameof(AssemblyDefinitionAsset)),
                new MediaSubFilter("References", nameof(AssemblyDefinitionReferenceAsset)), new MediaSubFilter("Source Code", nameof(MonoScript))),
            new MediaFilter("Models", new MediaSubFilter("Models", nameof(Mesh)), new MediaSubFilter("Materials", nameof(Material)),
                new MediaSubFilter("Animations", nameof(AnimationClip)), new MediaSubFilter("Animators", nameof(AnimatorController))),
            new MediaFilter("Prefabs", new MediaSubFilter("Prefabs", "prefab")),
            new MediaFilter("UXML", new MediaSubFilter("Style Sheets", nameof(StyleSheet)), new MediaSubFilter("Visual Trees", nameof(VisualTreeAsset)),
                new MediaSubFilter("Source Code", nameof(MonoScript))),
            new MediaFilter("Others", new MediaSubFilter("Texts", nameof(TextAsset)), new MediaSubFilter("Fonts", nameof(Font))),
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
        Prefab
    }

    internal static class MediaFilterTypeExtensions
    {
        public static string ToFilter(this MediaFilterType type)
        {
            return type switch
            {
                MediaFilterType.Type => "t",
                MediaFilterType.Prefab => "prefab",
                _ => throw new NotImplementedException(type.ToString())
            };
        }
    }
}