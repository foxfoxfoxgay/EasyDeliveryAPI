using BepInEx.Configuration;
using UnityEngine;

namespace EasyDeliveryAPI
{
    static internal class Configuration
    {
        public static ConfigEntry<bool> DragEnabled;
        public static ConfigEntry<bool> MoreEnabled;    
        static public void Awake(ConfigFile Config)
        {
            DragEnabled = Config.Bind("General.Toggles",
                                         "DragEnabled",
                                         true,
                                         "Wether or not files can be dragged.");
            MoreEnabled = Config.Bind("General.Toggles",
                                         "MoreEnabled",
                                         true,
                                         "Wether or not files can be dragged.");
            DragEnabled.SettingChanged += (sender, args) =>
            {
                foreach (DesktopDotExe Desktop in Resources.FindObjectsOfTypeAll(typeof(DesktopDotExe)))
                {
                    Desktop.drag = DragEnabled.Value;
                }
            };
            MoreEnabled.SettingChanged += (sender, args) =>
            {
                foreach (DesktopDotExe Desktop in Resources.FindObjectsOfTypeAll(typeof(DesktopDotExe)))
                {
                    foreach (DesktopDotExe.File file in Desktop.files)
                    {
                        if (file.type == DesktopDotExe.FileType.folder && file.name == "more")
                        {
                            if (!MoreEnabled.Value) {
                                foreach (DesktopDotExe.File file2 in ((DesktopDotExe.Folder)file).files)
                                {
                                    Desktop.files.Add(file2);
                                }
                                ((DesktopDotExe.Folder)file).files.Clear();
                            }
                            file.visible = MoreEnabled.Value;
                        }
                    }
                }
            };
        }
    }
}
