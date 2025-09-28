# Modding Framework For [Easy Delivery Co](https://store.steampowered.com/app/3293010/Easy_Delivery_Co/).

# Features
## Reworked Save System
You can rename save files, store modded data to save files, and have more than 3 save files simultaneously.
<img width="604" height="598" alt="image" src="https://github.com/user-attachments/assets/50c32423-42c6-43a6-a1ef-da079409eee7" />
<img width="1005" height="721" alt="image" src="https://github.com/user-attachments/assets/6bf47768-31e4-4dbd-b0ee-c79ef7f40ad2" />

## Draggable Programs

Programs can now be dragged and placed into folders, even on controller.
## More Programs Folder
<img width="1142" height="955" alt="image" src="https://github.com/user-attachments/assets/bc3b0203-6329-43d5-b284-879ae6e3c8ea" />

Allows for the storage of modded and vanilla programs

## Reworked Controller UI Navigation
More reliable formula for navigating UI on controller now.

## Modded Settings Menu
<img width="1119" height="592" alt="image" src="https://github.com/user-attachments/assets/a891abd8-02ba-4db0-a83a-b66071c64023" />
Adds a ModList with Modded Settings

# Examples
## Saving Modded Data
```cs
struct ExampleSaveFile {
    bool SaveValue;
    string SaveVal;
}
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

public class ExamplePlugin : BaseUnityPlugin
{
    private void Awake()
    {
        new ModdedSaveSystem<ExampleSaveFile>("ExamplePlugin");
    }
}
```
## Adding Config to Mod Options
Adding BepInEx Config
```cs
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

public class ExamplePlugin : BaseUnityPlugin
{
    private void Awake()
    {
        EasyAPI.AddConfig("ExamplePlugin", Config);
    }
}
```
See documentation on BepInEx Configuration [here](https://docs.bepinex.dev/v5.4.11/articles/dev_guide/plugin_tutorial/3_configuration.html)
Custom Config UI Example
```cs
class WindowViewUI : MonoBehaviour
{
    UIUtil util;
    public void FrameUpdate(DesktopDotExe.WindowView view)
    {
        util.M = view.M;
        util.R = view.R;
        util.nav = view.M.nav;
        Rect p = new Rect(view.position * 8f, view.size * 8f);
        float num = p.x + p.width / 2f;
        float num2 = p.y + 12f;
        bool? DragEnabledChanged = util.Toggle("drag enabled", PlayerPrefs.GetInt("file drag enabled") == 0, num, num2);
        
        num2 += 12;
        bool? MoreProgramsFolderEnabled = util.Toggle("more folder", PlayerPrefs.GetInt("more programs folder") == 0, num, num2);
        
    }
}
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]

public class ExamplePlugin : BaseUnityPlugin
{
    private void Awake()
    {
        EasyAPI.AddCustomOptionsMenu<WindowViewUI>("ExamplePlugin");
    }
}
```
Custom config type rendering example
```cs
AddCustomSettingsDrawHandler((ConfigEntryBase base) => {
    if (base.Description.Tags.Contains("Example")) {
        return (UIUtil util, float xpos, float ypos, ConfigEntryBase entry, string Name) => {
            util.R.put(Name, xpos - Name.Length * 4, ypos); return null;
        }
    } else {
        return null;
    }
});
```

## Adding Files
Easy Delivery Co has two main types of UI. ScreenPrograms (Map) and WindowViews (Mail / Options).
UIUtil is a class full of ui creation tools.
WindowView example
```cs
private void Awake()
{
    EasyAPI.AddListener<WindowViewUI>("ExamplePlugin")
    DesktopDotExe.File ExampleFile = EasyAPI.InstantiateFile();
    ExampleFile.name = "WOW!";
    ExampleFile.data = "listener_ExamplePlugin";
    ExampleFile.type = DesktopDotExe.FileType.exe;
    EasyAPI.AddProgram(new TextInputTest())
}
```
ScreenProgram example
```cs
public class TextInputTest : ScreenProgram
{
    public override void Setup()
    {
        this.mouseIcon = 112;
        if (Test == null)
        {
            Test = EasyAPI.MakeInputField(gameObject);
            Test.characterLimit = 20;
        }
        nav = new GamepadNavigation(this, this.audioSource, this.select);
    }

    public override void Resume()
    {


    }

    public override void Draw()
    {
        EasyDesktopAPI.DrawInput(this, nav, Test, "Type here!", 10f, 10f);
        if (this.backButtonDown)
        {
            this.screenSystem.SetMenu(1);
            this.screenSystem.OpenMenu();
        }
    }

    static InputField Test;
    GamepadNavigation nav;
    private AudioSource audioSource;
    public AudioClip select;
}
private void Awake()
{
    TextInputTest program = new TextInputTest();
    DesktopDotExe.File ExampleFile = EasyAPI.InstantiateFile();
    ExampleFile.name = "WOW!";
    ExampleFile.data = "modded_" + program.title;
    ExampleFile.type = DesktopDotExe.FileType.exe;
    EasyAPI.AddProgram(program)
}
```
## Adding to the ingame HUD
```cs
public class EasyDeliveryHudTest : MonoBehaviour
{
    // Token: 0x0600022A RID: 554 RVA: 0x00010170 File Offset: 0x0000E370
    private static FieldInfo RField = typeof(sHUD).GetField("R", BindingFlags.NonPublic | BindingFlags.Instance);
   
    public void FrameUpdate(sHUD hud)
    {
        EasyDeliveryAPI.Log.LogInfo("hi!");
        MiniRenderer R = (RField.GetValue(hud) as MiniRenderer);
        Vector2 vector = new Vector2((float)(R.width - 64 - 68), (float)(R.height - 64));
        R.put("yeagh", vector.x, vector.y - 40f );
    }
}
private void Awake()
{
    EasyAPI.AddsHUDListener<EasyDeliveryHudTest>("ExamplePlugin");
}
```
## Extra UI Stuff
Sub-Windowview example
```cs
public void FrameUpdate(DesktopDotExe.WindowView view)
{
    util.M = view.M;
    util.R = view.R;
    util.nav = view.M.nav;
    Rect p = new Rect(view.position * 8f, view.size * 8f);
    if (util.Button("Test", p.x + 10f, p.y + 10f))
    {
        windowViewer = EasyAPI.MakeWindow(view.M, str2, "optionsmenu" + str2); // looking for a registered listener
    }
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
}
```
