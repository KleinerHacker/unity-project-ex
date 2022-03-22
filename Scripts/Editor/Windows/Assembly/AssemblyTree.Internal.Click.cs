using System.Linq;
using Codice.Client.BaseCommands;
using Unity.CodeEditor;
using UnityCommonEx.Runtime.common_ex.Scripts.Runtime.Utils.Extensions;
using UnityEditor;
using UnityEngine;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Types;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed partial class AssemblyTree
    {
        protected override void SingleClickedItem(int id)
        {
            var treeViewItem = FindItem(id, rootItem);
            if (treeViewItem is AssemblyTreeItem assemblyTreeItem)
            {
                Selection.activeObject = assemblyTreeItem.Data.AssemblyDefinition;
            }
            else if (treeViewItem is SourceFileTreeItem sourceFileTreeItem)
            {
                Selection.activeObject = sourceFileTreeItem.MonoScript;
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            var treeViewItem = FindItem(id, rootItem);

            var path = "";
            if (treeViewItem is AssemblyTreeItem assemblyTreeItem)
            {
                path = AssetDatabase.GetAssetPath(assemblyTreeItem.Data.AssemblyDefinition);
            }
            else if (treeViewItem is SourceFileTreeItem sourceFileTreeItem)
            {
                path = AssetDatabase.GetAssetPath(sourceFileTreeItem.MonoScript.GetInstanceID());
            }
            else if (treeViewItem is ReferencesTreeItem)
            {
                ManageReferences(((AssemblyTreeItem)treeViewItem.parent).Data);
            }

            CodeEditor.CurrentEditor.OpenProject(path);
        }

        protected override void ContextClickedItem(int id)
        {
            var treeViewItem = FindItem(id, rootItem);
            if (treeViewItem is AssemblyTreeItem assemblyTreeItem)
            {
                var genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("Open Project in Editor"), false,
                    () => CodeEditor.CurrentEditor.OpenProject(AssetDatabase.GetAssetPath(assemblyTreeItem.Data.AssemblyDefinition)));
                genericMenu.ShowAsContext();
            }
            else if (treeViewItem is SourceFileTreeItem sourceFileTreeItem)
            {
                var genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("Open Project in Editor"), false,
                    () => CodeEditor.CurrentEditor.OpenProject(AssetDatabase.GetAssetPath(sourceFileTreeItem.MonoScript)));
                genericMenu.ShowAsContext();
            }
            else if (treeViewItem is ReferencesTreeItem)
            {
                var genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("Manage References..."), false,
                    () => ManageReferences(((AssemblyTreeItem)treeViewItem.parent).Data));
                genericMenu.ShowAsContext();
            }
            else if (GetSelection().All(x => FindItem(x, rootItem) is AssemblyReferenceTreeItem))
            {
                var genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("Remove reference(s)"), false, () => RemoveReference(
                    ((AssemblyTreeItem)treeViewItem.parent.parent).Data,
                    GetSelection()
                        .Select(x => FindItem(x, rootItem))
                        .Select(x => (AssemblyReferenceTreeItem)x)
                        .Select(x => x.AssemblyData)
                        .ToArray()
                ));
                genericMenu.ShowAsContext();
            }
        }

        private void ManageReferences(AssemblyData root)
        {
            var newRef = AssemblyReferenceManagerWindow.Show(root.References.Select(x => x.AssemblyDefinition).ToArray());
            if (newRef == null)
                return;

            root.UpdateReferences(newRef);
            Reload();
        }

        private void RemoveReference(AssemblyData root, AssemblyData[] references)
        {
            root.UpdateReferences(
                root.References
                    .Select(x => x.AssemblyDefinition)
                    .RemoveAll(references.Select(x => x.AssemblyDefinition).ToArray())
                    .ToArray()
            );
            Reload();
        }
    }
}