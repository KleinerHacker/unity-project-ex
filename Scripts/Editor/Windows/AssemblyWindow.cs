using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityCommonEx.Runtime.common_ex.Scripts.Runtime.Utils.Extensions;
using UnityEditor;
using UnityEditorEx.Editor.editor_ex.Scripts.Editor.Utils;
using UnityEditorInternal;
using UnityEngine;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows
{
    public sealed class AssemblyWindow : EditorWindow
    {
        private const int NameLimit = 20;
        
        [MenuItem("Window/General/Assembly", priority = 9)]
        public static void Show()
        {
            ((EditorWindow)CreateInstance<AssemblyWindow>()).Show();
        }

        private Texture _assemblyIcon;
        private Texture _assemblyReferenceIcon;

        private AssemblyData[] _projectAssemblies = Array.Empty<AssemblyData>();
        private AssemblyData[] _allAssemblies = Array.Empty<AssemblyData>();

        private Vector2 _scroll = Vector2.zero;
        private IDictionary<string, bool> _folds = new Dictionary<string, bool>();
        private bool _filterFold;
        private AssemblyType _assemblyProjectFilter = AssemblyType.Runtime | AssemblyType.Editor | AssemblyType.Test;
        private string _assemblyProjectNameFilter = "";
        private AssemblyType _assemblyReferenceFilter = AssemblyType.Runtime | AssemblyType.Editor | AssemblyType.Test;
        private AssemblyPlace _assemblyReferencePlaceFilter = AssemblyPlace.Package | AssemblyPlace.Project;
        private string _assembyReferenceNameFilter = "";
        private bool _useGuid = true;

        private void OnEnable()
        {
            titleContent = new GUIContent("Assembly", EditorGUIUtility.IconContent("Assembly Icon").image);
            minSize = new Vector2(350f, 100f);
            maxSize = new Vector2(400f, 1000f);
            
            _assemblyIcon = EditorGUIUtility.IconContent("AssemblyDefinitionAsset Icon").image;
            _assemblyReferenceIcon = EditorGUIUtility.IconContent("AssemblyDefinitionReferenceAsset Icon").image;
            
            OnValidate();
        }

        private void OnValidate()
        {
            _allAssemblies = AssetDatabase.FindAssets("t:" + nameof(AssemblyDefinitionAsset))
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                .Select(x => new AssemblyData(x))
                .ToArray();
            _projectAssemblies = AssetDatabase.FindAssets("t:" + nameof(AssemblyDefinitionAsset), new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>)
                .Select(x => new AssemblyData(x))
                .ToArray();
            
            hasUnsavedChanges = _projectAssemblies.Any(x => x.IsDirty);
        }

        private void OnGUI()
        {
            _filterFold = EditorGUILayout.BeginFoldoutHeaderGroup(_filterFold, "Filters");
            if (_filterFold)
            {
                EditorGUI.indentLevel = 1;
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Project:", GUILayout.Width(85f));
                    _assemblyProjectFilter = (AssemblyType)EditorGUILayout.EnumFlagsField(GUIContent.none, _assemblyProjectFilter, GUILayout.Width(100f));
                    _assemblyProjectNameFilter = EditorGUILayout.TextField(GUIContent.none, _assemblyProjectNameFilter, GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("References:", GUILayout.Width(85f));
                    EditorGUILayout.BeginVertical(GUILayout.Width(100f));
                    _assemblyReferenceFilter = (AssemblyType)EditorGUILayout.EnumFlagsField(GUIContent.none, _assemblyReferenceFilter, GUILayout.Width(100f));
                    _assemblyReferencePlaceFilter = (AssemblyPlace)EditorGUILayout.EnumFlagsField(GUIContent.none, _assemblyReferencePlaceFilter, GUILayout.Width(100f));
                    EditorGUILayout.EndVertical();
                    _assembyReferenceNameFilter = EditorGUILayout.TextField(GUIContent.none, _assembyReferenceNameFilter, GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            _useGuid = EditorGUILayout.Toggle("Use GUID for referencing", _useGuid);

            EditorGUILayout.Space();
            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUIStyle.none, GUI.skin.verticalScrollbar);
            foreach (var projectAssembly in _projectAssemblies
                         .Where(x => _assemblyProjectFilter.HasFlag(x.Type))
                         .Where(x => string.IsNullOrWhiteSpace(_assemblyProjectNameFilter) || x.Name.Contains(_assemblyProjectNameFilter, StringComparison.OrdinalIgnoreCase)))
            {
                var fold = _folds.GetOrDefault(projectAssembly.Name, false);
                fold = EditorGUILayout.BeginFoldoutHeaderGroup(fold, new GUIContent(projectAssembly.Name.Limit(NameLimit, "...") + " (" + projectAssembly.Type + ")", _assemblyIcon, projectAssembly.Name),
                    menuAction: _ =>
                    {
                        var genericMenu = new GenericMenu();
                        genericMenu.AddItem(new GUIContent("Select in tree"), false, () => Selection.activeObject = projectAssembly.AssemblyDefinition);
                        genericMenu.ShowAsContext();
                    }); 
                _folds.AddOrOverwrite(projectAssembly.Name, fold);

                if (fold)
                {
                    EditorGUI.indentLevel = 1;
                    foreach (var allAssembly in _allAssemblies
                                 .Where(x => _assemblyReferenceFilter.HasFlag(x.Type))
                                 .Where(x => _assemblyReferencePlaceFilter.HasFlag(x.Place))
                                 .Where(x => string.IsNullOrWhiteSpace(_assembyReferenceNameFilter) || x.Name.Contains(_assembyReferenceNameFilter, StringComparison.OrdinalIgnoreCase)))
                    {
                        EditorGUILayout.BeginHorizontal();
                        var selected = projectAssembly.References.Any(x =>
                        {
                            if (x.StartsWith("GUID:", StringComparison.OrdinalIgnoreCase))
                                return x.Substring(5) == AssetDatabaseEx.GetGUID(allAssembly.AssemblyDefinition).ToString();

                            return x == allAssembly.Name;
                        });
                        var newSelected = EditorGUILayout.ToggleLeft(new GUIContent(allAssembly.Name.Limit(NameLimit, "..."), _assemblyReferenceIcon, allAssembly.Name), selected, GUILayout.Width(175f));
                        if (selected != newSelected)
                        {
                            var guidElement = "GUID:" + AssetDatabaseEx.GetGUID(allAssembly.AssemblyDefinition);
                            if (newSelected)
                            {
                                projectAssembly.References = projectAssembly.References
                                    .Append(_useGuid ? guidElement : allAssembly.Name)
                                    .ToArray(); 
                            }
                            else
                            {
                                projectAssembly.References = projectAssembly.References
                                    .Remove(allAssembly.Name)
                                    .Remove(guidElement)
                                    .ToArray();
                            }
                            
                            hasUnsavedChanges = _projectAssemblies.Any(x => x.IsDirty);
                        }
                        EditorGUILayout.LabelField("(" + allAssembly.Type + " | " + allAssembly.Place + ")", EditorStyles.miniLabel, GUILayout.Width(125f));
                        EditorGUILayout.Space(0f, true);
                        if (GUILayout.Button(EditorGUIUtility.IconContent("_Menu").image, EditorStyles.iconButton))
                        {
                            var genericMenu = new GenericMenu();
                            genericMenu.AddItem(new GUIContent("Select in tree"), false, () => Selection.activeObject = allAssembly.AssemblyDefinition);
                            genericMenu.ShowAsContext();
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel = 0;
                }

                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
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
            
            using var reader = new JsonTextReader(new StringReader(Encoding.UTF8.GetString(AssemblyDefinition.bytes)));

            var includes = Array.Empty<string>();
            var excludes = Array.Empty<string>();
            var defineConstraints = Array.Empty<string>();
            while (reader.Read())
            {
                if (reader.Path.Equals("name", StringComparison.OrdinalIgnoreCase))
                {
                    name = (string)reader.Value;
                }
                else if (reader.Path.StartsWith("includePlatforms[", StringComparison.OrdinalIgnoreCase))
                {
                    includes = includes.Append((string)reader.Value).ToArray();
                }
                else if (reader.Path.StartsWith("excludePlatforms[", StringComparison.OrdinalIgnoreCase))
                {
                    includes = includes.Append((string)reader.Value).ToArray();
                }
                else if (reader.Path.StartsWith("references[", StringComparison.OrdinalIgnoreCase))
                {
                    references = references.Append((string)reader.Value).ToArray();
                }
                else if (reader.Path.StartsWith("defineConstraints[", StringComparison.OrdinalIgnoreCase))
                {
                    defineConstraints = defineConstraints.Append((string)reader.Value).ToArray();
                }
            }

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

            Place = AssetDatabase.GetAssetPath(AssemblyDefinition).StartsWith("Assets", StringComparison.OrdinalIgnoreCase)
                ? AssemblyPlace.Project
                : AssemblyPlace.Package;
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