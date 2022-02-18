using System;
using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows
{
    internal sealed class MediaList : ReorderableList 
    {
        public MediaList(IList elements, Type elementType) : base(elements, elementType, false, false, false, false)
        {
            drawElementCallback += DrawElementCallback;
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
    }
}