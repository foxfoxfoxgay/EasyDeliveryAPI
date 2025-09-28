using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
namespace EasyDeliveryAPI
{
    internal class SavePatches : MonoBehaviour
    {
        static private FieldInfo nav = typeof(IntroDotExe).GetField("nav", BindingFlags.NonPublic | BindingFlags.Instance);
        static private MethodInfo FileButton = (typeof(IntroDotExe).GetMethod("FileButton", BindingFlags.NonPublic | BindingFlags.Instance));
        static private MethodInfo GoBack = (typeof(IntroDotExe).GetMethod("GoBack", BindingFlags.NonPublic | BindingFlags.Instance));
        static private FieldInfo selectedSaveFile = typeof(IntroDotExe).GetField("selectedSaveFile", BindingFlags.NonPublic | BindingFlags.Instance);
        static private MethodInfo ToNextScreen = (typeof(IntroDotExe).GetMethod("ToNextScreen", BindingFlags.NonPublic | BindingFlags.Instance));
        static private MethodInfo DeleteSaveAndGoBack = (typeof(IntroDotExe).GetMethod("DeleteSaveAndGoBack", BindingFlags.NonPublic | BindingFlags.Instance));
        static private FieldInfo deleteConfirmation = typeof(IntroDotExe).GetField("deleteConfirmation", BindingFlags.NonPublic | BindingFlags.Instance);
        static private MethodInfo stopDeleteConfirmation = (typeof(IntroDotExe).GetMethod("stopDeleteConfirmation", BindingFlags.NonPublic | BindingFlags.Instance));
        static private MethodInfo startDeleteConfirmation = (typeof(IntroDotExe).GetMethod("startDeleteConfirmation", BindingFlags.NonPublic | BindingFlags.Instance));
        static private MethodInfo NewGame = (typeof(IntroDotExe).GetMethod("NewGame", BindingFlags.NonPublic | BindingFlags.Instance));
        
        static private int PageNum = 0;
        static Transform Parent;
        [HarmonyPatch(typeof(IntroDotExe), nameof(IntroDotExe.Awake))]
        class AwakePatch : HarmonyPatch
        {
            static void Prefix(IntroDotExe __instance)
            {
                PageNum = 0;
                Parent = __instance.saveFiles[0].transform.parent;
                foreach (GameObject go in __instance.saveFiles)
                {
                    Destroy(go);
                }
                __instance.saveFiles = [];
                EasyAPI.FetchSaveNames();
            }
        }
        [HarmonyPatch]
        class SelectorPatch : HarmonyPatch
        {
            static MethodBase TargetMethod()
            {
                return (typeof(IntroDotExe).GetMethod("FileSelectScreen", BindingFlags.NonPublic | BindingFlags.Instance));
            }
            static bool Prefix(IntroDotExe __instance)
            {

                GamepadNavigation gnav = nav.GetValue(__instance) as GamepadNavigation;
                UIUtil Util = new(__instance.R, __instance, gnav);
                Vector2 vector = new Vector2((float)(__instance.R.width / 2 - 64), (float)(__instance.R.height / 2 - 32));
                float num = 16f;

                for (int i = 0; i < Math.Min(4, (EasyAPI.SaveFileNames.Count) - PageNum * 4); i++)
                {
                    FileButton.Invoke(__instance, [vector.x, vector.y + (num + 8f) * (float)(i % 4), 128f, num, i + (PageNum * 4)]);
                }
                __instance.R.put("Page - " + PageNum, vector.x + 32f, vector.y - 12f);
                if (Util.Button("<", vector.x, vector.y - 12f, true) && PageNum > 0)
                {
                    PageNum--;
                }
                if (Util.Button(">", vector.x + 122f, vector.y - 12f, true) && EasyAPI.SaveFileNames.Count > ((PageNum + 1) * 4))
                {
                    PageNum++;
                };
                if (Util.Button("back", vector.x, vector.y + 90))
                {
                    GoBack.Invoke(__instance, []);
                };
                if (Util.Button("refresh saves", vector.x, vector.y + 98))
                {
                    EasyAPI.FetchSaveNames();
                };

                if (Util.Button("new save", vector.x + 64f, vector.y + 90))
                {
                    int TargetNum = EasyAPI.SaveFileNames.Count;
                    string TargetName = "";
                    while (true)
                    {
                        TargetName = "EasyDeliveryCoSaveData_File" + TargetNum;
                        if (!EasyAPI.SaveFileNames.Contains(TargetName))
                        {
                            break;
                        }
                        TargetNum++;
                    }
                    EasyAPI.SaveFileNames.Add(TargetName);
                    if (EasyAPI.SaveFileNames.Count > (PageNum + 1) * 4)
                    {
                        PageNum = EasyAPI.SaveFileNames.Count / 4;
                    }
                };
                return (false);
            }
        }

        [HarmonyPatch]
        class ButtonPatch : HarmonyPatch
        {
            static MethodBase TargetMethod()
            {
                return FileButton;
            }
            static bool Prefix(IntroDotExe __instance, ref float x, float y, float w, float h, int index)
            {
                string str = EasyAPI.SaveFileNames[index];
                if (str.Length <= 23)
                {
                    str = "File1";
                }
                else
                {
                    str = str.Substring(23).Substring(0, Math.Min(str.Length - 23, 12));
                }
                bool result = false;
                int num = 96;
                GamepadNavigation gnav = nav.GetValue(__instance) as GamepadNavigation;
                UIUtil Util = new(__instance.R, __instance, gnav);
                if (Util.MouseOver(x, y, w, h))
                {
                    num = 80;
                    x += 4f;
                    __instance.mouseIcon = 128;
                    result = true;
                    if (__instance.mouseButton)
                    {
                        __instance.mouseIcon = 160;
                        x -= 4f;
                    }
                    if (__instance.mouseButtonUp)
                    {
                        if (sSaveSystem.instance)
                        {
                            Destroy(sSaveSystem.instance.gameObject);
                        }
                        GameObject temp = new GameObject();
                        temp.SetActive(false);
                        temp.AddComponent<sSaveSystem>().saveFileName = EasyAPI.SaveFileNames[index];
                        EasyAPI.RenameSaveFileInput.text = str;
                        temp.transform.SetParent(Parent.parent);
                        selectedSaveFile.SetValue(__instance, temp);
                        temp.SetActive(true);
                        ToNextScreen.Invoke(__instance, []);
                    }
                }
                __instance.drawBox((x - 4f) / 8f, (y - 4f) / 8f, w / 8f, h / 8f);
                __instance.R.put(str, x + 24f, y + h / 2f - 4f);
                __instance.R.spr((float)num, 0f, x + 8f, y + 3f, 10f, 10f);
                // this method uses the name "Prefix", no annotation necessary
                return (false);
            }
        }
        [HarmonyPatch]
        class DeletePatch : HarmonyPatch
        {
            static MethodBase TargetMethod()
            {
                return NewGame;
            }
            static bool Prefix(IntroDotExe __instance)
            {
                string FilePath = Application.persistentDataPath + "/" + sSaveSystem.instance.saveFileName + ".txt";
                string FilePath2 = Application.persistentDataPath + "/" + "modded-" + sSaveSystem.instance.saveFileName + ".json";

                InterSceneData interSceneData = FindObjectOfType<InterSceneData>();
                if (interSceneData)
                {
                    Destroy(interSceneData.gameObject);
                }
                DestroyImmediate(sSaveSystem.instance);
                DeleteSaveAndGoBack.Invoke(__instance, []);
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                    EasyAPI.FetchSaveNames();
                }
                if (File.Exists(FilePath2))
                {
                    File.Delete(FilePath2);
                }
                return false;
            }
        }
        [HarmonyPatch]
        class StartGamePatch : HarmonyPatch
        {
            static MethodBase TargetMethod()
            {
                return typeof(IntroDotExe).GetMethod("StartGameScreen", BindingFlags.NonPublic | BindingFlags.Instance);
            }
            static void Prefix(IntroDotExe __instance)
            {
                Vector2 vector = new Vector2((float)(__instance.R.width / 2 - 72), (float)(__instance.R.height / 2 - 64));
                GamepadNavigation gnav = nav.GetValue(__instance) as GamepadNavigation;
                UIUtil Util = new(__instance.R, __instance, gnav);
                if (!sSaveSystem.HasKey("deliveryMoney"))
                {
                    string text = "";
                    bool delhover = false;
                    
                    if (Util.Button("delete", vector.x + 8f, vector.y + 138f, delhover))
                    {
                        NewGame.Invoke(__instance, []);
                    }
                }
                String[] keys = EasyAPI.ModdedSaveHandlers.Keys.ToArray();
                if (keys.Length > 0)
                {
                    __instance.R.put("Mods:", __instance.R.width / 2 + 72, __instance.R.height / 2 - 64);
                    int y = 12;
                    foreach (string key in keys)
                    {
                        __instance.R.put(key, __instance.R.width / 2 + 72, __instance.R.height / 2 - 64 + y);
                        y += 12;
                    }
                }
                Util.DrawInput(EasyAPI.RenameSaveFileInput, "File Name", __instance.R.width / 2 - 64, __instance.R.height / 2 - 80);
            }
        }
        [HarmonyPatch(typeof(sSaveSystem), nameof(sSaveSystem.SaveData))]
        class SaveDataPatch : HarmonyPatch
        {
            static void Postfix(sSaveSystem __instance)
            {
                string FilePath2 = Application.persistentDataPath + "/" + "modded-" + sSaveSystem.instance.saveFileName + ".json";
                Dictionary<string, string> temp = new();
                EasyAPI.OnSave?.Invoke();
                sSaveSystem.SetString("timeLastSaved", DateTime.Now.ToString());
                string text = JsonConvert.SerializeObject(EasyAPI.ModdedSaveHandlers);//dual serialization, but whatever. theres some benefits probably
                if (!File.Exists(FilePath2))
                {
                    File.WriteAllText(FilePath2, text);
                    Debug.Log("Making new modded file - Saved Successfuwwy :3");
                    return;
                }
                using (StreamWriter streamWriter = new StreamWriter(FilePath2, false))
                {
                    streamWriter.Write(text);
                }
                Debug.Log("Mods saved Successfuwwy :3");
            }
        }
        [HarmonyPatch]
        class LoadDataPatch : HarmonyPatch
        {
            static MethodInfo TargetMethod()
            {
                return (typeof(sSaveSystem).GetMethod("ParseFileContents", BindingFlags.NonPublic | BindingFlags.Instance));
            }
            static void Postfix(sSaveSystem __instance)
            {
                EasyAPI.ModdedSaveHandlers.Clear();
                string FilePath2 = Application.persistentDataPath + "/" + "modded-" + sSaveSystem.instance.saveFileName + ".json";
                if (!File.Exists(FilePath2))
                {
                    return;
                }
                StreamReader streamReader = new StreamReader(FilePath2);
                string result = streamReader.ReadToEnd();
                streamReader.Close();
                Dictionary<string, string> temp = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                foreach (KeyValuePair<string, string> kvp in temp)
                {
                    if (!EasyAPI.ModsLoaded.ContainsKey(kvp.Key) || !EasyAPI.ModsLoaded[kvp.Key])
                    {
                        EasyAPI.ModdedSaveHandlers[kvp.Key] = kvp.Value;
                    }
                }
                EasyAPI.OnLoad?.Invoke(temp);
            }
        }
    }
}
