using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
namespace EasyDeliveryAPI;
internal class ModSettingsPage : MonoBehaviour
{
    private void DrawDescription(UIUtil util, Rect p, string str, string title)
    {
        int layer = util.R.lget();
        util.R.lset(4);
        p.y -= 8f;
        float num = p.width / 8f - 2f;
        util.drawBox(p.x / 8f, (p.y + p.height) / 8f - 8f, num, 7f, false, title);
        util.R.spr(8f, 8f, p.x + 8f, p.y + p.height - 64f + 8f, 8f, 8f, false, num * 8f - 8f, 48f);
        string[] array = str.Split('\n');
        string text = "";
        for (int i = 0; i < array.Length; i++)
        {
            text += UIUtil.FormatLine(array[i], num - 2f);
            text += "\n";
        }
        util.R.put(text, p.x + 16f, (p.y + p.height) - 64f);
        util.R.lset(layer);
    }
    static string DrawBool(UIUtil util, float xpos, float ypos, ConfigEntryBase entry, string Name)
    {
        bool Hovered = false;
        bool? ValChanged = util.Toggle(Name, entry.GetSerializedValue() == "true", xpos, ypos, out Hovered);
        if (ValChanged != null)
        {
            entry.SetSerializedValue(((bool)ValChanged).ToString());
        }
        return Hovered ? entry.Description.Description : null;
    }
    static string DrawInt(UIUtil util, float xpos, float ypos, ConfigEntryBase entry, string Name)
    {
        bool Hovered = false;
        float? ValChanged = util.Adjustable(Name, entry.GetSerializedValue(), 1f, xpos, ypos, out Hovered);
        if (ValChanged != null)
        {
            entry.SetSerializedValue((int.Parse(entry.GetSerializedValue()) + (int)ValChanged).ToString());
        }
        return Hovered ? entry.Description.Description : null;
    }
    static string DrawFloat(UIUtil util, float xpos, float ypos, ConfigEntryBase entry, string Name)
    {
        bool Hovered = false;
        float? ValChanged = util.Slider(Name, float.Parse(entry.GetSerializedValue()), xpos, ypos, out Hovered);
        if (ValChanged != null)
        {
            entry.SetSerializedValue(((float)ValChanged).ToString());
        }
        return Hovered ? entry.Description.Description : null;
    }
    static string DrawString(UIUtil util, float xpos, float ypos, ConfigEntryBase entry, string Name)
    {
        bool Hovered = false;
        util.R.put(Name + ": ", xpos - ((Name + ":").Length * 8), ypos);
        util.DrawInput(Fields[Name], "text here", xpos + 8, ypos, out Hovered);
        return Hovered ? entry.Description.Description : null;
    }
    static string DrawEnum(UIUtil util, float xpos, float ypos, ConfigEntryBase entry, string Name)
    {
        List<string> options = new();
        foreach (var item in Enum.GetValues(entry.SettingType))
        {
            options.Add(item.ToString());
        }
        bool Hovered = false;
        int indx = options.IndexOf(entry.GetSerializedValue());
        indx = indx == -1 ? 0 : indx;
        string ValChanged = util.Dropdown(Name, indx, options.ToArray(), xpos, ypos, out Hovered);
        if (ValChanged != null)
        {
            entry.SetSerializedValue(ValChanged);
        }
        return Hovered ? entry.Description.Description : null;
    }
    private static string DrawLabel(UIUtil util, float xpos, float ypos, ConfigEntryBase entry, string Name)
    {
        util.R.put(Name, xpos - Name.Length * 4, ypos);
        return null;
    }

    public static List<Func<ConfigEntryBase, Func<UIUtil, float, float, ConfigEntryBase, string, string>>> CustomDrawHandler = new();

    public static ConfigFile TargetFile;
    static Dictionary<string, InputField> Fields = new();

    static List<(string, ConfigEntryBase, Func<UIUtil, float, float, ConfigEntryBase, string, string>)> builtVals = new();
    public static void BuildVals()
    {
        foreach (KeyValuePair<string, InputField> destroy in Fields)
        {
            Destroy(destroy.Value.gameObject);
        }
        builtVals.Clear();
        Fields.Clear();
        Dictionary<string, Dictionary<string, ConfigEntryBase>> Entries = new();
        Dictionary<string, Func<UIUtil, float, float, ConfigEntryBase, string, string>> CustomDraws = new();
        foreach (ConfigDefinition Definition in TargetFile.Keys)
        {
            Type EntryType = TargetFile[Definition].SettingType;
            bool f = false;
            foreach (Func<ConfigEntryBase, Func<UIUtil, float, float, ConfigEntryBase, string, string>> func in CustomDrawHandler)
            {
                Func<UIUtil, float, float, ConfigEntryBase, string, string> val = func(TargetFile[Definition]);
                if (val != null)
                {
                    CustomDraws.Add(Definition.Key, val);
                    if (!Entries.ContainsKey(Definition.Section))
                    {
                        Entries[Definition.Section] = new Dictionary<string, ConfigEntryBase>();
                    }
                    Entries[Definition.Section][Definition.Key] = TargetFile[Definition];
                    f = true;
                    break;
                }
            }
            if (f)
            {
                continue;
            }
            if (EntryType.IsEnum || EntryType == typeof(int) || EntryType.IsEnum || EntryType == typeof(bool) || EntryType == typeof(string) || EntryType == typeof(char) || EntryType == typeof(double) || EntryType == typeof(float))
            {
                if (!Entries.ContainsKey(Definition.Section))
                {
                    Entries[Definition.Section] = new Dictionary<string, ConfigEntryBase>();
                }
                Entries[Definition.Section][Definition.Key] = TargetFile[Definition];
            }
        }
        List<string> keys = Entries.Keys.ToList();
        keys.Sort();
        for (int i = 0; i < keys.Count; i++)
        {
            string sectionname = keys[i];
            Dictionary<string, ConfigEntryBase> Entry = Entries[keys[i]];
            builtVals.Add((sectionname, null, DrawLabel));
            List<string> keys2 = Entry.Keys.ToList();
            keys2.Sort();
            for (int i2 = 0; i2 < keys2.Count; i2++)
            {
                string key = keys2[i2];
                ConfigEntryBase ConfigEntry = Entry[key];
                EasyAPI.Log.LogInfo(ConfigEntry.SettingType);
                if (CustomDraws.ContainsKey(key))
                {
                    builtVals.Add((key, ConfigEntry, CustomDraws[key]));
                }else if (ConfigEntry.SettingType == typeof(bool))
                {
                    builtVals.Add((key, ConfigEntry, DrawBool));
                }
                else if (ConfigEntry.SettingType == typeof(int))
                {
                    builtVals.Add((key, ConfigEntry, DrawInt));
                }
                else if (ConfigEntry.SettingType == typeof(float))
                {
                    builtVals.Add((key, ConfigEntry, DrawFloat));
                }
                else if (ConfigEntry.SettingType == typeof(string) || ConfigEntry.SettingType == typeof(char))
                {
                    GameObject temp = new GameObject();
                    temp.hideFlags = HideFlags.HideAndDontSave;
                    InputField TempField = EasyAPI.MakeInputField(temp);
                    TempField.text = ConfigEntry.GetSerializedValue();
                    TempField.characterLimit = ConfigEntry.SettingType == typeof(string) ? 1600 : 1;
                    TempField.onSubmit.AddListener((string submittedname) =>
                    {
                        ConfigEntry.SetSerializedValue(submittedname);
                    });
                    Fields.Add(key, TempField);
                    builtVals.Add((key, ConfigEntry, DrawString));
                }
                else if (ConfigEntry.SettingType.IsEnum)
                {
                    builtVals.Add((key, ConfigEntry, DrawEnum));
                }
            }
        }
    }
    private int PageNum = 0;
    public void FrameUpdate(DesktopDotExe.WindowView view)
    {
        util.M = view.M;
        util.R = view.R;
        util.nav = view.M.nav;
        Rect p = new Rect(view.position * 8f, view.size * 8f);
        float yNum = 8f;
        int max = (int)Math.Floor((float)((p.height - 88f) / 12f));
        string HintStr = "";
        string TitleStr = "";
        for (int i = 0; i < Math.Min(max, builtVals.Count - (PageNum * max)); i++)
        {
            (string, ConfigEntryBase, Func<UIUtil, float, float, ConfigEntryBase, string, string>) val = builtVals[i + PageNum * max];
            string outstr = val.Item3(util, p.x + (p.width / 2), p.y + yNum, val.Item2, val.Item1);
            if (outstr != null)
            {
                HintStr = outstr;
                TitleStr = val.Item1;
            }
            yNum += 12;
        }
        Math.Min(max, builtVals.Count - PageNum * max);
        string str1 = "Page - " + PageNum;
        if (util.Button("<", p.x + p.width / 2 - 8f - (str1.Length * 4), p.y + p.height - 72f, true))
        {
            PageNum = Math.Max(PageNum - 1, 0);
        }
        util.R.put(str1, p.x + p.width / 2 - (str1.Length * 4), p.y + p.height - 72f);
        if (util.Button(">", p.x + p.width / 2 + (str1.Length * 4f), p.y + p.height - 72f, true))
        {
            if ((builtVals.Count - 1) > (PageNum + 1) * max)
            {
                PageNum += 1;
            }
        };
        DrawDescription(util, new Rect(p.x +8f, p.y + 8f, p.width, p.height), HintStr, TitleStr);
    }
    private UIUtil util = new UIUtil();
}
