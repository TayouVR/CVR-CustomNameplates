using System;
using MelonLoader;
using UnityEngine;
using System.IO;
using Tayou;
using ABI_RC.Core.Player;
using System.Collections.Generic;

[assembly: MelonInfo(typeof(CustomNameplatesMod), "Custom Nameplates", "1.0.5", "Tayou")]
[assembly: MelonColor(ConsoleColor.Yellow)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace Tayou {
    public class CustomNameplatesMod : MelonMod {

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

        public static NameplateStyleData data_original;
        public static NameplateStyleData data_custom;

        public static Dictionary<PlayerNameplate, CustomNameplateHelper> customNameplateHelpers = new Dictionary<PlayerNameplate, CustomNameplateHelper>();


        private static HarmonyLib.Harmony Instance  = new HarmonyLib.Harmony(Guid.NewGuid().ToString());
        public override void OnApplicationStart() {
            data_original = new NameplateStyleData();
            data_custom = new CustomNameplateStyleData();

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
            try {
                Instance.PatchAll();
            } catch (Exception e) {
                MelonLogger.Msg("Harmony Patching Failed with exception, unpatching!\n" + e.Message);
                Instance.UnpatchSelf();
            }
        }

        private void LoadImages() {
            string imagesDir = "UserData/CustomNameplates/";
            if (debugLogging.EditedValue) MelonLogger.Msg("Loading Images from Data Dir: " + imagesDir);
            CustomNameplatesMod.data_custom.backgroundImage = LoadImage(Path.Combine(imagesDir, "background.png"), new Vector4(255, 0, 255, 0));
            CustomNameplatesMod.data_custom.staffBackgroundImage = LoadImage(Path.Combine(imagesDir, "background.png"), new Vector4(255, 0, 255, 0)); //TODO: add extra image for staff plate
            CustomNameplatesMod.data_custom.profileBackgroundImage = LoadImage(Path.Combine(imagesDir, "profileIcon.png"));
            CustomNameplatesMod.data_custom.micOnImage = LoadImage(Path.Combine(imagesDir, "micOn.png"));
            CustomNameplatesMod.data_custom.micOffImage = LoadImage(Path.Combine(imagesDir, "micOff.png"));
            CustomNameplatesMod.data_custom.friendImage = LoadImage(Path.Combine(imagesDir, "friend.png"));
            Sprite LoadImage(string path, Vector4 border = new Vector4())
            {
                Texture2D tex = new Texture2D(256, 256);
                ImageConversion.LoadImage(tex, File.ReadAllBytes(path));
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 200, 1000u, SpriteMeshType.FullRect, border, false);
            }
        }

        private void CacheImages()
        {
            if (debugLogging.EditedValue) MelonLogger.Msg("Unpacking Images from Mod");
            string imagesDir = "UserData/CustomNameplates/";
            if (!Directory.Exists(imagesDir))
                Directory.CreateDirectory(imagesDir);
            Properties.Resources.background.Save(Path.Combine(imagesDir, "background.png"));
            Properties.Resources.background.Save(Path.Combine(imagesDir, "profileIcon.png"));
            Properties.Resources.micOn.Save(Path.Combine(imagesDir, "micOn.png"));
            Properties.Resources.micOff.Save(Path.Combine(imagesDir, "micOff.png"));
            Properties.Resources.friend.Save(Path.Combine(imagesDir, "friend.png"));
            imagesUnpacked.EditedValue = true;
            if (debugLogging.EditedValue) MelonLogger.Msg("Images Unpacked successfully to " + imagesDir);
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