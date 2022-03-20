using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Codice.Client.BaseCommands;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Utils.Extensions;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows
{
    public sealed class AssemblyWindow : EditorWindow
    {
        [MenuItem("Window/General/Assembly", priority = 9)]
        public new static void Show()
        {
            ((EditorWindow)CreateInstance<AssemblyWindow>()).Show();
        }

        private AssemblyData[] _projectAssemblies = Array.Empty<AssemblyData>();

        private bool _useGuid = true;

        private AssemblyTree _assemblyTree;

        private Texture _expandAllIcon;
        private Texture _collapseAllIcon;

        private void OnEnable()
        {
            _expandAllIcon = EditorGUIUtility.IconContent("FolderOpened Icon").image;
            _collapseAllIcon = EditorGUIUtility.IconContent("Folder Icon").image;
            
            titleContent = new GUIContent("Assembly", EditorGUIUtility.IconContent("Assembly Icon").image);
            minSize = new Vector2(350f, 100f);
            maxSize = new Vector2(400f, 1000f);

            _assemblyTree = new AssemblyTree(new TreeViewState());
            _assemblyTree.Reload();

            OnValidate();
        }

        private void OnValidate()
        {
            _projectAssemblies = AssetDatabase.FindAssets("t:" + nameof(AssemblyDefinitionAsset), new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                .Select(x => new AssemblyData(x))
                .ToArray();

            hasUnsavedChanges = _projectAssemblies.Any(x => x.IsDirty);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(_collapseAllIcon, "Collapse all"), EditorStyles.iconButton))
            {
                _assemblyTree.CollapseAll();
            }

            if (GUILayout.Button(new GUIContent(_expandAllIcon, "Expand all"), EditorStyles.iconButton))
            {
                _assemblyTree.ExpandAll();
            }
            EditorGUILayout.EndHorizontal();

            _useGuid = EditorGUILayout.Toggle("Use GUID for referencing", _useGuid);

            EditorGUILayout.Space();
            
            var controlRect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _assemblyTree.OnGUI(controlRect);

            EditorGUI.BeginDisabledGroup(_projectAssemblies.All(x => !x.IsDirty));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Changes"))
            {
                foreach (var projectAssembly in _projectAssemblies)
                {
                    projectAssembly.Store();
                }

                hasUnsavedChanges = _projectAssemblies.Any(x => x.IsDirty);
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Revert Changes"))
            {
                foreach (var projectAssembly in _projectAssemblies)
                {
                    projectAssembly.Revert();
                }

                hasUnsavedChanges = _projectAssemblies.Any(x => x.IsDirty);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }
    }

    public sealed class AssemblyData
    {
        private static readonly Regex NameRegex = new Regex(@"""name""\:\s*""([^""]*)""");
        private static readonly Regex ReferencesRegex = new Regex(@"""references""\:\s*\[([^\]]*)\]");
        private static readonly Regex IncludesRegex = new Regex(@"""includePlatforms""\:\s*\[([^\]]*)\]");
        private static readonly Regex ExcludesRegex = new Regex(@"""excludePlatforms""\:\s*\[([^\]]*)\]");
        private static readonly Regex DefinesRegex = new Regex(@"""defineConstraints""\:\s*\[([^\]]*)\]");

        private string name;
        private string[] references = Array.Empty<string>();

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

        public AssemblyType Type { get; private set; }
        public AssemblyPlace Place { get; private set; }

        public string[] References
        {
            get => references;
            set
            {
                references = value;
                IsDirty = true;
            }
        }

        public bool IsDirty { get; private set; }

        public AssemblyData(AssemblyDefinitionAsset assemblyDefinition)
        {
            AssemblyDefinition = assemblyDefinition;
            ReloadAssembly();
        }

        private void ReloadAssembly()
        {
            references = Array.Empty<string>();

            var json = Encoding.UTF8.GetString(AssemblyDefinition.bytes);

            var nameMatch = NameRegex.Match(json);
            if (!nameMatch.Success)
                throw new InvalidOperationException("Match for assembly name has failed");
            name = nameMatch.Groups[1].Value;

            var referencesMatch = ReferencesRegex.Match(json);
            references = referencesMatch.Success ? referencesMatch.Groups[1].Value.SplitJson() : Array.Empty<string>();

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

        public void Store()
        {
            if (!IsDirty)
                return;

            var json = Encoding.UTF8.GetString(AssemblyDefinition.bytes);

            var match = NameRegex.Match(json);
            if (!match.Success)
                throw new InvalidOperationException("Match for assembly name has failed");
            json = json.Replace(match.Groups[1].Value, Name);

            var rawReferences = string.Join("," + Environment.NewLine, References.Select(x => "\"" + x + "\""));
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