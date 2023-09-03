using System;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace TTools.Editor
{
    /// <summary>To be honest, these should be built-in already.</summary>
    public class PackageInstaller
    {
        static AddAndRemoveRequest AddAndRemoveRequest;

        [MenuItem("Tharky/Setup/Install Initial Packages", priority = 100)]
        public static void InstallPackages()
        {
            AddAndRemoveRequest = Client.AddAndRemove(new string[]
            {
                "com.unity.cinemachine",
                "com.unity.textmeshpro",
                "com.unity.inputsystem",
                "https://github.com/BennyKok/unity-hierarchy-header.git"
            }, new string[]
            {
                "com.unity.collab-proxy",
                "com.unity.visualscripting",
                "com.unity.ide.rider", //I'm poor
                "com.unity.modules.androidjni", //I do not generally make mobile games
                "com.unity.modules.cloth",
                "com.unity.modules.vehicles",
                "com.unity.modules.video",
                "com.unity.modules.vr",
                "com.unity.modules.wind",
                "com.unity.modules.xr"
            });
            EditorApplication.update += Progress;
        }

        private static void Progress()
        {
            if (AddAndRemoveRequest.IsCompleted)
            {
                if (AddAndRemoveRequest.Status == StatusCode.Success)
                {
                    Debug.Log($"Installed and removed packages");
                }
                else if (AddAndRemoveRequest.Status >= StatusCode.Failure)
                {
                    Debug.Log($"Error Code: {AddAndRemoveRequest.Error.errorCode}");
                    Debug.Log(AddAndRemoveRequest.Error.message);
                }

                EditorApplication.update -= Progress;
            }
        }
    }
}
