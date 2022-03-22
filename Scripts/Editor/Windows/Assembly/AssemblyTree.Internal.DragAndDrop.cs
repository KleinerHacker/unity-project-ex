using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed partial class AssemblyTree
    {
        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            if (args.parentItem is ReferencesTreeItem && args.dragAndDropPosition == DragAndDropPosition.UponItem &&
                DragAndDrop.objectReferences.All(x => x is AssemblyDefinitionAsset))
            {
                var assemblyTreeItem = args.parentItem.parent as AssemblyTreeItem;
                if (assemblyTreeItem == null)
                    return DragAndDropVisualMode.None;

                if (!args.performDrop)
                    return DragAndDropVisualMode.Link;

                assemblyTreeItem.Data.UpdateReferences(
                    assemblyTreeItem.Data.References
                        .Select(x => x.AssemblyDefinition)
                        .Concat(
                            DragAndDrop.objectReferences
                                .Select(x => (AssemblyDefinitionAsset)x)
                                .ToArray()
                        )
                        .ToArray()
                );

                Reload();
                return DragAndDropVisualMode.Link;
            }

            return DragAndDropVisualMode.None;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            Debug.LogError("*** " + (args.draggedItem is AssemblyTreeItem || args.draggedItemIDs.All(x => FindItem(x, rootItem) is AssemblyTreeItem)));
            return args.draggedItem is AssemblyTreeItem || args.draggedItemIDs.All(x => FindItem(x, rootItem) is AssemblyTreeItem);
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            Debug.LogError("+++");
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.objectReferences = args.draggedItemIDs
                .Select(x => FindItem(x, rootItem))
                .Select(x => (AssemblyTreeItem)x)
                .Select(x => x.Data.AssemblyDefinition)
                .ToArray();
            DragAndDrop.StartDrag("Assembly Definition Drag");
        }
    }
}