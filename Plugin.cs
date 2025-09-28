using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace EasyDeliveryAPI
{

    internal struct simplevec2
    {
        public float x;
        public float y;
        public simplevec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
    internal struct filepositions
    {
        public Dictionary<string, simplevec2> Positions;
        public Dictionary<string, bool> InFolder;
    };
    
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class EasyAPI : BaseUnityPlugin
    {
        internal static List<string> SaveFileNames = new();
        internal static Dictionary<string, string> ModdedSaveHandlers = new();
        internal static Action OnSave;
        internal static Action<Dictionary<string, string>> OnLoad;
        internal static Dictionary<string, bool> ModsLoaded = new();
        internal static List<DesktopDotExe.File> WeirdAreaFiles = new();
        internal static List<DesktopDotExe.File> MainGameFiles = new();
        internal static List<DesktopDotExe.File> MainMenuFiles = new();
        internal static List<ScreenProgram> Programs = new();
        internal static List<GameObject> Listeners = new();
        internal static List<GameObject> sHUDListeners = new();
        internal static (GameObject, GameObject) OptionsMenu;
        internal static Dictionary<string, GameObject> CustomOptionsMenus = new();
        internal static Dictionary<string, ConfigFile> ConfigList = new();
        internal static ModdedSaveSystem<filepositions> handler = new("EasyAPI");
        internal static ManualLogSource Log;
        private static MiniRenderer annoyingrenderer = new();
        private static DesktopDotExe annoyingdesktop = new();
        internal static InputField RenameSaveFileInput;
        internal static FieldInfo windowViewer = typeof(DesktopDotExe).GetField("windowViewer", BindingFlags.NonPublic | BindingFlags.Instance);
        internal static List<DesktopDotExe.WindowView> OldWindowViewers = new();

        [Flags]
        public enum DesktopLocation
        {
            Main = 0,
            MainMenu = 1,
            Weird = 2,
        }
        public static DesktopDotExe.WindowView MakeWindow(DesktopDotExe __instance, string name, string target)
        {
            DesktopDotExe.File f = InstantiateFile();
            f.data = "listener_" + target;
            f.name = name;
            f.openSound = __instance.select;
            return (new DesktopDotExe.WindowView(__instance, f));
        }

        public static bool ChangeWindow(DesktopDotExe __instance, string name, string target)
        {
            DesktopDotExe.WindowView targetwindow = (windowViewer.GetValue(__instance) as DesktopDotExe.WindowView);
            if (targetwindow != null)
            {
                targetwindow.Kill();
                
                windowViewer.SetValue(__instance, MakeWindow(__instance, name, target));
                OldWindowViewers.Add(targetwindow);
                return true;
            }
            return false;
        }
        public static void FetchSaveNames()
        {
            List<string> FileNames = new();
            foreach (string file in Directory.GetFiles(Application.persistentDataPath))
            {
                FileInfo CurrentFile = new FileInfo(@file);
                if (CurrentFile.Extension == ".txt" && CurrentFile.Name.Length >=22 && CurrentFile.Name.Substring(0, 22) == "EasyDeliveryCoSaveData")
                {
                    FileNames.Add(CurrentFile.Name.Substring(0, CurrentFile.Name.Length - 4));
                }
            }
            FileNames.Sort();
            SaveFileNames = FileNames;
        }
        public static void AddProgram(ScreenProgram Program)
        {
            Programs.Add(Program);
        }
        public static T AddListener<T>(string Name) where T : UnityEngine.Component
        {
            GameObject Target = new GameObject(Name);
            T component = Target.AddComponent<T>();
            Target.hideFlags = HideFlags.HideAndDontSave;
            Listeners.Add(Target);
            return component;
        }
        public static T AddCustomOptionsMenu<T>(string Name) where T : UnityEngine.Component
        {
            GameObject Target = new GameObject(Name);
            T component = Target.AddComponent<T>();
            Target.hideFlags = HideFlags.HideAndDontSave;
            CustomOptionsMenus.Add(Name, Target);
            return component;
        }
        public static T AddsHUDListener<T>(string Name) where T : UnityEngine.Component
        {
            GameObject Target = new GameObject(Name);
            T component = Target.AddComponent<T>();
            Target.hideFlags = HideFlags.HideAndDontSave;
            sHUDListeners.Add(Target);
            return component;
        }
        public static void AddConfig(string Name, ConfigFile target)
        {
            ConfigList.Add(Name, target);
        }
        public static UnityEngine.UI.InputField MakeInputField(GameObject Target)
        {
            return Target.AddComponent<UnityEngine.UI.InputField>();
        }
        public static DesktopDotExe.File InstantiateFile()
        {
            return (new DesktopDotExe.File(annoyingrenderer, annoyingdesktop));
        }
        public static void AddFile(DesktopLocation Location, DesktopDotExe.File File)
        {
            if ((Location & DesktopLocation.Main) == DesktopLocation.Main)
            {
                MainGameFiles.Add(File);
            }
            if ((Location & DesktopLocation.MainMenu) == DesktopLocation.MainMenu)
            {
                MainMenuFiles.Add(File);
            }
            if ((Location & DesktopLocation.Weird) == DesktopLocation.Weird)
            {
                WeirdAreaFiles.Add(File);
            }
        }
        static private MethodInfo UpdateFileName = (typeof(sSaveSystem).GetMethod("UpdateFileName", BindingFlags.NonPublic | BindingFlags.Instance));
        public static void AddCustomSettingsDrawHandler(Func<ConfigEntryBase, Func<UIUtil, float, float, ConfigEntryBase, string, string>> Target)
        {
            ModSettingsPage.CustomDrawHandler.Add(Target);
        }

        private void Awake()
        {
            GameObject temp = new GameObject();
            temp.hideFlags = HideFlags.HideAndDontSave;
            RenameSaveFileInput = MakeInputField(temp);
            RenameSaveFileInput.characterLimit = 12;
            RenameSaveFileInput.onSubmit.AddListener((String submittedname) => {
                string FilePathT = Application.persistentDataPath + "/" + "EasyDeliveryCoSaveData_" + submittedname + ".txt";
                if (!File.Exists(FilePathT))
                {
                    string FilePathT2 = Application.persistentDataPath + "/" + "modded-" + "EasyDeliveryCoSaveData_" + submittedname + ".json";

                    string FilePath = Application.persistentDataPath + "/" + sSaveSystem.instance.saveFileName + ".txt";
                    string FilePath2 = Application.persistentDataPath + "/" + "modded-" + sSaveSystem.instance.saveFileName + ".json";
                    if (File.Exists(FilePath))
                    {
                        File.Copy(FilePath, FilePathT);
                        if (File.Exists(FilePath2))
                        {
                            File.Copy(FilePath2, FilePathT2);
                            File.Delete(FilePath2);
                        }
                        File.Delete(FilePath);
                    }
                    sSaveSystem.instance.saveFileName = "EasyDeliveryCoSaveData_" + submittedname;
                    UpdateFileName.Invoke(sSaveSystem.instance, []);
                    FetchSaveNames();
                }
                else
                {
                    EasyAPI.RenameSaveFileInput.text = sSaveSystem.instance.saveFileName.Substring(23).Substring(0, Math.Min(sSaveSystem.instance.saveFileName.Length - 23, 12));
                }
            });
            Log = Logger;
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("EasyDesktopAPI");
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            GameObject Target1 = new GameObject("modsettingslist");
            Target1.AddComponent<ModSettingsMenu>();
            Target1.hideFlags = HideFlags.HideAndDontSave;
            GameObject Target2 = new GameObject("modsettingssingle");
            Target2.AddComponent<ModSettingsPage>();
            Target2.hideFlags = HideFlags.HideAndDontSave;
            OptionsMenu = (Target1, Target2);
            Configuration.Awake(Config);
            AddConfig("easy delivery api", Config);
            AddCustomSettingsDrawHandler((ConfigEntryBase Base) => { Log.LogInfo(Base.Description.Tags.Length); if (Base.Description.Tags.Length > 0) { Log.LogInfo("HIT!"); return (UIUtil util, float xpos, float ypos, ConfigEntryBase entry, string Name) => { util.R.put(Name, xpos - Name.Length * 4, ypos); return null; }; } return null; });
        }
    }
}
