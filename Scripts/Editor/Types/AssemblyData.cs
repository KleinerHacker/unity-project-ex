using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Utils;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Utils.Extensions;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Types
{
    public sealed class AssemblyData
    {
        private static readonly Regex NameRegex = new Regex(@"""name""\:\s*""([^""]*)""");
        private static readonly Regex ReferencesRegex = new Regex(@"""references""\:\s*\[([^\]]*)\]");
        private static readonly Regex IncludesRegex = new Regex(@"""includePlatforms""\:\s*\[([^\]]*)\]");
        private static readonly Regex ExcludesRegex = new Regex(@"""excludePlatforms""\:\s*\[([^\]]*)\]");
        private static readonly Regex DefinesRegex = new Regex(@"""defineConstraints""\:\s*\[([^\]]*)\]");

        private string name;

        public AssemblyDefinitionAsset AssemblyDefinition { get; }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                IsDirty = true;
            }
        }

        public string FileName { get; private set; }
        public string FilePath { get; private set; }

        public AssemblyType Type { get; private set; }
        public AssemblyPlace Place { get; private set; }

        public ReferenceInfo[] References { get; private set; } = Array.Empty<ReferenceInfo>();

        public bool IsDirty { get; private set; }

        public AssemblyData(AssemblyDefinitionAsset assemblyDefinition)
        {
            AssemblyDefinition = assemblyDefinition;

            var assetPath = AssetDatabase.GetAssetPath(assemblyDefinition);
            FileName = Path.GetFileName(assetPath);
            FilePath = Path.GetDirectoryName(assetPath);

            ReloadAssembly();
        }

        private void ReloadAssembly()
        {
            References = Array.Empty<ReferenceInfo>();

            var json = Encoding.UTF8.GetString(AssemblyDefinition.bytes);

            var nameMatch = NameRegex.Match(json);
            if (!nameMatch.Success)
                throw new InvalidOperationException("Match for assembly name has failed");
            name = nameMatch.Groups[1].Value;

            var referencesMatch = ReferencesRegex.Match(json);
            var rawReferences = referencesMatch.Success ? referencesMatch.Groups[1].Value.SplitJson() : Array.Empty<string>();
            References = rawReferences
                .Select(AssemblyReferenceUtils.FromReference)
                .Select(x => new ReferenceInfo(x))
                .ToArray();

            var includesMatch = IncludesRegex.Match(json);
            var includes = includesMatch.Success ? includesMatch.Groups[1].Value.SplitJson() : Array.Empty<string>();

            var excludesMatch = ExcludesRegex.Match(json);
            var excludes = excludesMatch.Success ? excludesMatch.Groups[1].Value.SplitJson() : Array.Empty<string>();

            var definesMatch = DefinesRegex.Match(json);
            var defineConstraints = definesMatch.Success ? definesMatch.Groups[1].Value.SplitJson() : Array.Empty<string>();

            if (defineConstraints.Contains("UNITY_INCLUDE_TESTS"))
            {
                Type = AssemblyType.Test;
            }
            else if (includes.Contains("Editor") && !excludes.Contains("Editor"))
            {
                Type = AssemblyType.Editor;
            }
            else
            {
                Type = AssemblyType.Runtime;
            }

            Place = AssetDatabase.GetAssetPath(AssemblyDefinition).StartsWith("Assets", StringComparison.OrdinalIgnoreCase) ? AssemblyPlace.Project : AssemblyPlace.Package;
        }

        public void UpdateReferences(AssemblyDefinitionAsset[] references)
        {
            References = references
                .Select(x => new ReferenceInfo(x))
                .ToArray();
            IsDirty = true;
        }

        public void Store(bool useGuid)
        {
            if (!IsDirty)
                return;

            var json = Encoding.UTF8.GetString(AssemblyDefinition.bytes);

            var match = NameRegex.Match(json);
            if (!match.Success)
                throw new InvalidOperationException("Match for assembly name has failed");
            json = json.Replace(match.Groups[1].Value, Name);

            var rawReferences = string.Join(
                "," + Environment.NewLine,
                References
                    .Select(x => AssemblyReferenceUtils.CreateReference(x.AssemblyDefinition, useGuid))
                    .Select(x => "\"" + x + "\"")
            );
            match = ReferencesRegex.Match(json);
            json = !match.Success ? json.Insert(json.Length - 1, "\"references\": [" + rawReferences + "]") :
                string.IsNullOrEmpty(match.Groups[1].Value) ? json.Insert(match.Groups[1].Index, rawReferences) :
                json.Replace(match.Groups[1].Value, rawReferences);

            File.WriteAllText(AssetDatabase.GetAssetPath(AssemblyDefinition), json);
            EditorUtility.SetDirty(AssemblyDefinition);

            IsDirty = false;
        }

        public void Revert()
        {
            if (!IsDirty)
                return;

            ReloadAssembly();
            IsDirty = false;
        }
    }

    public struct ReferenceInfo
    {
        public AssemblyDefinitionAsset AssemblyDefinition { get; }

        public ReferenceInfo(AssemblyDefinitionAsset assemblyDefinition)
        {
            AssemblyDefinition = assemblyDefinition;
        }
    }

    [Flags]
    public enum AssemblyType
    {
        None = 0x00,
        Runtime = 0x01,
        Editor = 0x02,
        Test = 0x04
    }

    [Flags]
    public enum AssemblyPlace
    {
        None = 0x00,
        Project = 0x01,
        Package = 0x02
    }
}