using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace TTools.Editor
{
    public class SceneManagerWindow : EditorWindow, IHasCustomMenu
    {
        public List<SceneAsset> Scenes = new();
        private List<SceneAsset> _scenesToBeDeleted = new();

        private SceneAsset _addedScene;
        private Vector2 _scrollPos;

        private const float _notificationDuration = 0.3f;

        private readonly GUIContent _contextClearScenes = new("Clear Scenes");
        private readonly GUIContent _contextClearSavedData = new("Clear Saved Data");

        private readonly GUIContent _successSceneLoaded = new("Scenes loaded.");
        private readonly GUIContent _successSceneAddedToList = new("Scene successfully added to the list.");
        private readonly GUIContent _failSceneAlreadyAddedToList = new("Scene is already in the list!");
        private readonly GUIContent _failSceneIsAlreadyOpen = new("Scene is already open!");
        private readonly GUIContent _failSceneIsOnlyOpenScene = new("You can't close the last scene!");

        [MenuItem("Tharky/Tools/Scene Manager", priority = 133)]
        public static void ShowWindow()
        {
            SceneManagerWindow window = GetWindow<SceneManagerWindow>("Scene Manager");
            window.minSize = new Vector2(300, 300);
        }

        private void OnGUI()
        {
            ValidateScenesList();
            /* Scrolling scene list and control */
            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
            foreach (var scene in Scenes)
            {
                if (GUILayout.Button(scene.name))
                {
                    if (Event.current.button == 0) // Left mouse click on a scene
                    {
                        var openedScene = GetSceneIfAssetIsLoaded(scene);
                        if (EditorSceneManager.sceneCount > 1 || !openedScene.IsValid()) // When multiple scenes are open or the current scene is not already open
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                EditorSceneManager.OpenScene(GetScenePath(scene), OpenSceneMode.Single);
                            }
                        }
                        else
                        {
                            ShowNotification(_failSceneIsAlreadyOpen, _notificationDuration);
                        }
                    }
                    else if (Event.current.button == 1) // Right mouse click on a scene
                    {
                        var openedScene = GetSceneIfAssetIsLoaded(scene);
                        if (!openedScene.IsValid()) // If the selected scene is not open, open it additively
                        {
                            EditorSceneManager.OpenScene(GetScenePath(scene), OpenSceneMode.Additive);
                        }
                        else // Or if the selected scene is open, close it
                        {
                            if (EditorSceneManager.sceneCount > 1)
                            {
                                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    EditorSceneManager.CloseScene(openedScene, true);
                                }
                            }
                            else
                            {
                                ShowNotification(_failSceneIsOnlyOpenScene, _notificationDuration);
                            }
                        }
                    }
                    else if (Event.current.button == 2) // Middle mouse click on a scene
                    {
                        if (EditorUtility.DisplayDialog($"Deleting {scene.name}", $"Are you sure you want to delete {scene.name} from the list?", "Yeah baby", "Nope"))
                        {
                            _scenesToBeDeleted.Add(scene);
                        }
                    }
                }
            }

            GUILayout.FlexibleSpace();

            /* New scene adding section */
            _addedScene = (SceneAsset)EditorGUILayout.ObjectField("Add scene to be added", _addedScene, typeof(SceneAsset), true);

            if (_addedScene != null)
            {
                if (!Scenes.Contains(_addedScene))
                {
                    Scenes.Add(_addedScene);
                    ShowNotification(_successSceneAddedToList, _notificationDuration);
                }
                else
                {
                    ShowNotification(_failSceneAlreadyAddedToList, _notificationDuration);
                }

                _addedScene = null;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void ValidateScenesList()
        {
            if (Scenes == null)
            {
                Scenes = new();
                return;
            }

            foreach (SceneAsset scene in Scenes)
            {
                if (scene == null)
                {
                    _scenesToBeDeleted.Add(scene);
                }
            }

            foreach (SceneAsset scene in _scenesToBeDeleted)
            {
                Scenes.Remove(scene);
            }
            _scenesToBeDeleted.Clear();
        }

        protected void OnEnable()
        {
            if (LoadScenes())
            {
                ShowNotification(_successSceneLoaded, _notificationDuration);
            }
        }

        protected void OnDisable()
        {
            SaveScenes();
        }

        protected void OnDestroy()
        {
            SaveScenes();
        }

        private string SaveFileName() => $"{Application.persistentDataPath}/editorSceneManagerData.json";
        private string GetScenePath(SceneAsset scene) => AssetDatabase.GetAssetPath(scene.GetInstanceID());
        private Scene GetSceneIfAssetIsLoaded(SceneAsset scene)
        {
            return EditorSceneManager.GetSceneByPath(GetScenePath(scene));
        }
        private void InitializeOrClearScenes()
        {
            if (Scenes != null) Scenes.Clear();
            else Scenes = new();
        }

        private void SaveScenes()
        {
            var savedPaths = new SavedPathsClass();
            foreach (var scene in Scenes)
            {
                savedPaths.SavedPaths.Add(GetScenePath(scene));
            }

            var data = EditorJsonUtility.ToJson(savedPaths);
            File.WriteAllText(SaveFileName(), data);
        }

        private bool LoadScenes()
        {
            string saveFileName = SaveFileName();
            if (File.Exists(saveFileName))
            {
                SavedPathsClass savedPaths = new();
                string data = File.ReadAllText(saveFileName);
                try
                {
                    EditorJsonUtility.FromJsonOverwrite(data, savedPaths);
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Saved scenes corrupted!",
                            "Scene list save file cannot be read. File must be deleted to continue. This action cannot be undone.\nReason: " +
                            e.Message,
                            "Ok");
                    File.Delete(SaveFileName());
                    Debug.LogWarning("Save file deleted.");
                }

                InitializeOrClearScenes();

                if (savedPaths == null || savedPaths.SavedPaths.Count == 0)
                {
                    return false;
                }

                foreach (var path in savedPaths.SavedPaths)
                {
                    if (File.Exists(path))
                    {
                        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);

                        if (scene != null)
                        {
                            Scenes.Add(scene);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Cannot load a scene as no scene found at {path}! Saved scenes might be corrupted. It will fix itself automatically.");
                    }
                }

                return true;
            }

            Debug.Log("No load file found for Scene Manager.");
            InitializeOrClearScenes();
            return false;
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(_contextClearScenes, false, () =>
            {
                InitializeOrClearScenes();
                _addedScene = null;
            });
            menu.AddItem(_contextClearSavedData, false, () =>
            {
                File.Delete(SaveFileName());
            });
        }

        [Serializable]
        private class SavedPathsClass //High level languages are weird, a simple list doesn't serialize while a class does...
        {
            public List<string> SavedPaths = new();
        }
    }
}
