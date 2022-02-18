using System;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Media
{
    internal sealed class MediaList : ReorderableList 
    {
        public MediaList(IList elements, Type elementType) : base(elements, elementType, false, false, false, false)
        {
            drawElementCallback += DrawElementCallback;
            onSelectCallback += OnSelectCallback;
        }

        private void DrawElementCallback(Rect rect, int i, bool isactive, bool isfocused)
        {
            var asset = (MediaData) list[i];
            if (asset == null)
            {
                GUI.Label(rect, "<null>");
                return;
            }
            
            GUI.Label(rect, new GUIContent(asset.Name, asset.Icon));
        }

        private void OnSelectCallback(ReorderableList reorderableList)
        {
            if (index < 0)
                return;

            Selection.activeObject = ((MediaData)list[index]).Asset;
        }
    }
}