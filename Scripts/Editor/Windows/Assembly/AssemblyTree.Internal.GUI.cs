using System;
using UnityEditor;
using UnityEngine;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Types;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed partial class AssemblyTree
    {
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
                    new GUIContent(assemblyTreeItem.Data.Name, assemblyTreeItem.Data.AssemblyDefinition.name), EditorStyles.boldLabel);
            }
            else if (args.item is AssemblyReferenceTreeItem assemblyReferenceTreeItem)
            {
                GUI.DrawTexture(new Rect(indentedRect.x + 18f, indentedRect.y + 2f, 16f, 16f), _assemblyReferenceIcon);

                var secondIcon = assemblyReferenceTreeItem.AssemblyData.Type switch
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
                    new GUIContent(assemblyReferenceTreeItem.AssemblyData.Name, assemblyReferenceTreeItem.AssemblyData.AssemblyDefinition.name));
            }
            else
            {
                var closeIcon = args.item.icon;
                var openIcon = args.item is DoubleIconTreeItem doubleIconTreeItem ? doubleIconTreeItem.ExpandedIcon : closeIcon;

                GUI.DrawTexture(new Rect(indentedRect.x + 18f, indentedRect.y + 2f, 16f, 16f), IsExpanded(args.item.id) ? openIcon : closeIcon);

                GUI.Label(new Rect(indentedRect.x + 36f, indentedRect.y, indentedRect.width - 36f, indentedRect.height),
                    args.label, args.item is HeaderTreeItem ? EditorStyles.boldLabel : EditorStyles.label);
            }
        }
    }
}