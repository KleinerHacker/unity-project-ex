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

        private MediaFilter[] _mediaFilters = Array.Empty<MediaFilter>();
        private MediaScope[] _mediaScopes = Array.Empty<MediaScope>();

        private Vector2 _scroll = Vector2.zero;
        private int _mediaScopeIndex;
        private int _mediaSubScopeIndex;
        private int _mediaFilterIndex;
        private int _mediaSubFilterIndex;
        private string _mediaSearchFilter = "";

        private void OnEnable()
        {
            titleContent = new GUIContent("Media", EditorGUIUtility.IconContent("PreMatCube").image);

            OnValidate();
        }

        private void OnValidate()
        {
            RefreshMediaScopeFilters();
            RefreshMediaTypeFilters();
            RefreshAssets();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scopes:", EditorStyles.boldLabel, GUILayout.Width(50f));
            var newMediaScopeIndex = EditorGUILayout.Popup(GUIContent.none, _mediaScopeIndex,
                new[] { "<All>" }.Concat(_mediaScopes.Select(x => x.Name).ToArray()).ToArray(),
                GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
            if (newMediaScopeIndex != _mediaScopeIndex)
            {
                _mediaScopeIndex = newMediaScopeIndex;
                _mediaSubScopeIndex = 0;
                RefreshAssets();
            }

            if (_mediaScopeIndex > 0 && _mediaScopes[_mediaScopeIndex - 1].SubScopes.Length > 1)
            {
                var newMediaSubScopeIndex = EditorGUILayout.Popup(GUIContent.none, _mediaSubScopeIndex,
                    new[] { "<All>" }.Concat(_mediaScopes[_mediaScopeIndex - 1].SubScopes.Select(x => x.Name).ToArray()).ToArray(),
                    GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
                if (newMediaSubScopeIndex != _mediaSubScopeIndex)
                {
                    _mediaSubScopeIndex = newMediaSubScopeIndex;
                    RefreshAssets();
                }
            }

            GUILayout.Button(EditorGUIUtility.IconContent("editicon.sml").image, CustomStyles.IconButton);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", EditorStyles.boldLabel, GUILayout.Width(50f));
            var newMediaTypeFilterIndex = EditorGUILayout.Popup(GUIContent.none, _mediaFilterIndex,
                new[] { "<All>" }.Concat(_mediaFilters.Select(x => x.Name).ToArray()).ToArray(),
                GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
            if (newMediaTypeFilterIndex != _mediaFilterIndex)
            {
                _mediaFilterIndex = newMediaTypeFilterIndex;
                _mediaSubFilterIndex = 0;
                RefreshAssets();
            }

            if (_mediaFilterIndex > 0 && _mediaFilters[_mediaFilterIndex - 1].Filters.Length > 1)
            {
                var newMediaTypeSubFilterIndex = EditorGUILayout.Popup(GUIContent.none, _mediaSubFilterIndex,
                    new[] { "<All>" }.Concat(_mediaFilters[_mediaFilterIndex - 1].Filters.Select(x => x.Name).ToArray()).ToArray(),
                    GUILayout.ExpandWidth(true), GUILayout.MinWidth(100f));
                if (newMediaTypeSubFilterIndex != _mediaSubFilterIndex)
                {
                    _mediaSubFilterIndex = newMediaTypeSubFilterIndex;
                    RefreshAssets();
                }
            }

            GUILayout.Button(EditorGUIUtility.IconContent("editicon.sml").image, CustomStyles.IconButton);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var newMediaSearchFilter = EditorGUILayout.TextField(GUIContent.none, _mediaSearchFilter, GUILayout.ExpandWidth(true));
            if (newMediaSearchFilter.GetHashCode() != _mediaSearchFilter.GetHashCode())
            {
                _mediaSearchFilter = newMediaSearchFilter;
                RefreshAssets();
            }

            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            _mediaList.DoLayoutList();
            EditorGUILayout.EndScrollView();
        }

        private void RefreshAssets()
        {
            var filterLine = "";
            if (_mediaFilterIndex > 0)
            {
                if (_mediaSubFilterIndex > 0)
                {
                    filterLine = _mediaFilters[_mediaFilterIndex - 1].Filters[_mediaSubFilterIndex - 1].FilterLine;
                }
                else
                {
                    filterLine = _mediaFilters[_mediaFilterIndex - 1].FilterLine;
                }
            }

            var paths = Array.Empty<string>();
            if (_mediaScopeIndex > 0)
            {
                if (_mediaSubScopeIndex > 0)
                {
                    paths = new[] { _mediaScopes[_mediaScopeIndex - 1].SubScopes[_mediaSubScopeIndex - 1].Path };
                }
                else
                {
                    paths = _mediaScopes[_mediaScopeIndex - 1].Paths;   
                }
            }

            _assets = AssetDatabase.FindAssets(filterLine, paths)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(x => !AssetDatabase.IsValidFolder(x))
                .Select(AssetDatabase.LoadMainAssetAtPath)
                .Where(x => x != null)
                .Select(x => new MediaData(x))
                .Where(x => string.IsNullOrWhiteSpace(_mediaSearchFilter) || x.Name.Contains(_mediaSearchFilter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.Name)
                .ToArray();
            _mediaList = new MediaList(_assets, typeof(MediaData));

            var scopeName = _mediaScopeIndex > 0 ? _mediaScopes[_mediaScopeIndex - 1].Name : "<All>";
            var filterName = _mediaFilterIndex > 0 ? _mediaFilters[_mediaFilterIndex].Name : "<All>";
            titleContent = new GUIContent("Media - " + scopeName + " / " + filterName);
        }

        private void RefreshMediaTypeFilters()
        {
            _mediaFilters = MediaFilter.BuiltinFilters;
        }

        private void RefreshMediaScopeFilters()
        {
            _mediaScopes = MediaScope.BuiltinScopes;
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