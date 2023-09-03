using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TTools.Editor
{
    public class FavouriteManagerWindow : EditorWindow, IHasCustomMenu
    {
        private const string FAVOURITE_TOOL_SAVE_PREFIX = "EditorFavourites";
        private const string FAVOURITE_TOOL_SAVE_LIST = FAVOURITE_TOOL_SAVE_PREFIX + "/List";

        private GUIContent _contentClearAll = new GUIContent("Clear All Favourites");
        private GUIContent _contentErasePrefs = new GUIContent("Erase Saved Data");

        private List<string> _favouritePaths;
        private static bool _isCurrentSelectedPathAlreadyContained;
        private static string _currentSelectedPath;
        private static List<string> _currentFavouriteList;

        private static FavouriteManagerWindow _instance = null;
        private Vector2 _scrollPos;

        [MenuItem("Tharky/MyWindows/Favourite Manager", priority = 133)]
        public static void ShowWindow()
        {
            var window = GetWindow<FavouriteManagerWindow>("Favourite Manager");
            window.CheckForLocalPathVariable();
        }

        [MenuItem("Assets/Tharky/Add - Remove Fav", priority = 133)]
        public static void AddToFavourites()
        {
            if (_isCurrentSelectedPathAlreadyContained)
            {
                _currentFavouriteList.Remove(_currentSelectedPath);
            }
            else
            {
                _currentFavouriteList.Add(_currentSelectedPath);
            }
            SetFavouriteList(_currentFavouriteList);
            if (_instance != null)
            {
                _instance._favouritePaths = _currentFavouriteList;
            }
        }

        [MenuItem("Assets/Tharky/Add - Remove Fav", true, priority = 133)]
        public static bool ValidateMenuItem()
        {
            _currentFavouriteList = GetFavouriteList();
            _currentSelectedPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            _isCurrentSelectedPathAlreadyContained = _currentFavouriteList.Contains(_currentSelectedPath);

            return true;
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(_contentClearAll, false, () =>
            {
                _favouritePaths.Clear();
            });
            menu.AddItem(_contentErasePrefs, false, () =>
            {
                DeleteFavouritePrefs();
            });
        }

        private void OnEnable()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void OnGUI()
        {
            CheckForLocalPathVariable();

            /* Scrolling scene list and control */
            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
            string favouriteToBeDeleted = null;
            foreach (var path in _favouritePaths)
            {
                string pathName = path.Split('/').LastOrDefault() ?? "## Invalid Name ##";
                if (GUILayout.Button(pathName))
                {
                    if (Event.current.button == 0) // Left mouse click
                    {
                        Selection.activeObject = null;
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
                    }
                    else if (Event.current.button == 1) // Right mouse click
                    {
                        if (EditorUtility.DisplayDialog($"Deleting {pathName}", $"Are you sure you want to delete {pathName} from the list?", "Yes, remove it", "No, I misclicked"))
                        {
                            favouriteToBeDeleted = path;
                        }
                    }
                }
            }

            if (favouriteToBeDeleted != null)
            {
                if (_favouritePaths.Remove(favouriteToBeDeleted))
                {
                    Debug.Log($"{favouriteToBeDeleted} deleted.");
                    UpdateFavouriteList();
                }
                else
                {
                    Debug.LogError($"Logical error when deleting {favouriteToBeDeleted} as it is not in the list.");
                }
            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void CheckForLocalPathVariable()
        {
            if (_favouritePaths == null)
            {
                _favouritePaths = GetFavouriteList();
            }
        }

        private void UpdateFavouriteList()
        {
            SetFavouriteList(_favouritePaths);
            Debug.Log("Updated Favourite List");
        }

        private static List<string> GetFavouriteList()
        {
            string favouriteList = EditorPrefs.GetString(FAVOURITE_TOOL_SAVE_LIST);
            if (favouriteList != null)
            {
                var splitArray = favouriteList.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
                return splitArray.ToList();
            }
            else
            {
                return new List<string>();
            }
        }

        private static void SetFavouriteList(List<string> newList)
        {
            if (newList != null)
            {
                EditorPrefs.SetString(FAVOURITE_TOOL_SAVE_LIST, string.Join(';', newList));
            }
        }

        private void DeleteFavouritePrefs()
        {
            EditorPrefs.DeleteKey(FAVOURITE_TOOL_SAVE_LIST);
            Debug.LogWarning("Favourite prefs deleted.");
        }
    }
}