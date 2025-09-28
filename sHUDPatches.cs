using System.Reflection;
using HarmonyLib;
using UnityEngine;
namespace EasyDeliveryAPI
{
    internal class sHUDPatches : MonoBehaviour
    {
        internal static FieldInfo heldFile = typeof(DesktopDotExe).GetField("heldFile", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static FieldInfo clickedOn = typeof(DesktopDotExe.File).GetField("clickedOn", BindingFlags.Instance | BindingFlags.NonPublic);
        [HarmonyPatch(typeof(sHUD), nameof(sHUD.Init))]
        class InitPatch : HarmonyPatch
        {
            static void Postfix(DesktopDotExe __instance)
            {
                foreach (GameObject targetListener in EasyAPI.Listeners)
                {
                    GameObject temp = Instantiate(targetListener, __instance.transform);
                    temp.name = targetListener.name;
                    temp.hideFlags = HideFlags.None;
                }
            }
        }
        [HarmonyPatch(typeof(sHUD), nameof(sHUD.FrameUpdate))]
        class FramePatch : HarmonyPatch
        {
            static void Postfix(sHUD __instance)
            {
                foreach (GameObject Listener in EasyAPI.sHUDListeners) {
                    GameObject.Find(Listener.name).gameObject.SendMessage("FrameUpdate", __instance);
                }
            }
        }
    }
}
