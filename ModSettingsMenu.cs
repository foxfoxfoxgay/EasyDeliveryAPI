using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;
namespace EasyDeliveryAPI;
internal class ModSettingsMenu : MonoBehaviour
{
    public DesktopDotExe.WindowView windowViewer;
    public void FrameUpdate(DesktopDotExe.WindowView view)
    {
        util.M = view.M;
        util.R = view.R;
        util.nav = view.M.nav;
        Rect p = new Rect(view.position * 8f, view.size * 8f);
        string str1 = "Page - " + PageNum;
        float width = (str1.Length * 8) / 2;
        float xc0 = p.x + p.width / 2;
        float offset = 20;
        float size = 12;
        int max = (int)Math.Floor((p.height - offset) / size);
        if (util.Button("<", xc0 - width - 8, p.y + p.height - 8f, true))
        {
            PageNum = Math.Max(PageNum-1, 0);
        }
        util.R.put(str1, xc0 - width, p.y + p.height - 8f);
        if (util.Button(">", xc0 + width, p.y + p.height - 8f, true))
        {
            if ((EasyAPI.ConfigList.Count) > PageNum * max)
            {
                PageNum += 1;
            }
        };
        float current = 8;
        List<string> Keys = EasyAPI.ConfigList.Keys.ToList();
        for (int i = 0; i < Math.Min(max, EasyAPI.ConfigList.Count - (PageNum * max)); i++)
        {
            string str2 = Keys[PageNum * max + i];
            ConfigFile Current = EasyAPI.ConfigList[str2];
            width = (str2.Length * 8) / 2;
            if (util.Button(str2, xc0 - width - 8, p.y + current, true))
            {
                if (EasyAPI.CustomOptionsMenus.ContainsKey(str2))
                {
                    windowViewer = EasyAPI.MakeWindow(view.M, str2, "optionsmenu" + str2);
                }
                else
                {
                    ModSettingsPage.TargetFile = Current;
                    ModSettingsPage.BuildVals();
                    windowViewer = EasyAPI.MakeWindow(view.M, str2, "modsettingssingle");
                }
            }
            current += size;
        }
        util.R.put("foxfoxfox.gay", p.x + p.width - (("foxfoxfox.gay").Length * 8f), p.y + p.height);
        if (windowViewer != null)
        {
            windowViewer.size = new Vector2(32f, 25f);
            if (!windowViewer.kill)
            {
                windowViewer.targetPosition = new Vector2(20f - this.windowViewer.size.x / 2f, 3f);
            }
            bool flag;
            util.nav.zones.Clear();
            windowViewer.Draw(out flag);
            if (flag)
            {
                windowViewer = null;
            }
        }
    }
    public void BackButtonPressed()
    {
        if (windowViewer != null && !windowViewer.kill) {
            windowViewer.Kill();
            util.M.SendMessage("InterruptBackButton", SendMessageOptions.DontRequireReceiver);
            return;
        };
        if (EasyAPI.ChangeWindow(util.M as DesktopDotExe, "settings", "OptionsMenu"))
        {
            util.M.SendMessage("InterruptBackButton", SendMessageOptions.DontRequireReceiver);

        }
    }

    private UIUtil util = new UIUtil();
    private int PageNum = 0;
}
