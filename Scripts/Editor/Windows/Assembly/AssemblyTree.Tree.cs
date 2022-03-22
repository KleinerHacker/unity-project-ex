using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Types;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Utils;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Assembly
{
    public sealed partial class AssemblyTree
    {
        private AssemblyData[] _projectAssemblies = Array.Empty<AssemblyData>();
        private AssemblyData[] _allAssemblies = Array.Empty<AssemblyData>();
        
        protected override TreeViewItem BuildRoot()
        {
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

                var assemblyItem = new AssemblyTreeItem(counter, projectAssembly);
                BuildAssemblyItem(assemblyItem);

                root.AddChild(assemblyItem);
            }

            #region Inner Methods

            void BuildAssemblyItem(AssemblyTreeItem assemblyItem)
            {
                counter++;
                var referenceItem = new ReferencesTreeItem(counter)
                {
                    displayName = "References (" + assemblyItem.Data.References.Length + ")",
                    icon = (Texture2D)_dependencyIcon,
                    depth = 1
                };
                BuildReferencesItem(referenceItem, assemblyItem.Data);

                assemblyItem.AddChild(referenceItem);

                counter++;
                var sourcesItem = new SourcesTreeItem(counter)
                {
                    displayName = "Sources",
                    icon = (Texture2D)_scriptIcon,
                    depth = 1
                };
                BuildSourcesItem(sourcesItem, assemblyItem.Data);

                assemblyItem.AddChild(sourcesItem);

                #region Inner Methods

                void BuildReferencesItem(TreeViewItem item, AssemblyData assemblyData)
                {
                    foreach (var reference in assemblyData.References)
                    {
                        var refData = _allAssemblies.FirstOrDefault(x => reference.AssemblyDefinition == x.AssemblyDefinition);
                        if (refData == null)
                            continue; //TODO

                        counter++;
                        item.AddChild(new AssemblyReferenceTreeItem(refData, counter)
                        {
                            displayName = refData.Name,
                            icon = (Texture2D)_assemblyReferenceIcon,
                            depth = 2
                        });
                    }
                }

                void BuildSourcesItem(TreeViewItem item, AssemblyData assemblyData)
                {
                    var assemblyPath = assemblyData.FilePath;
                    var assetPaths = AssetDatabase.FindAssets("t:" + nameof(MonoScript), new[] { assemblyPath })
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .GroupBy(Path.GetDirectoryName, Path.GetFileName);

                    foreach (var asset in assetPaths)
                    {
                        counter++;
                        item.AddChild(new FolderTreeItem(counter)
                        {
                            displayName = Path.GetRelativePath(assemblyData.FilePath, asset.Key),
                            icon = (Texture2D)_folderClose,
                            ExpandedIcon = (Texture2D)_folderOpen,
                            depth = 2,
                            children = asset
                                .Select(x => (TreeViewItem)new SourceFileTreeItem(++counter, AssetDatabase.LoadAssetAtPath<MonoScript>(asset.Key + "/" + x))
                                {
                                    displayName = x,
                                    icon = (Texture2D)_csIcon,
                                    depth = 3
                                })
                                .ToList()
                        });
                    }
                }
                
                #endregion
            }

            #endregion
        }

        private void UpdateAssemblies()
        {
            _allAssemblies = AssemblyReferenceUtils.FindAllAssemblies();
            _projectAssemblies = AssemblyReferenceUtils.FindProjectAssemblies(SearchText, SearchType);
        }
    }
}