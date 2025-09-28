using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

namespace EasyDeliveryAPI
{
    public class UIUtil
    {
        static MethodInfo ActivateInputFieldInternal = typeof(InputField).GetMethod("ActivateInputFieldInternal", BindingFlags.NonPublic | BindingFlags.Instance);
        public bool NavEnabled;
        public ScreenProgram M;

        public GamepadNavigation nav;
        public MiniRenderer R;
        public bool MouseOver(float x, float y, float w, float h)
        {
            bool flag = M.mouse.x > x && M.mouse.x < (x + w);
            bool flag2 = M.mouse.y > y && M.mouse.y < (y + h);
            nav.Add(x, y, w, h);
            return flag && flag2;
        }
        public bool MouseOver(int x, int y, int w, int h)
        {
            bool flag = M.mouse.x > (float)x && M.mouse.x < (float)(x + w);
            bool flag2 = M.mouse.y > (float)y && M.mouse.y < (float)(y + h);
            nav.Add((float)x, (float)y, (float)w, (float)h);
            return flag && flag2;
        }
        public float? Adjustable(string name, string value, float delta, float x, float y)
        {
            bool tmp;
            return (Adjustable(name, value, delta, x, y, out tmp));
        }
        public float? Adjustable(string name, string value, float delta, float x, float y, out bool Hovered)
        {
            x -= 4;
            Hovered = false;
            float? ret = null;
            R.put(name, x - (name.Length * 8), y);
            if (Button("[-]", x - (name.Length * 8) - 24, y, out Hovered, true))
            {
                ret = delta * -1;
            };
            x += 32f;
            R.put(value, x - (float)(value.Length * 8), y);
            string name2 = "[+]";
            if (Button(name2, x, y, out Hovered, true))
            {
                ret = delta;
            };
            return ret;
        }
        public bool Button(string name, float x, float y, bool smol = false, bool manualhover = false)
        {
            bool tmp;
            return(Button(name , x, y, out tmp, smol, manualhover));
        }
        public bool Button(string name, float x, float y, out bool Hovered, bool smol = false, bool manualhover = false)
        {
            Hovered = false;
            bool res = false;
            if (MouseOver((int)x - 2, (int)y, (name.Length * 8) + 4, 8))
            {
                Hovered = true;
                if (!smol)
                {
                    x += 4f;
                    name = ">" + name;
                }
                M.mouseIcon = 128;
                //R.put(">", x - 8f, y);
                if (M.mouseButton)
                {
                    M.mouseIcon = 160;
                }
                if (M.mouseButtonUp)
                {
                    res = true;
                }
            }
            else if (manualhover)
            {
                x += 4f;
                name = ">" + name;
            }
            R.put(name, x, y);
            return res;
        }
        public void DrawInput(InputField Field, string PlaceholderText, float x, float y)
        {
            bool tmp;
            DrawInput(Field, PlaceholderText, x, y, out tmp);
        }
        public void DrawInput(InputField Field, string PlaceholderText, float x, float y, out bool Hovered)
        {
            Hovered = false;
            if (Button(Field.text == "" ? PlaceholderText : Field.text, x, y, out Hovered, false, Field.isFocused))
            {
                ActivateInputFieldInternal.Invoke(Field, []);
            }
        }
        public string Dropdown(string name, int value, string[] options, float x, float y)
        {
            bool tmp;
            return (Dropdown(name, value, options, x, y, out tmp));
        }
        public string Dropdown(string name, int value, string[] options, float x, float y, out bool Hovered)
        {
            Hovered = false;
            string retv = null;
            string text = "[" + options[value] + "]";
            R.put(text, x + 4f, y);
            if (MouseOver((int)x - 2, (int)y, text.Length * 8 + 4, 8))
            {
                Hovered = true;
                M.mouseIcon = 128;
                name = ">" + name;
                if (M.mouseButton)
                {
                    M.mouseIcon = 160;
                }
                if (M.mouseButtonUp)
                {
                    retv = options[(value + 1) % options.Length];
                }
            }
            R.put(name, x - (float)(name.Length * 8) - 4f, y);
            return retv;
        }
        public bool? Toggle(string name, bool state, float x, float y, out bool Hovered)
        {
            Hovered = false;
            return (Toggle(name, state, x, y, "on", "off", out Hovered));
        }
        public bool? Toggle(string name, bool state, float x, float y)
        {
            bool tmp;
            return (Toggle(name, state, x, y, "on", "off", out tmp));
        }
        public bool? Toggle(string name, bool state, float x, float y, string on, string off)
        {
            bool tmp;
            return (Toggle(name, state, x, y, on, off, out tmp));
        }
        public bool? Toggle(string name, bool state, float x, float y, string on, string off, out bool Hovered)
        {
            Hovered = false;
            bool? retv = null;
            string text = "[" + (state ? on : off) + "]";
            R.put(text, x + 4f, y);
            if (MouseOver((int)x - 2, (int)y, text.Length * 8 + 4, 8))
            {
                Hovered = true;
                M.mouseIcon = 128;
                name = ">" + name;
                if (M.mouseButton)
                {
                    M.mouseIcon = 160;
                }
                if (M.mouseButtonUp)
                {
                    retv = !state;
                }
            }
            R.put(name, x - (float)(name.Length * 8) - 4f, y);
            return retv;
        }
        public float? Slider(string name, float value, float x, float y )
        {
            bool tmp;
            return (Slider(name, value, x, y, out tmp));
        }
        public float? Slider(string name, float value, float x, float y, out bool Hovered)
        {
            Hovered = false;
            float? retv = null;
            int num = 10;
            for (int i = 0; i < num; i++)
            {
                R.spr(32f, 0f, x + 4f + (float)(i * 8), y, 8f, 8f);
            }
            float x2 = x + value * (float)num * 8f;
            R.spr(0f, 24f, x2, y, 8f, 8f);
            if (MouseOver((int)x - 8, (int)y, num * 8 + 16, 8))
            {
                Hovered = true;
                M.mouseIcon = 128;
                name = ">" + name;
                if (M.mouseButton)
                {
                    if (nav.menuInput.x < 0f)
                    {
                        retv = (Mathf.Clamp01(value - Time.unscaledDeltaTime / 2f));
                    }
                    if (nav.menuInput.x > 0f)
                    {
                        retv = (Mathf.Clamp01(value + Time.unscaledDeltaTime / 2f));
                    }
                    M.mouseIcon = 160;
                    value = Mathf.InverseLerp(x + 4f, x + 4f + (float)(num * 8), M.mouse.x);
                    value = Mathf.Clamp01(value);
                    retv = (value);
                    /*if (mouseYLock == 0f)
                    {
                        mouseYLock = M.mouse.y;
                    }*///lotta effort for a small vis change
                }
            }
            R.put(name, x - (float)(name.Length * 8) - 4f, y);
            return retv;
        }
        

        public static string FormatLine(string input, float w)
        {
            string[] array = input.Split(' ');
            string text = array[0];
            string str = "";
            for (int i = 1; i < array.Length; i++)
            {
                if ((float)(text.Length + array[i].Length) <= w)
                {
                    text = text + " " + array[i];
                }
                else
                {
                    str = str + "\n" + text;
                    text = array[i];
                }
            }
            return str + "\n" + text;
        }
        public void tile(float sx, float sy, float x, float y)
        {
            R.spr(sx, sy, x * 8f, y * 8f, 8f, 8f);
        }
        public static void tile(MiniRenderer R, float sx, float sy, float x, float y)
        {
            R.spr(sx, sy, x * 8f, y * 8f, 8f, 8f);
        }
        public void drawBox(float x, float y, float w, float h, bool hasTop)
        {
            int num = hasTop ? 24 : 0;
            int num2 = hasTop ? 16 : 0;
            int num3 = (int)x + 1;
            while ((float)num3 < x + w)
            {
                tile((float)(8 + num), (float)num2, (float)num3, y);
                tile(8f, 16f, (float)num3, y + h);
                num3++;
            }
            int num4 = (int)y + 1;
            while ((float)num4 < y + h)
            {
                tile(0f, 8f, x, (float)num4);
                tile(16f, 8f, x + w, (float)num4);
                num4++;
            }
            tile(0f, 16f, x, y + h);
            tile(16f, 16f, x + w, y + h);
            tile((float)num, (float)num2, x, y);
            tile((float)(16 + num), (float)num2, x + w, y);
        }
        public static void drawBox(MiniRenderer R, float x, float y, float w, float h, bool hasTop)
        {
            int num = hasTop ? 24 : 0;
            int num2 = hasTop ? 16 : 0;
            int num3 = (int)x + 1;
            while ((float)num3 < x + w)
            {
                tile(R, (float)(8 + num), (float)num2, (float)num3, y);
                tile(R, 8f, 16f, (float)num3, y + h);
                num3++;
            }
            int num4 = (int)y + 1;
            while ((float)num4 < y + h)
            {
                tile(R, 0f, 8f, x, (float)num4);
                tile(R, 16f, 8f, x + w, (float)num4);
                num4++;
            }
            tile(R, 0f, 16f, x, y + h);
            tile(R, 16f, 16f, x + w, y + h);
            tile(R, (float)num, (float)num2, x, y);
            tile(R, (float)(16 + num), (float)num2, x + w, y);
        }
        public static void drawBox(MiniRenderer R, float x, float y, float w, float h, bool hasTop, string title)
        {
            drawBox(R, x, y, w, h, hasTop);
            R.put(title, x * 8 + 8f, y * 8f);
        }
        public void drawBox(float x, float y, float w, float h, bool hasTop, string title)
        {
            drawBox(x, y, w, h, hasTop);
            R.put(title, x * 8 + 8f, y * 8f);
        }

        public UIUtil(MiniRenderer PR, ScreenProgram PM , GamepadNavigation pnav)
        {
            R = PR;
            M = PM;
            nav = pnav;
        }
        public UIUtil()
        {
        }
    }
}
