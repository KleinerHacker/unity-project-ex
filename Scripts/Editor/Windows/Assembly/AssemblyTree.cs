using System;
using System.Linq;
using Codice.Client.BaseCommands;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorEx.Editor.editor_ex.Scripts.Editor.Utils;
using UnityEditorInternal;
using UnityEngine;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed class AssemblyTree : TreeView
    {
        private Texture _assemblyIcon;
        private Texture _assemblyReferenceIcon;
        private Texture _scriptIcon;
        private Texture _editorIcon;
        private Texture _gameIcon;
        private Texture _testIcon;

        private AssemblyData[] _projectAssemblies = Array.Empty<AssemblyData>();
        private AssemblyData[] _allAssemblies = Array.Empty<AssemblyData>();

        public AssemblyTree(TreeViewState state) : base(state)
        {
            _assemblyIcon = EditorGUIUtility.IconContent("AssemblyDefinitionAsset Icon").image;
            _assemblyReferenceIcon = EditorGUIUtility.IconContent("AssemblyDefinitionReferenceAsset Icon").image;
            _scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
            _editorIcon = EditorGUIUtility.IconContent("BuildSettings.Editor.Small").image;
            _gameIcon = EditorGUIUtility.IconContent("UnityEditor.GameView").image;
            _testIcon = EditorGUIUtility.IconContent("TestPassed").image;

            rowHeight = 20f;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            EditorGUI.indentLevel = args.item.depth;
            var indentedRect = EditorGUI.IndentedRect(args.rowRect);

            if (args.item is AssemblyTreeItem assemblyTreeItem)
            {
                var clicked = GUI.Button(new Rect(indentedRect.x + 2f, indentedRect.y + 2f, 16f, 16f),
                    new GUIContent(_assemblyIcon, "Click to expand / collapse"), EditorStyles.iconButton);
                if (clicked)
                {
                    SetExpanded(args.item.id, !IsExpanded(args.item.id));
                }

                var secondIcon = assemblyTreeItem.Data.Type switch
                {
                    AssemblyType.None => null,
                    AssemblyType.Runtime => (Texture2D)_gameIcon,
                    AssemblyType.Editor => (Texture2D)_editorIcon,
                    AssemblyType.Test => (Texture2D)_testIcon,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (secondIcon != null)
                {
                    GUI.DrawTexture(new Rect(indentedRect.x + 20f, indentedRect.y + 2f, 16f, 16f), secondIcon);
                }

                GUI.Label(new Rect(indentedRect.x + 38f, indentedRect.y, indentedRect.width - 38f, indentedRect.height), assemblyTreeItem.Data.Name);
            }
            else
            {
                if (GUI.Button(new Rect(indentedRect.x + 2f, indentedRect.y + 2f, 16f, 16f), args.item.icon, EditorStyles.iconButton))
                {
                    SetExpanded(args.item.id, !IsExpanded(args.item.id));
                }

                GUI.Label(new Rect(indentedRect.x + 20f, indentedRect.y, indentedRect.width - 20f, indentedRect.height), args.label);
            }
        }

        protected override TreeViewItem BuildRoot()
        {
            UpdateAssemblies();

            var root = new TreeViewItem();
            BuildTree(root);

            return root; 
        }

        private void BuildTree(TreeViewItem root)
        {
            var counter = 0;
            foreach (var projectAssembly in _projectAssemblies)
            {
                counter++;
                
                var assemblyItem = new AssemblyTreeItem(projectAssembly) {id = counter};
                BuildAssemblyItem(assemblyItem, ref counter);

                root.AddChild(assemblyItem);
            }

            void BuildAssemblyItem(AssemblyTreeItem assemblyItem, ref int counter)
            {
                counter++;
                var referenceItem = new TreeViewItem(counter)
                {
                    displayName = "References (" + assemblyItem.Data.References.Length + ")",
                    icon = (Texture2D)_assemblyReferenceIcon,
                    depth = 1
                };
                BuildReferencesItem(referenceItem, assemblyItem.Data, ref counter);
                
                assemblyItem.AddChild(referenceItem);

                counter++;
                var sourcesItem = new TreeViewItem(counter)
                {
                    displayName = "Sources",
                    icon = (Texture2D)_scriptIcon,
                    depth = 1
                };
                BuildSourcesItem(sourcesItem, assemblyItem.Data, ref counter);
                
                assemblyItem.AddChild(sourcesItem);

                void BuildReferencesItem(TreeViewItem item, AssemblyData assemblyData, ref int counter)
                {
                    foreach (var reference in assemblyData.References)
                    {
                        //TODO: GUID handling
                        var refData = _allAssemblies.FirstOrDefault(x => AssetDatabaseEx.GetGUID(x.AssemblyDefinition).ToString() == reference.Substring(5));
                        if (refData == null)
                            continue; //TODO

                        counter++;
                        item.AddChild(new TreeViewItem(counter)
                        {
                            displayName = refData.Name,
                            icon = (Texture2D)_assemblyIcon,
                            depth = 2
                        });
                    }
                }

                void BuildSourcesItem(TreeViewItem item, AssemblyData assemblyData, ref int counter)
                {
                    
                }
            }
        }

        private void UpdateAssemblies()
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
        }

        private sealed class AssemblyTreeItem : TreeViewItem
        {
            public AssemblyData Data { get; }

            public AssemblyTreeItem(AssemblyData data)
            {
                Data = data;
            }
        }
    }
}