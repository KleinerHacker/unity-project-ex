using System;
using System.Linq;
using Codice.Client.BaseCommands;
using UnityCommonEx.Runtime.common_ex.Scripts.Runtime.Utils.Extensions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Types;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Utils;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed class AssemblyReferenceManagerWindow : EditorWindow
    {
        public static AssemblyDefinitionAsset[] Show(AssemblyDefinitionAsset[] referencedAssemblies)
        {
            var assemblyReferenceManagerWindow = CreateInstance<AssemblyReferenceManagerWindow>();
            assemblyReferenceManagerWindow._selectedAssemblies = referencedAssemblies;
            assemblyReferenceManagerWindow.ShowModal();

            return assemblyReferenceManagerWindow.IsReturnPositive ? assemblyReferenceManagerWindow._selectedAssemblies : null;
        }

        private AssemblyDefinitionAsset[] _selectedAssemblies = Array.Empty<AssemblyDefinitionAsset>();
        private AssemblyData[] _allAssemblies = Array.Empty<AssemblyData>();
        private Vector2 _scroll;
        private string _searchText = null;
        private AssemblyType _searchType = AssemblyType.Runtime | AssemblyType.Editor;

        public bool IsReturnPositive { get; private set; }

        private void Awake()
        {
            _allAssemblies = AssemblyReferenceUtils.FindAllAssemblies();

            titleContent = new GUIContent("Assembly References Manager", EditorGUIUtility.IconContent("Assembly Icon").image);

            minSize = new Vector2(500f, 500f);
            maxSize = new Vector2(500f, 1000f);
        }

        private void OnEnable()
        {
            var size = focusedWindow.position.size;
            position = new Rect(size.x / 2f - maxSize.x / 2f, size.y / 2f - maxSize.y / 2f, maxSize.x, maxSize.y);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                _searchText = EditorGUILayout.TextField(GUIContent.none, _searchText, EditorStyles.toolbarTextField, GUILayout.MinWidth(150f), GUILayout.MaxWidth(300f));
                _searchType = (AssemblyType)EditorGUILayout.EnumFlagsField(GUIContent.none, _searchType, EditorStyles.toolbarPopup, GUILayout.MinWidth(100f), GUILayout.MaxWidth(200f));
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            {
                foreach (var assemblyData in _allAssemblies
                             .Where(x => string.IsNullOrWhiteSpace(_searchText) || x.Name.Contains(_searchText, StringComparison.OrdinalIgnoreCase))
                             .Where(x => _searchType.HasFlag(x.Type))
                         )
                {
                    var selected = _selectedAssemblies.Any(x => x == assemblyData.AssemblyDefinition);
                    var newSelected = GUILayout.Toggle(selected, new GUIContent(assemblyData.Name, assemblyData.FileName));
                    if (newSelected != selected)
                    {
                        if (newSelected)
                        {
                            _selectedAssemblies = _selectedAssemblies.Append(assemblyData.AssemblyDefinition).ToArray();
                        }
                        else
                        {
                            _selectedAssemblies = _selectedAssemblies.Remove(assemblyData.AssemblyDefinition).ToArray();
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Apply"))
                {
                    IsReturnPositive = true;
                    Close();
                }

                if (GUILayout.Button("Discard"))
                {
                    IsReturnPositive = false;
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}