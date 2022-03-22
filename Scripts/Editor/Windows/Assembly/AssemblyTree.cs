using System;
using Codice.Client.BaseCommands;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed partial class AssemblyTree : TreeView
    {
        private readonly Texture _assemblyIcon;
        private readonly Texture _assemblyReferenceIcon;
        private readonly Texture _scriptIcon;
        private readonly Texture _csIcon;
        private readonly Texture _dependencyIcon;
        private readonly Texture _editorIcon;
        private readonly Texture _gameIcon;
        private readonly Texture _testIcon;
        private readonly Texture _folderOpen;
        private readonly Texture _folderClose;

        private readonly Func<bool> _useGuidGetter;

        public AssemblyTree(TreeViewState state, Func<bool> useGuidGetter) : base(state)
        {
            _assemblyIcon = EditorGUIUtility.IconContent("AssemblyDefinitionAsset Icon").image;
            _assemblyReferenceIcon = EditorGUIUtility.IconContent("AssemblyDefinitionReferenceAsset Icon").image;
            _scriptIcon = EditorGUIUtility.IconContent("Occlusion").image;
            _dependencyIcon = EditorGUIUtility.IconContent("EditCollider").image;
            _csIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
            _editorIcon = EditorGUIUtility.IconContent("BuildSettings.Editor.Small").image;
            _gameIcon = EditorGUIUtility.IconContent("UnityEditor.GameView").image;
            _testIcon = EditorGUIUtility.IconContent("TestPassed").image;
            _folderOpen = EditorGUIUtility.IconContent("FolderOpened Icon").image;
            _folderClose = EditorGUIUtility.IconContent("Folder Icon").image;

            _useGuidGetter = useGuidGetter;

            rowHeight = 20f;
        }
    }
}