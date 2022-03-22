using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Types;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed partial class AssemblyTree
    {
        private sealed class AssemblyTreeItem : TreeViewItem
        {
            public AssemblyData Data { get; }

            public AssemblyTreeItem(int id, AssemblyData data) : base(id)
            {
                Data = data;
                displayName = data.Name;
            }
        }

        private abstract class HeaderTreeItem : TreeViewItem
        {
            protected HeaderTreeItem(int id) : base(id)
            {
            }
        }

        private sealed class ReferencesTreeItem : HeaderTreeItem
        {
            public ReferencesTreeItem(int id) : base(id)
            {
            }
        }

        private sealed class AssemblyReferenceTreeItem : TreeViewItem
        {
            public AssemblyData AssemblyData { get; }

            public AssemblyReferenceTreeItem(AssemblyData assemblyData, int id) : base(id)
            {
                AssemblyData = assemblyData;
            }
        }

        private sealed class SourcesTreeItem : HeaderTreeItem
        {
            public SourcesTreeItem(int id) : base(id)
            {
            }
        }

        private abstract class DoubleIconTreeItem : TreeViewItem
        {
            public Texture2D ExpandedIcon { get; set; }

            protected DoubleIconTreeItem(int id) : base(id)
            {
            }
        }

        private sealed class FolderTreeItem : DoubleIconTreeItem
        {
            public FolderTreeItem(int id) : base(id)
            {
            }
        }

        private sealed class SourceFileTreeItem : TreeViewItem
        {
            public MonoScript MonoScript { get; }

            public SourceFileTreeItem(int id, MonoScript monoScript) : base(id)
            {
                MonoScript = monoScript;
            }
        }
    }
}