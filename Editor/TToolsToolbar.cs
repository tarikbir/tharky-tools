using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;

namespace TTools.Editor
{
    [Overlay(typeof(SceneView), "TTools")]
    [Icon(_icon)]
    public class TToolsToolbar : ToolbarOverlay
    {
        public const string _icon = "Packages/com.tharky.tools/Textures/logo.png";

        public TToolsToolbar() : base(TToolsDropdown._id) { }

        [EditorToolbarElement(_id, typeof(SceneView))]
        class TToolsDropdown : EditorToolbarDropdown, IAccessContainerWindow
        {
            public const string _id = "TToolsToolbar/TToolsDropdown";

            public EditorWindow containerWindow { get; set; }

            public TToolsDropdown()
            {
                text = "TTools";
                tooltip = "Tools from Tharky";
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(TToolsToolbar._icon);

                clicked += ShowToolsMenu;
            }

            private void ShowToolsMenu()
            {
                GenericMenu menu = new();

                menu.AddItem(new GUIContent("Setup/Install Initial Packages"), false, () => PackageInstaller.InstallPackages());
                menu.AddItem(new GUIContent("MyWindows/Favourite Manager"), false, () => FavouriteManagerWindow.ShowWindow());
                menu.AddItem(new GUIContent("MyWindows/Scene Manager"), false, () => SceneManagerWindow.ShowWindow());

                menu.ShowAsContext();
            }
        }
    }
    public class TToolsMenu : MonoBehaviour
    {

    }
}