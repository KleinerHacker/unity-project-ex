using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Types;
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

            _assemblyTree = new AssemblyTree(new TreeViewState(), () => _useGuid);
            _assemblyTree.Refresh();

            OnValidate();
        }

        private void OnValidate()
        {
            hasUnsavedChanges = _assemblyTree?.HasUnsavedChanges ?? false;
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(_collapseAllIcon, "Collapse all"), new GUIStyle(EditorStyles.toolbarButton) { fixedWidth = 25f }))
            {
                _assemblyTree.CollapseAll();
            }

            if (GUILayout.Button(new GUIContent(_expandAllIcon, "Expand all"), new GUIStyle(EditorStyles.toolbarButton) { fixedWidth = 25f }))
            {
                _assemblyTree.ExpandAll();
            }

            _assemblyTree.SearchText = EditorGUILayout.TextField(GUIContent.none, _assemblyTree.SearchText, EditorStyles.toolbarTextField, GUILayout.MinWidth(100f), GUILayout.MaxWidth(300f));
            _assemblyTree.SearchType = (AssemblyType)EditorGUILayout.EnumFlagsField(GUIContent.none, _assemblyTree.SearchType, EditorStyles.toolbarPopup, GUILayout.Width(100f));
            EditorGUILayout.EndHorizontal();

            _useGuid = EditorGUILayout.Toggle("Use GUID for referencing", _useGuid);

            EditorGUILayout.Space();

            var controlRect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            _assemblyTree.OnGUI(controlRect);

            EditorGUI.BeginDisabledGroup(!_assemblyTree.HasUnsavedChanges);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Changes"))
            {
                _assemblyTree.ApplyChanges(_useGuid);
                hasUnsavedChanges = _assemblyTree.HasUnsavedChanges;
                
                AssetDatabase.Refresh();
            }

            if (GUILayout.Button("Discard Changes"))
            {
                _assemblyTree.DiscardChanges();
                hasUnsavedChanges = _assemblyTree.HasUnsavedChanges;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }
    }
}