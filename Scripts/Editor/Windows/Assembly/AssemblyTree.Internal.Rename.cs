using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed partial class AssemblyTree
    {
        protected override bool CanRename(TreeViewItem item)
        {
            if (item is AssemblyTreeItem || item is SourceFileTreeItem)
            {
                if (HasUnsavedChanges)
                {
                    if (!EditorUtility.DisplayDialog("Renaming", "There are unsaved changes. If you rename this assembly, all changes will be lost! You are sure to rename now?", "Yes", "No"))
                        return false;
                }

                return true;
            }

            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (string.IsNullOrWhiteSpace(args.newName) || args.newName.Any(x => x == '\\' || x == '/' || x == ':' || x == '?'))
            {
                args.acceptedRename = false;
                return;
            }

            var treeViewItem = FindItem(args.itemID, rootItem);
            if (treeViewItem is AssemblyTreeItem assemblyTreeItem)
            {
                DiscardChanges();
                
                assemblyTreeItem.Data.Name = args.newName;
                assemblyTreeItem.Data.Store(_useGuidGetter.Invoke());
                
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(assemblyTreeItem.Data.AssemblyDefinition), args.newName);
                args.acceptedRename = true;
            }
            else if (treeViewItem is SourceFileTreeItem sourceFileTreeItem)
            {
                DiscardChanges();
                
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(sourceFileTreeItem.MonoScript), args.newName);
                args.acceptedRename = true;
            }
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            EditorGUI.indentLevel = item.depth;
            var indentedRect = EditorGUI.IndentedRect(rowRect);

            if (item is AssemblyTreeItem)
                return new Rect(indentedRect.x + 56f, indentedRect.y, indentedRect.width - 56f, indentedRect.height);

            return new Rect(indentedRect.x + 38f, indentedRect.y, indentedRect.width - 38f, indentedRect.height);
        }
    }
}