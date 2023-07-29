using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ABI_RC.Core.Player;
using MelonLoader;
using Tayou.CustomNameplateMod;
using UnityEngine;

[assembly: MelonInfo(typeof(Mod), "Custom Nameplates", "1.1.0", "Tayou")]
[assembly: MelonColor(ConsoleColor.Yellow)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
[assembly: HarmonyDontPatchAll]
namespace Tayou.CustomNameplateMod {
    public class Mod : MelonMod {

        public static MelonPreferences_Entry<bool> enabled;
        public static MelonPreferences_Entry<Color32> defaultColor;
        public static MelonPreferences_Entry<Color32> friendsColor;
        public static MelonPreferences_Entry<Color32> legendColor;
        public static MelonPreferences_Entry<Color32> guideColor;
        public static MelonPreferences_Entry<Color32> talkingDefaultColor;
        public static MelonPreferences_Entry<Color32> talkingFriendsColor;
        public static MelonPreferences_Entry<Color32> talkingLegendColor;
        public static MelonPreferences_Entry<Color32> talkingGuideColor;
        public static MelonPreferences_Entry<bool> showFriendIcon;
        public static MelonPreferences_Entry<bool> showMicIcon;
        public static MelonPreferences_Entry<bool> noBackground;
        public static MelonPreferences_Entry<bool> imagesUnpacked;
        public static MelonPreferences_Entry<string> profile; //TODO: implement
        public static MelonPreferences_Entry<bool> debugLogging;

        public static NameplateStyleData DataOriginal;
        public static NameplateStyleData DataCustom;

        public static readonly Dictionary<PlayerNameplate, Helper> CustomNameplateHelpers = new Dictionary<PlayerNameplate, Helper>();


        private static HarmonyLib.Harmony _instance  = new HarmonyLib.Harmony(Guid.NewGuid().ToString());
        
        private static void ApplyPatches(Type type)
        {
            ConsoleLog($"Applying {type.Name} patches!");
            try {
                HarmonyLib.Harmony.CreateAndPatchAll(type, BuildInfo.Name + "_Hooks");
            } catch (Exception e) {
                MelonLogger.Error($"Failed while patching {type.Name}!\n{e}");
            }
        }
        
        public override void OnApplicationStart() {
            DataOriginal = new NameplateStyleData();
            DataCustom = new CustomNameplateStyleData();

            var category = MelonPreferences.CreateCategory("CustomNameplates", "Custom Nameplates");
            enabled =               category.CreateEntry("Enabled",                 true, "Enabled");
                                    category.CreateEntry("----- Colors -----",      false, "----- Colors -----", true);
            defaultColor =          category.CreateEntry("DefaultColor",            new Color32(75, 75, 75, 150), "Default Color");
            friendsColor =          category.CreateEntry("FriendsColor",            new Color32(153, 153, 0, 50), "Friends Color");
            legendColor =           category.CreateEntry("LegendColor",             new Color32(50, 150, 147, 50), "Legend Color");
            guideColor =            category.CreateEntry("GuideColor",              new Color32(221, 90, 0, 50), "Guide Color");
            talkingDefaultColor =   category.CreateEntry("TalkingDefaultColor",     new Color32(100, 100, 100, 200), "Default Color when talking");
            talkingFriendsColor =   category.CreateEntry("TalkingFriendsColor",     new Color32(153, 153, 0, 150), "Friends Color when talking");
            talkingLegendColor =    category.CreateEntry("TalkingLegendColor",      new Color32(50, 150, 147, 150), "Legend Color when talking");
            talkingGuideColor =     category.CreateEntry("TalkingGuideColor",       new Color32(221, 90, 0, 150), "Guide Color when talking");
                                    category.CreateEntry("----- Misc Settings -----", false, "----- Misc Settings -----", true);
            showFriendIcon =        category.CreateEntry("ShowFriendIcon",          false, "Show Friend Icon", "Shows a icon next to the nameplate, that indicates that the player is a friend.");
            showMicIcon =           category.CreateEntry("ShowMicIcon",             false, "Show Mic Icon", "Shows an extra mic icon next to the nameplate, which indicates if the player is talking.");
            noBackground =          category.CreateEntry("NoBackground",            false, "No Background", "Hide Background Panels and color Nameplate Text instead. This might be hard to see in some circumstances.");
            imagesUnpacked =        category.CreateEntry("ImagesUnpacked",          false, "Images Cached", "Indicates if the packaged images have been extracted, only set to true if you want to overwrite your custom images, or if you (accidently) removed them.", true);
                                    category.CreateEntry("----- Debug -----", false, "----- Debug -----", true);
            debugLogging =          category.CreateEntry("DebugLogging", false, "Debug Logging", "Prints various info to the melon loader console for debug purposes.");

            if (!imagesUnpacked.EditedValue)
                CacheImages();

            LoadImages();
            ApplyPatches(typeof(PlayerNameplatePatch));
        }

        public static void ConsoleLog(string message, bool debug = false) {
            if (!debug || debugLogging.EditedValue) {
                MelonLogger.Msg(message);
            }
        }

        private static void LoadImages() {
            const string imagesDir = "UserData/CustomNameplates/";
            ConsoleLog("Loading Images from Data Dir: " + imagesDir, true);
            DataCustom.backgroundImage = LoadImage(Path.Combine(imagesDir, "background.png"), new Vector4(255, 0, 255, 0));
            DataCustom.staffBackgroundImage = LoadImage(Path.Combine(imagesDir, "background.png"), new Vector4(255, 0, 255, 0)); //TODO: add extra image for staff plate
            DataCustom.profileBackgroundImage = LoadImage(Path.Combine(imagesDir, "profileIcon.png"));
            DataCustom.micOnImage = LoadImage(Path.Combine(imagesDir, "micOn.png"));
            DataCustom.micOffImage = LoadImage(Path.Combine(imagesDir, "micOff.png"));
            DataCustom.friendImage = LoadImage(Path.Combine(imagesDir, "friend.png"));
        }
        
        private static Sprite LoadImage(string path, Vector4 border = new Vector4()) {
            Texture2D tex = new Texture2D(256, 256);
            tex.LoadImage(File.ReadAllBytes(path));
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 200, 1000u, SpriteMeshType.FullRect, border, false);
        }

        private static void CacheImages() {
            ConsoleLog("Unpacking Images from Mod", true);
            const string imagesDir = "UserData/CustomNameplates/";
            if (!Directory.Exists(imagesDir))
                Directory.CreateDirectory(imagesDir);
            Properties.Resources.background.Save(Path.Combine(imagesDir, "background.png"));
            Properties.Resources.background.Save(Path.Combine(imagesDir, "profileIcon.png"));
            Properties.Resources.micOn.Save(Path.Combine(imagesDir, "micOn.png"));
            Properties.Resources.micOff.Save(Path.Combine(imagesDir, "micOff.png"));
            Properties.Resources.friend.Save(Path.Combine(imagesDir, "friend.png"));
            imagesUnpacked.EditedValue = true;
            ConsoleLog("Images Unpacked successfully to " + imagesDir, true);
        }

        public static object InvokeMethod(object targetObject, string methodName, params object[] parameters) {
            return targetObject.GetType()
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.Invoke(targetObject, parameters);
        }

        public static object GetField(object targetObject, string fieldName) {
            return targetObject.GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(targetObject);
        }

        public static object GetProperty(object targetObject, string propertyName) {
            return targetObject.GetType()
                .GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(targetObject);
        }
    }

    /// <summary>
    /// Container Class to store various nameplate data
    /// </summary>
    public class NameplateStyleData {
        public Sprite backgroundImage;
        public Vector3 backgroundImagePos;
        public Vector3 backgroundImageScale;

        public Sprite staffBackgroundImage;
        public Vector3 staffBackgroundImagePos;
        public Vector3 staffBackgroundImageScale;

        public Sprite profileImageMask;
        public Vector3 profileImageMaskPos;
        public Vector3 profileImageMaskScale;

        public Sprite profileBackgroundImage;
        public Vector3 profileBackgroundImagePos;
        public Vector3 profileBackgroundImageScale;

        public Sprite friendImage;
        public Vector3 friendImagePos;
        public Vector3 friendImageScale;

        public Sprite micOnImage;
        public Sprite micOffImage;
        public Vector3 micImagePos;
        public Vector3 micImageScale;
    }

    /// <summary>
    /// Container Class to store various nameplate data
    /// with custom defaults for certain fields
    /// </summary>
    public class CustomNameplateStyleData : NameplateStyleData {

        public CustomNameplateStyleData() {
            //backgroundImage;
            //backgroundImagePos;
            //backgroundImageScale;

            //staffBackgroundImage;
            staffBackgroundImagePos = new Vector3(-0.08f, 0.2902f, 0f);
            //staffBackgroundImageScale;

            //profileImageMask;
            //profileImageMaskPos;
            profileImageMaskScale = new Vector3(1.45f, 1.25f, 1);

            //profileBackgroundImage;
            //profileBackgroundImagePos;
            profileBackgroundImageScale = new Vector3(1.25f, 1.1f, 1);

            //friendImage;
            friendImagePos = new Vector3(0.60f, 0.39f, 0);
            friendImageScale = new Vector3(0.9f, 0.6f, 1);

            //micOnImage;
            //micOffImage;
            micImagePos = new Vector3(0.944f, 0.39f, 0);
            //micImageScale;
        }
    }
}