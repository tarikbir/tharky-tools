using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public List<Scene> OpenScenes = new();

        private SceneAsset _addedScene;
        private Vector2 _scrollPos;

        private const string _sceneFolder = "Assets/Scenes/";
        private const float _notificationDuration = 0.3f;

        private readonly GUIContent _contextClearScenes = new("Clear Scenes");
        private readonly GUIContent _contextClearSavedData = new("Clear Saved Data");

        private readonly GUIContent _successSceneLoaded = new("Scenes loaded.");
        private readonly GUIContent _successSceneAddedToList = new("Scene successfully added to the list.");
        private readonly GUIContent _failSceneAlreadyAddedToList = new("Scene is already in the list!");
        private readonly GUIContent _failSceneIsAlreadyOpen = new("Scene is already open!");
        private readonly GUIContent _failSceneIsOnlyOpenScene = new("You can't close the last scene!");

        [MenuItem("Tharky/Windows/Scene Manager")]
        public static void ShowWindow()
        {
            SceneManagerWindow window = GetWindow<SceneManagerWindow>("Scene Manager");
            window.minSize = new Vector2(300, 300);
        }

        private void OnGUI()
        {
            CheckForSaveFile();

            /* Scrolling scene list and control */
            EditorGUILayout.BeginVertical();
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
            SceneAsset sceneToBeDeleted = null;
            foreach (var scene in Scenes)
            {
                if (GUILayout.Button(scene.name))
                {
                    if (Event.current.button == 0) // Left mouse click on a scene
                    {
                        if (EditorSceneManager.sceneCount > 1 || !OpenScenes.Any(s => s.name == scene.name)) // When multiple scenes are open or the current scene is not already open
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                var openedScene = EditorSceneManager.OpenScene(SceneFileName(scene.name), OpenSceneMode.Single);
                                if (openedScene != null)
                                {
                                    OpenScenes.Clear();
                                    OpenScenes.Add(openedScene);
                                }
                            }
                        }
                        else
                        {
                            ShowNotification(_failSceneIsAlreadyOpen, _notificationDuration);
                        }
                    }
                    else if (Event.current.button == 1) // Right mouse click on a scene
                    {
                        if (!OpenScenes.Any(s => s.name == scene.name)) // If the selected scene is not open, open it additively
                        {
                            var openedScene = EditorSceneManager.OpenScene(SceneFileName(scene.name), OpenSceneMode.Additive);
                            if (!OpenScenes.Contains(openedScene))
                            {
                                OpenScenes.Add(openedScene);
                            }
                        }
                        else // Or if the selected scene is open, close it
                        {
                            var openedScene = OpenScenes.Where(s => s.name == scene.name).FirstOrDefault();
                            if (EditorSceneManager.sceneCount > 1)
                            {
                                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    EditorSceneManager.CloseScene(openedScene, true);
                                    OpenScenes.Remove(openedScene);
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
                            sceneToBeDeleted = scene;
                        }
                    }
                }
            }

            if (sceneToBeDeleted != null)
            {
                Scenes.Remove(sceneToBeDeleted);
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

        private void CheckForSaveFile()
        {
            if (Scenes == null || Scenes.Any(s => Resources.InstanceIDToObject(s.GetInstanceID()) == null))
            {
                if (EditorUtility.DisplayDialog("Saved scenes corrupted!",
                    "Scene list save file cannot be read. File must be deleted to continue. This action cannot be undone.",
                    "Fine, jeez",
                    "Fuck off"))
                {
                    File.Delete(SaveFileName());
                    Debug.LogWarning("Save file deleted.");
                }
                Scenes = new();
            }
        }

        protected void OnEnable()
        {
            if (LoadScenes())
            {
                ShowNotification(_successSceneLoaded, _notificationDuration);
            }

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                OpenScenes.Add(EditorSceneManager.GetSceneAt(i));
            }
        }

        protected void OnDisable()
        {
            SaveScenes();
            OpenScenes.Clear();
        }

        protected void OnDestroy()
        {
            SaveScenes();
        }

        private string SceneFileName(string name) => $"{_sceneFolder}{name}.unity";
        private string SaveFileName() => $"{Application.persistentDataPath}/editorSceneManagerData.json";

        private void SaveScenes()
        {
            var savedPaths = new SavedPathsClass();
            foreach (var scene in Scenes)
            {
                savedPaths.SavedPaths.Add(AssetDatabase.GetAssetPath(scene.GetInstanceID()));
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
                EditorJsonUtility.FromJsonOverwrite(data, savedPaths);

                if (Scenes != null) Scenes.Clear();
                else Scenes = new();

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
                        Debug.LogWarning($"Cannot load a scene as no scene found at {path}!");
                    }
                }

                return true;
            }

            Debug.Log("No load file found for Scene Manager.");
            if (Scenes != null) Scenes.Clear();
            else Scenes = new();

            return false;
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(_contextClearScenes, false, () =>
            {
                Scenes.Clear();
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
