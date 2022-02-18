using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows
{
    public sealed class MediaWindow : EditorWindow
    {
        [MenuItem("Window/General/Media", priority = 8)]
        public static void Show()
        {
            ((EditorWindow)CreateInstance<MediaWindow>()).Show();
        }

        private MediaData[] _assets = Array.Empty<MediaData>();
        private MediaList _mediaList;
        
        private Vector2 _scroll = Vector2.zero;
        
        private void OnEnable()
        {
            titleContent = new GUIContent("Media", EditorGUIUtility.IconContent("PreMatCube").image);
            
            OnValidate();
        }

        private void OnValidate()
        {
            RefreshAssets();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Popup(GUIContent.none, 0, new[] { "Example Scope" }, GUILayout.ExpandWidth(true));
            GUILayout.Button(EditorGUIUtility.IconContent("editicon.sml").image, CustomStyles.IconButton);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Popup(GUIContent.none, 0, new[] { "Media Type" }, GUILayout.ExpandWidth(true));
            GUILayout.Button(EditorGUIUtility.IconContent("editicon.sml").image, CustomStyles.IconButton);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(GUIContent.none, "Search Filter", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _mediaList.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }

        private void RefreshAssets()
        {
            EditorUtility.DisplayProgressBar("Refresh Asset Database", "Reload assets...", 0f);
            try
            {
                _assets = AssetDatabase.FindAssets("t:Scene")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(x => !AssetDatabase.IsValidFolder(x))
                    .Select(AssetDatabase.LoadMainAssetAtPath)
                    .Where(x => x != null)
                    .Select(x => new MediaData(x))
                    .OrderBy(x => x.Name)
                    .ToArray();
                _mediaList = new MediaList(_assets, typeof(MediaData));
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static class CustomStyles
        {
            public static readonly GUIStyle IconButton = new GUIStyle(EditorStyles.iconButton) { margin = new RectOffset(2, 2, 03, 0) };
        }
    }

    public sealed class MediaData
    {
        public Object Asset { get; }

        public string Name => Asset.name;
        
        public string Path { get; }
        public Texture Icon { get; }

        public MediaData(Object asset)
        {
            Asset = asset;

            Path = AssetDatabase.GetAssetPath(asset);
            Icon = AssetDatabase.GetCachedIcon(Path);
        }
    }
}