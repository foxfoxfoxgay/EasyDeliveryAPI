using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace EasyDeliveryAPI
{
    internal class DesktopPatches : MonoBehaviour
    {
        internal static FieldInfo heldFile = typeof(DesktopDotExe).GetField("heldFile", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo clickedOn = typeof(DesktopDotExe.File).GetField("clickedOn", BindingFlags.Instance | BindingFlags.NonPublic);
        [HarmonyPatch(typeof(DesktopDotExe), nameof(DesktopDotExe.Setup))]
        class SetupPatch : HarmonyPatch
        {
            static void Prefix(DesktopDotExe __instance)
            {
                EasyAPI.DesktopLocation location = EasyAPI.DesktopLocation.Main;
                foreach (DesktopDotExe.File file in __instance.files)
                {
                    if (file.name == "more")
                    {
                        return;
                    }
                    if (file.name == "play")
                    {
                        location = EasyAPI.DesktopLocation.MainMenu;
                    }
                    if (file.name == "birds")
                    {
                        location = EasyAPI.DesktopLocation.Weird;
                    }
                    file.cantFolder = false;
                }
                if (Configuration.DragEnabled.Value) { 
                    __instance.drag = true;
                }
                DesktopDotExe.File MoreProgramsBase = new(__instance.R, __instance);
                DesktopDotExe.Folder MoreProgramsFolder = new(MoreProgramsBase);
                MoreProgramsFolder.icon = 3;
                MoreProgramsFolder.iconHover = 4;
                MoreProgramsFolder.name = "more";
                MoreProgramsFolder.position = new Vector2(6, 4);
                MoreProgramsFolder.visible = Configuration.MoreEnabled.Value;
                __instance.files.Add(MoreProgramsFolder);
                List<DesktopDotExe.File> target = location switch
                {
                    EasyAPI.DesktopLocation.Main => EasyAPI.MainGameFiles,
                    EasyAPI.DesktopLocation.MainMenu => EasyAPI.MainMenuFiles,
                    EasyAPI.DesktopLocation.Weird => EasyAPI.WeirdAreaFiles,
                    _ => throw new Exception("Invalid file location(???) this shouldnt be remotely possible.")
                };
                foreach (DesktopDotExe.File targetFile in target)
                {
                    __instance.files.Add(targetFile);
                }
                if (EasyAPI.handler.data.Positions != null && EasyAPI.handler.data.InFolder != null)
                {
                    foreach (DesktopDotExe.File file in __instance.files)
                    {
                        if (EasyAPI.handler.data.Positions.ContainsKey(file.name))
                        {
                            simplevec2 vec2 = EasyAPI.handler.data.Positions[file.name];
                            file.position = new Vector2(vec2.x, vec2.y);
                        }
                    }
                }
            }
            static void Postfix(DesktopDotExe __instance)
            {
                if (EasyAPI.handler.data.InFolder != null)
                {
                    DesktopDotExe.Folder MoreProgramsFolder = null;
                    List<DesktopDotExe.File> swap = new();
                    foreach (DesktopDotExe.File file in __instance.files)
                    {
                        if (file.name == "more")
                        {
                            MoreProgramsFolder = file as DesktopDotExe.Folder;
                        }else if ((EasyAPI.handler.data.InFolder.ContainsKey(file.name) && EasyAPI.handler.data.InFolder[file.name]) || (!EasyAPI.handler.data.InFolder.ContainsKey(file.name) && (EasyAPI.WeirdAreaFiles.Any(file2 => file == file2) || EasyAPI.MainGameFiles.Any(file2 => file == file2))))
                        {
                            swap.Add(file);
                        }
                    }
                    if (MoreProgramsFolder != null && Configuration.MoreEnabled.Value)
                    {
                        foreach (DesktopDotExe.File file in swap)
                        {
                            MoreProgramsFolder.files.Add(file);
                            __instance.files.Remove(file);
                        }
                    }
                }
                foreach (GameObject targetListener in EasyAPI.Listeners)
                {
                    GameObject temp = Instantiate(targetListener, __instance.transform);
                    temp.name = targetListener.name;
                    temp.hideFlags = HideFlags.None;
                }
                foreach (GameObject optionMenu in EasyAPI.CustomOptionsMenus.Values)
                {
                    GameObject temp = Instantiate(optionMenu, __instance.transform);
                    temp.name = "optionsmenu" + optionMenu.name;//precaution to avoid collisions
                    temp.hideFlags = HideFlags.None;
                }
                GameObject otemp1 = Instantiate(EasyAPI.OptionsMenu.Item1, __instance.transform);
                otemp1.name = EasyAPI.OptionsMenu.Item1.name;
                otemp1.hideFlags = HideFlags.None;
                GameObject otemp2 = Instantiate(EasyAPI.OptionsMenu.Item2, __instance.transform);
                otemp2.name = EasyAPI.OptionsMenu.Item2.name;
                otemp2.hideFlags = HideFlags.None;
            }
        }
        [HarmonyPatch(typeof(ScreenSystem), nameof(ScreenSystem.Init))]
        class SetupPatch2 : HarmonyPatch
        {
            static void Prefix(ScreenSystem __instance)
            {
                List<ScreenProgram> programs = __instance.programs.ToList();
                foreach (ScreenProgram targetProgram in EasyAPI.Programs)
                {
                    ScreenProgram Program2 = Instantiate(targetProgram);
                    Program2.hideFlags = HideFlags.None;
                    Program2.enabled = true;
                    programs.Add(Program2);
                }
                __instance.programs = programs.ToArray();
            }
        }
        [HarmonyPatch(typeof(DesktopDotExe), nameof(DesktopDotExe.Draw))]
        class SaveDataPatch : HarmonyPatch
        {
            static void Prefix(DesktopDotExe __instance)
            {
                if (EasyAPI.handler.data.Positions == null)
                {
                    EasyAPI.handler.data.Positions = new Dictionary<string, simplevec2> ();
                }
                if (EasyAPI.handler.data.InFolder == null)
                {
                    EasyAPI.handler.data.InFolder = new Dictionary<string, bool>();
                }
                DesktopDotExe.Folder targetFolder = null;
                foreach (DesktopDotExe.File file in __instance.files)
                {
                    if (file.name == "more")
                    {
                        targetFolder = file as DesktopDotExe.Folder;
                    }
                    if (file.name != "play")
                    {
                        EasyAPI.handler.data.Positions[file.name] = new simplevec2(file.position.x, file.position.y);
                        EasyAPI.handler.data.InFolder[file.name] = false;
                    }
                }
                if (targetFolder != null)
                {
                    foreach (DesktopDotExe.File file in targetFolder.files)
                    {
                        EasyAPI.handler.data.Positions.Remove(file.name);
                        EasyAPI.handler.data.InFolder[file.name] = true;
                    }
                }   
            }
            static void Postfix()
            {
                for (int i = EasyAPI.OldWindowViewers.Count - 1; i > 0; i--)
                {
                    DesktopDotExe.WindowView winview = EasyAPI.OldWindowViewers[i];
                    bool flag2;
                    winview.Draw(out flag2);
                    if (flag2)
                    {
                        EasyAPI.OldWindowViewers.Remove(winview);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(DesktopDotExe.File), nameof(DesktopDotExe.File.Execute))]
        class ExecutePatch : HarmonyPatch
        {
            static bool Prefix(DesktopDotExe.File __instance, out DesktopDotExe.WindowView view)
            {
                view = null;
                __instance.notification = false;
                if (__instance.type == DesktopDotExe.FileType.exe)
                {
                    if (__instance.data.Contains("listener_"))
                    {
                        view = new DesktopDotExe.WindowView(__instance.M, __instance);
                    }
                    else if (__instance.data.Contains("modded_"))
                    {
                        for (int i = 0; i < __instance.M.screenSystem.programs.Length; i++)
                        {
                            ScreenProgram program = __instance.M.screenSystem.programs[i];
                            if (program.title == __instance.data.Substring(7))
                            {
                                __instance.M.screenSystem.SetMenu(i);
                                __instance.M.screenSystem.OpenMenu();
                            }
                        }
                    }
                    else
                    {
                        __instance.M.screenSystem.SetMenu(int.Parse(__instance.data));
                        __instance.M.screenSystem.OpenMenu();
                    }
                    return (false);
                }
                return true;
            }
        }

        private static MethodInfo LockInputMethod = typeof(sInputManager).GetMethod("LockInput", BindingFlags.NonPublic | BindingFlags.Instance);

        [HarmonyPatch]
        class LockInput : HarmonyPatch
        {
            static MethodInfo TargetMethod()
            {
                return(typeof(sInputManager).GetMethod("GetInput", BindingFlags.NonPublic | BindingFlags.Instance));
            }
            static void Postfix(sInputManager __instance)
            {
                if (EventSystem.current && EventSystem.current.currentSelectedGameObject && EventSystem.current.currentSelectedGameObject.GetComponent<InputField>() != null)
                {
                    
                    LockInputMethod.Invoke(__instance, []);
                    __instance.pausePressed = false;
                }
            }
        }
        [HarmonyPatch]
        class InputFieldFix1 : HarmonyPatch
        {
            static MethodInfo TargetMethod()
            {
                return (typeof(InputField).GetMethod("IsValidChar", BindingFlags.NonPublic | BindingFlags.Instance));
            }
            static bool Prefix(InputField __instance, char c, ref bool __result)
            {
                if (!__instance.textComponent)
                {
                    __result = c != '\0' && c != '\u007f';
                    return false;
                }
                return true;
            }
        }
    }
}
