using System;
using System.IO;
using System.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorEx.Editor.editor_ex.Scripts.Editor.Utils;
using UnityEditorInternal;
using UnityEngine;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed class AssemblyTree : TreeView
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

        private AssemblyData[] _projectAssemblies = Array.Empty<AssemblyData>();
        private AssemblyData[] _allAssemblies = Array.Empty<AssemblyData>();

        public AssemblyTree(TreeViewState state) : base(state)
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

            rowHeight = 20f;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            EditorGUI.indentLevel = args.item.depth;
            var indentedRect = EditorGUI.IndentedRect(args.rowRect);

            if (args.item is AssemblyTreeItem assemblyTreeItem)
            {
                GUI.DrawTexture(new Rect(indentedRect.x + 18f, indentedRect.y + 2f, 16f, 16f), _assemblyIcon);

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
                    GUI.DrawTexture(new Rect(indentedRect.x + 36f, indentedRect.y + 2f, 16f, 16f), secondIcon);
                }

                GUI.Label(new Rect(indentedRect.x + 54f, indentedRect.y, indentedRect.width - 54f, indentedRect.height),
                    assemblyTreeItem.Data.Name, EditorStyles.boldLabel);
            }
            else
            {
                var closeIcon = args.item.icon;
                var openIcon = args.item is ExpandTreeItem expandTreeItem ? expandTreeItem.ExpandedIcon : closeIcon;

                GUI.DrawTexture(new Rect(indentedRect.x + 18f, indentedRect.y + 2f, 16f, 16f), IsExpanded(args.item.id) ? openIcon : closeIcon);

                GUI.Label(new Rect(indentedRect.x + 36f, indentedRect.y, indentedRect.width - 36f, indentedRect.height),
                    args.label, args.item is HeaderTreeItem ? EditorStyles.boldLabel : EditorStyles.label);
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
            var counter = new Counter();
            foreach (var projectAssembly in _projectAssemblies)
            {
                counter++;

                var assemblyItem = new AssemblyTreeItem(projectAssembly) { id = counter };
                BuildAssemblyItem(assemblyItem);

                root.AddChild(assemblyItem);
            }

            void BuildAssemblyItem(AssemblyTreeItem assemblyItem)
            {
                counter++;
                var referenceItem = new HeaderTreeItem(counter)
                {
                    displayName = "References (" + assemblyItem.Data.References.Length + ")",
                    icon = (Texture2D)_dependencyIcon,
                    depth = 1
                };
                BuildReferencesItem(referenceItem, assemblyItem.Data);

                assemblyItem.AddChild(referenceItem);

                counter++;
                var sourcesItem = new HeaderTreeItem(counter)
                {
                    displayName = "Sources",
                    icon = (Texture2D)_scriptIcon,
                    depth = 1
                };
                BuildSourcesItem(sourcesItem, assemblyItem.Data);

                assemblyItem.AddChild(sourcesItem);

                void BuildReferencesItem(TreeViewItem item, AssemblyData assemblyData)
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
                            icon = (Texture2D)_assemblyReferenceIcon,
                            depth = 2
                        });
                    }
                }

                void BuildSourcesItem(TreeViewItem item, AssemblyData assemblyData)
                {
                    var assemblyFile = AssetDatabase.GetAssetPath(assemblyData.AssemblyDefinition);
                    var assemblyPath = Path.GetDirectoryName(assemblyFile);
                    var assetPaths = AssetDatabase.FindAssets("t:" + nameof(MonoScript), new[] { assemblyPath })
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .GroupBy(Path.GetDirectoryName, Path.GetFileName);

                    foreach (var asset in assetPaths)
                    {
                        counter++;
                        item.AddChild(new ExpandTreeItem(counter)
                        {
                            displayName = asset.Key,
                            icon = (Texture2D)_folderClose,
                            ExpandedIcon = (Texture2D)_folderOpen,
                            depth = 2,
                            children = asset
                                .Select(x => new TreeViewItem(++counter)
                                {
                                    displayName = x,
                                    icon = (Texture2D)_csIcon,
                                    depth = 3
                                })
                                .ToList()
                        });
                    }
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

        protected override bool CanRename(TreeViewItem item)
        {
            return item is AssemblyTreeItem;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            var treeViewItem = FindItem(args.itemID, rootItem);
            if (!(treeViewItem is AssemblyTreeItem assemblyTreeItem))
                return;

            if (string.IsNullOrWhiteSpace(args.newName) || args.newName.Any(x => x is '\\' or '/' || x == ':' || x == '?'))
            {
                args.acceptedRename = false;
                return;
            }

            assemblyTreeItem.Data.AssemblyDefinition.name = args.newName;
            args.acceptedRename = true;
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            return new Rect(rowRect.x + 38f, rowRect.y, rowRect.width - 38f, rowRect.height);
        }

        protected override void SingleClickedItem(int id)
        {
            var treeViewItem = FindItem(id, rootItem);
            if (!(treeViewItem is AssemblyTreeItem assemblyTreeItem))
                return;

            Selection.activeObject = assemblyTreeItem.Data.AssemblyDefinition;
        }

        protected override void DoubleClickedItem(int id)
        {
            /*var treeViewItem = FindItem(id, rootItem);
            if (!(treeViewItem is AssemblyTreeItem assemblyTreeItem))
                return;*/

            CodeEditor.CurrentEditor.OpenProject();
        }

        protected override void ContextClickedItem(int id)
        {
            var treeViewItem = FindItem(id, rootItem);
            if (treeViewItem is AssemblyTreeItem assemblyTreeItem)
            {
                var genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("Open Project in Editor"), false, () => CodeEditor.CurrentEditor.OpenProject());
                genericMenu.ShowAsContext();
            }
        }

        private sealed class AssemblyTreeItem : TreeViewItem
        {
            public AssemblyData Data { get; }

            public AssemblyTreeItem(AssemblyData data)
            {
                Data = data;
            }
        }

        private sealed class HeaderTreeItem : TreeViewItem
        {
            public HeaderTreeItem(int id) : base(id)
            {
            }
        }

        private sealed class ExpandTreeItem : TreeViewItem
        {
            public Texture2D ExpandedIcon { get; set; }

            public ExpandTreeItem(int id) : base(id)
            {
            }
        }

        private sealed class Counter
        {
            public int Counting { get; }

            public Counter()
            {
            }

            private Counter(int counting)
            {
                Counting = counting;
            }

            public static implicit operator int(Counter c)
            {
                return c.Counting;
            }

            public static Counter operator ++(Counter c)
            {
                var newValue = c.Counting + 1;
                return new Counter(newValue);
            }
        }
    }
}