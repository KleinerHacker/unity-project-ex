using System;
using System.Linq;
using Codice.Client.Common;
using UnityEditor;
using UnityEditorEx.Editor.editor_ex.Scripts.Editor.Utils;
using UnityEditorInternal;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Types;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Utils
{
    internal static class AssemblyReferenceUtils
    {
        public static string CreateReference(AssemblyDefinitionAsset asset, bool useGuid)
        {
            if (useGuid)
                return "GUID:" + AssetDatabaseEx.GetGUID(asset);

            return AssetDatabase.GetAssetPath(asset);
        }

        public static AssemblyDefinitionAsset FromReference(string reference)
        {
            if (reference.StartsWith("guid:", StringComparison.OrdinalIgnoreCase))
                return AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(AssetDatabase.GUIDToAssetPath(reference.Substring(5)));

            return AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(reference);
        }

        public static AssemblyData[] FindAllAssemblies()
        {
            return AssetDatabase.FindAssets("t:" + nameof(AssemblyDefinitionAsset))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                .Select(x => new AssemblyData(x))
                .ToArray();
        }

        public static AssemblyData[] FindProjectAssemblies(string searchText = null, AssemblyType searchType = AssemblyType.Runtime | AssemblyType.Editor | AssemblyType.Test)
        {
            return AssetDatabase.FindAssets("t:" + nameof(AssemblyDefinitionAsset), new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                .Select(x => new AssemblyData(x))
                .Where(x => string.IsNullOrWhiteSpace(searchText) || x.Name.Contains(searchText))
                .Where(x => searchType.HasFlag(x.Type))
                .ToArray();
        }
    }
}