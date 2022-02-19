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
            var asset = (MediaData)list[i];
            GUI.DrawTexture(new Rect(rect.x, rect.y, 24f, 24f), asset.Icon);
            GUI.Label(new Rect(rect.x + 30f, rect.y, rect.width - 30f, rect.height), asset.Name, CustomStyles.Label);
        }

        private void OnSelectCallback(ReorderableList reorderableList)
        {
            if (index < 0)
                return;

            Selection.activeObject = ((MediaData)list[index]).Asset;
        }

        private static class CustomStyles
        {
            public static readonly GUIStyle Label = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleLeft };
        }
    }
}