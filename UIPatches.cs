using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace EasyDeliveryAPI
{
    internal class UIPatches
    {
        static FieldInfo M = typeof(sOptionsMenu).GetField("M", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo R = typeof(sOptionsMenu).GetField("R", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo hoveredZones = typeof(GamepadNavigation).GetField("hoveredZones", BindingFlags.NonPublic | BindingFlags.Instance);
        static FieldInfo program = typeof(GamepadNavigation).GetField("program", BindingFlags.NonPublic | BindingFlags.Instance);
        
        [HarmonyPatch(typeof(GamepadNavigation), nameof(GamepadNavigation.GetZone))]
        class GamepadPatch : HarmonyPatch
        {
            static bool Prefix(GamepadNavigation __instance, Vector2 input, out Rect __result)
            {
                Rect r = (hoveredZones.GetValue(__instance) as List<Rect>)[0];
                if (input.magnitude < 0.4)
                {
                    __result = r;
                    return false;
                }
                ScreenProgram prog = (program.GetValue(__instance) as ScreenProgram);
                if ((prog).mouseButton)
                {
                    __result = r;
                    return false;
                }
                input.y = -input.y;
                List < (Rect, float, float) > Values = new();
                Vector2 NormInput = input.normalized;
                float Smallest = 10000;
                Rect SmallestRect = r;
                foreach (Rect rect in __instance.zones)
                {
                    if (Vector2.Dot(rect.center - r.center, NormInput) < 0.1)
                    {
                        continue;
                    }
                    
                    Vector2 MinPos = new Vector2(Mathf.Clamp(r.center.x, rect.xMin, rect.xMax), Mathf.Clamp(r.center.y, rect.yMin, rect.yMax));
                    Vector2 Distance = (MinPos - r.center) * NormInput;
                    Vector2 Offset = (MinPos - r.center) * new Vector2(NormInput.y, NormInput.x);
                    float RealDist = new Vector2(Offset.magnitude * 100, Distance.magnitude).magnitude;
                    if (RealDist < 1f) {
                        continue;
                    }
                    if (RealDist < Smallest)
                    {
                        Smallest = RealDist;
                        SmallestRect = rect;
                    }
                }
                __result = SmallestRect;
                return false;
            }
        }
        [HarmonyPatch(typeof(GamepadNavigation), nameof(GamepadNavigation.GoToZone))]
        class DragPatch : HarmonyPatch
        {
            static bool Prefix(GamepadNavigation __instance)
            {
                if (__instance.menuInput.magnitude < 0.4)
                {
                    return false;
                }
                ScreenProgram prog = (program.GetValue(__instance) as ScreenProgram);
                if (prog.mouseButton)
                {
                    ScreenSystem.mouse += __instance.menuInput * Time.deltaTime * 100 * new Vector2(1, -1);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GamepadNavigation), nameof(GamepadNavigation.DoZones))]
        class DragPatch2 : HarmonyPatch
        {
            static void Postfix(GamepadNavigation __instance)
            {
                ScreenProgram prog = (program.GetValue(__instance) as ScreenProgram);
                if (!__instance.menuInputChanged && prog.mouseButton)
                {
                    ScreenSystem.mouse += __instance.menuInput * Time.deltaTime * 100 * new Vector2(1, -1);
                }
            }
        }
        [HarmonyPatch]
        class SliderFix : HarmonyPatch
        {
            static MethodInfo TargetMethod()
            {
                return (typeof(sOptionsMenu).GetMethod("Slider", BindingFlags.NonPublic | BindingFlags.Instance));
            }
            static bool Prefix(sOptionsMenu __instance, string name, float value, float x, float y, Action<float> action)
            {
                MiniRenderer MR = R.GetValue(__instance) as MiniRenderer;
                DesktopDotExe MM = M.GetValue(__instance) as DesktopDotExe;
                int num = 10;
                for (int i = 0; i < num; i++)
                {
                    MR.spr(32f, 0f, x + 4f + (float)(i * 8), y, 8f, 8f);
                }
                float x2 = x + value * (float)num * 8f;
                MR.spr(0f, 24f, x2, y, 8f, 8f);
                if (MM.MouseOver((int)x - 8, (int)y, num * 8 + 16, 8))
                {
                    MM.mouseIcon = 128;
                    name = ">" + name;
                    
                    if (MM.mouseButton)
                    {
                        if (MM.nav.menuInput.x < 0f)
                        {
                            action(Mathf.Clamp01(value - Time.unscaledDeltaTime / 2f));
                        }
                        if (MM.nav.menuInput.x > 0f)
                        {
                            action(Mathf.Clamp01(value + Time.unscaledDeltaTime / 2f));
                        }
                        MM.mouseIcon = 160;
                        value = Mathf.InverseLerp(x + 4f, x + 4f + (float)(num * 8), MM.mouse.x);
                        value = Mathf.Clamp01(value);
                        action(value);
                    }
                }
                MR.put(name, x - (float)(name.Length * 8) - 4f, y);
                return false;
            }
        }
        [HarmonyPatch]
        class OptionsPatch : HarmonyPatch
        {
            static MethodInfo TargetMethod()
            {
                return (typeof(sOptionsMenu).GetMethod("DrawMenu", BindingFlags.NonPublic | BindingFlags.Instance));
            }
            static void Prefix(sOptionsMenu __instance, Rect p)
            {
                DesktopDotExe d = M.GetValue(__instance) as DesktopDotExe;
                MiniRenderer mr = R.GetValue(__instance) as MiniRenderer;
                UIUtil Util = new(mr, d, d.nav);
                float num = p.x + p.width / 2f - 16f;
                float num2 = p.y + 10f;

                Util.nav.Add(p.x + p.width - 40f, p.y + p.height - 84, 1000f, 8f);
                if (Util.Button("mods", p.x + p.width - 40f, p.y + p.height - 8f, true))
                {
                    EasyAPI.ChangeWindow(d, "modded settings", "modsettingslist");
                }
            }
        }
    }
}
