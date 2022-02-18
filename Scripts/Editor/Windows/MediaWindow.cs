using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityProjectEx.Editor.project_ex.Scripts.Editor.Windows.Media;
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

        private MediaTypeFilter[] _mediaTypeFilters = Array.Empty<MediaTypeFilter>();

        private Vector2 _scroll = Vector2.zero;
        private int _mediaTypeFilterIndex;
        private int _mediaTypeSubFilterIndex;

        private void OnEnable()
        {
            titleContent = new GUIContent("Media", EditorGUIUtility.IconContent("PreMatCube").image);

            OnValidate();
        }

        private void OnValidate()
        {
            RefreshMediaTypeFilters();
            RefreshAssets();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Popup(GUIContent.none, 0, new[] { "Example Scope" }, GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
            GUILayout.Button(EditorGUIUtility.IconContent("editicon.sml").image, CustomStyles.IconButton);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var newMediaTypeFilterIndex = EditorGUILayout.Popup(GUIContent.none, _mediaTypeFilterIndex,
                new[] { "<All>" }.Concat(_mediaTypeFilters.Select(x => x.Name).ToArray()).ToArray(),
                GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
            if (newMediaTypeFilterIndex != _mediaTypeFilterIndex)
            {
                _mediaTypeFilterIndex = newMediaTypeFilterIndex;
                _mediaTypeSubFilterIndex = 0;
                RefreshAssets();
            }

            if (_mediaTypeFilterIndex > 0 && _mediaTypeFilters[_mediaTypeFilterIndex - 1].Filters.Length > 1)
            {
                var newMediaTypeSubFilterIndex = EditorGUILayout.Popup(GUIContent.none, _mediaTypeSubFilterIndex,
                    new [] {"<All>"}.Concat(_mediaTypeFilters[_mediaTypeFilterIndex - 1].Filters.Select(x => x.Name).ToArray()).ToArray(),
                    GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
                if (newMediaTypeSubFilterIndex != _mediaTypeSubFilterIndex)
                {
                    _mediaTypeSubFilterIndex = newMediaTypeSubFilterIndex;
                    RefreshAssets();
                }
            }

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
                var filterLine = "";
                if (_mediaTypeFilterIndex > 0)
                {
                    if (_mediaTypeSubFilterIndex > 0)
                    {
                        filterLine = _mediaTypeFilters[_mediaTypeFilterIndex - 1].Filters[_mediaTypeSubFilterIndex - 1].FilterLine;
                    }
                    else
                    {
                        filterLine = _mediaTypeFilters[_mediaTypeFilterIndex - 1].FilterLine;
                    }
                }
                
                _assets = AssetDatabase.FindAssets(filterLine)
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

        private void RefreshMediaTypeFilters()
        {
            _mediaTypeFilters = MediaTypeFilter.BuiltinFilters;
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