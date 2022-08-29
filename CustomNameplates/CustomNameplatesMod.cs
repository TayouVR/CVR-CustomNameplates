using System;
using MelonLoader;
using ABI_RC.Core.Player;
using ABI_RC.Core;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using HarmonyLib;
using System.IO;
using Tayou;
using ABI_RC.Core.Networking.IO.Social;

namespace Tayou
{
    public class CustomNameplatesMod : MelonMod
    {
        private const string SettingsCategory = "CustomNameplates";
        private const string SettingEnableMod = "Enabled";
        private const string SettingDefaultColor = "DefaultColor";
        private const string SettingFriendsColor = "FriendsColor";
        private const string SettingLegendColor = "LegendColor";
        private const string SettingGuideColor = "GuideColor";
        private const string SettingDefaultColorTalking = "TalkingDefaultColor";
        private const string SettingFriendsColorTalking = "TalkingFriendsColor";
        private const string SettingLegendColorTalking = "TalkingLegendColor";
        private const string SettingGuideColorTalking = "TalkingGuideColor";
        private const string SettingShowFriendIcon = "ShowFriendIcon";
        private const string SettingShowMicIcon = "ShowMicIcon";
        private const string SettingNoBackground = "NoBackground";
        private const string SettingImagesUnpacked = "ImagesUnpacked";

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
        public static Sprite backgroundImage;
        public static Sprite profileBackgroundImage;
        public static Sprite micOnImage;
        public static Sprite micOffImage;
        public static Sprite friendImage;


        private static HarmonyLib.Harmony Instance  = new HarmonyLib.Harmony(Guid.NewGuid().ToString());
        public static Transform LocalPlayerTransform { get; set; }
        public override void OnApplicationStart()
        {

            var category = MelonPreferences.CreateCategory(SettingsCategory, "Custom Nameplates");
            enabled =               category.CreateEntry(SettingEnableMod,              true, "Enabled");
            defaultColor =          category.CreateEntry(SettingDefaultColor,           new Color32(75, 75, 75, 150), "Default Color");
            friendsColor =          category.CreateEntry(SettingFriendsColor,           new Color32(153, 153, 0, 50), "Friends Color");
            legendColor =           category.CreateEntry(SettingLegendColor,            new Color32(50, 150, 147, 50), "Legend Color");
            guideColor =            category.CreateEntry(SettingGuideColor,             new Color32(221, 90, 0, 50), "Guide Color");
            talkingDefaultColor =   category.CreateEntry(SettingDefaultColorTalking,    new Color32(100, 100, 100, 200), "Default Color when talking");
            talkingFriendsColor =   category.CreateEntry(SettingFriendsColorTalking,    new Color32(153, 153, 0, 150), "Friends Color when talking");
            talkingLegendColor =    category.CreateEntry(SettingLegendColorTalking,     new Color32(50, 150, 147, 150), "Legend Color when talking");
            talkingGuideColor =     category.CreateEntry(SettingGuideColorTalking,      new Color32(221, 90, 0, 150), "Guide Color when talking");
            showFriendIcon =        category.CreateEntry(SettingShowFriendIcon,         false, "Show Friend Icon", "Shows a icon next to the nameplate, that indicates that the player is a friend");
            showMicIcon =           category.CreateEntry(SettingShowMicIcon,            false, "Show Mic Icon", "[kinda broken]Shows an extra mic icon next to the nameplate, which indicates if the player is talking");
            noBackground =          category.CreateEntry(SettingNoBackground,           false, "No Background", "This doesn't brighten colors, so some names would be hard to see");
            imagesUnpacked =        category.CreateEntry(SettingImagesUnpacked,         false, "Images Cached", "Indicates if the packaged images have been extracted, only set to true if you want to overwrite your custom images, or if you (accidently) removed them", true);

            if (!imagesUnpacked.EditedValue)
                CacheImages();

            LoadImages();
            Instance.PatchAll();
        }

        private void LoadImages()
        {
            backgroundImage = LoadImage("UserData/CustomNameplates/" + "background.png", new Vector4(255, 0, 255, 0));
            profileBackgroundImage = LoadImage("UserData/CustomNameplates/" + "profileIcon.png");
            micOnImage = LoadImage("UserData/CustomNameplates/" + "micOn.png");
            micOffImage = LoadImage("UserData/CustomNameplates/" + "micOff.png");
            friendImage = LoadImage("UserData/CustomNameplates/" + "friend.png");
            Sprite LoadImage(string path, Vector4 border = new Vector4())
            {
                Texture2D tex = new Texture2D(256, 256);
                ImageConversion.LoadImage(tex, File.ReadAllBytes(path));
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 200, 1000u, SpriteMeshType.FullRect, border, false);
            }
        }

        private void CacheImages()
        {
            if (!Directory.Exists("UserData/CustomNameplates"))
                Directory.CreateDirectory("UserData/CustomNameplates");
            Properties.Resources.background.Save("UserData/CustomNameplates/" + "background.png");
            Properties.Resources.background.Save("UserData/CustomNameplates/" + "profileIcon.png");
            Properties.Resources.micOn.Save("UserData/CustomNameplates/" + "micOn.png");
            Properties.Resources.micOff.Save("UserData/CustomNameplates/" + "micOff.png");
            Properties.Resources.friend.Save("UserData/CustomNameplates/" + "friend.png");
            imagesUnpacked.EditedValue = true;
        }
    }
}

[HarmonyPatch]
class PlayerNameplatePatch
{

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerNameplate), nameof(PlayerNameplate.UpdateNamePlate))]
    static void UpdateNamePlatePatch(PlayerNameplate __instance)
    {
        CustomNameplateHelper customNameplateHelper = null;
        if (__instance.gameObject.GetComponent<CustomNameplateHelper>() == null)
            customNameplateHelper = __instance.gameObject.AddComponent<CustomNameplateHelper>();
        // calling this to get custom colors to apply initially without duping code
        // I'm not sure when TalkerState() is called by default, so I'm just doing this to be safe.
        __instance.TalkerState(0);




        // patch background image
        __instance.nameplateBackground.sprite = CustomNameplatesMod.backgroundImage;
        __instance.nameplateBackground.type = Image.Type.Sliced;
        __instance.nameplateBackground.pixelsPerUnitMultiplier = 500;

        // patch staff part of background image
        __instance.staffplateBackground.sprite = CustomNameplatesMod.backgroundImage;
        __instance.staffplateBackground.type = Image.Type.Sliced;
        __instance.staffplateBackground.pixelsPerUnitMultiplier = 500;
        // staff text
        __instance.staffText.transform.localPosition = new Vector3(-0.08f, 0.2902f, 0f);

        // patch profile mask & background image
        GameObject _maskGameObject = __instance.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask").gameObject;
        _maskGameObject.GetComponent<Image>().sprite = CustomNameplatesMod.profileBackgroundImage;
        _maskGameObject.transform.localScale = new Vector3(1.25f, 1.1f, 1);

        Image _backgroundGameObj = __instance.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask (1)").GetComponent<Image>();
        _backgroundGameObj.sprite = CustomNameplatesMod.profileBackgroundImage;
        _backgroundGameObj.color = __instance.nameplateBackground.color;
        _backgroundGameObj.transform.SetSiblingIndex(0); // move up, so that the (prior frame, now background) is behind the image
        _backgroundGameObj.transform.localScale = new Vector3(1.45f, 1.25f, 1);

        // patch friends image / icon
        if (CustomNameplatesMod.showFriendIcon.EditedValue) {
            __instance.friendsImage.sprite = CustomNameplatesMod.friendImage;
            __instance.friendsImage.transform.localScale = new Vector3(0.9f, 0.6f, 1);
            __instance.friendsImage.transform.localPosition = new Vector3(0.60f, 0.39f, 0);
        } else {
            __instance.friendsImage.gameObject.SetActive(false);
        }

        // set up mic icon
        if (CustomNameplatesMod.showMicIcon.EditedValue && customNameplateHelper != null)
        {
            customNameplateHelper.micOffImage = GameObject.Instantiate(__instance.friendsImage.gameObject, __instance.friendsImage.transform.parent.transform).GetComponent<Image>();
            customNameplateHelper.micOffImage.gameObject.name = "MicOffIndicator";
            customNameplateHelper.micOffImage.sprite = CustomNameplatesMod.micOffImage;
            customNameplateHelper.micOffImage.enabled = true;
            customNameplateHelper.micOffImage.transform.localPosition = new Vector3(0.944f, 0.39f, 0);

            customNameplateHelper.micOnImage = GameObject.Instantiate(customNameplateHelper.micOffImage, __instance.friendsImage.transform.parent.transform).GetComponent<Image>();
            customNameplateHelper.micOnImage.gameObject.name = "MicOnIndicator";
            customNameplateHelper.micOnImage.sprite = CustomNameplatesMod.micOnImage;
            customNameplateHelper.micOnImage.transform.localPosition = new Vector3(0.944f, 0.39f, 0);
            customNameplateHelper.micOnImage.gameObject.SetActive(false);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerNameplate), nameof(PlayerNameplate.TalkerState))]
    static void TalkerStatePatch(PlayerNameplate __instance, float amplitude)
    {
        Color32 UserColor = __instance.nameplateBackground.color;
        bool flag = amplitude > 0f;

        bool isFriend = Friends.FriendsWith(__instance.player.ownerId);
        if (isFriend) {
            UserColor = (flag ? CustomNameplatesMod.talkingFriendsColor.EditedValue : CustomNameplatesMod.friendsColor.EditedValue);
        } else {
            switch (__instance.player.userRank)
            {
                default:
                    UserColor = (flag ? CustomNameplatesMod.talkingDefaultColor.EditedValue : CustomNameplatesMod.defaultColor.EditedValue);
                    break;
                case "Legend":
                    UserColor = (flag ? CustomNameplatesMod.talkingLegendColor.EditedValue : CustomNameplatesMod.legendColor.EditedValue);
                    break;
                case "Community Guide":
                    UserColor = (flag ? CustomNameplatesMod.talkingGuideColor.EditedValue : CustomNameplatesMod.guideColor.EditedValue);
                    break;
                case "Moderator":
                    // Don't overwrite, keep color from OG method -- UserColor = (flag ? new Color32(221, 0, 118, 150) : new Color32(221, 0, 118, 50));
                    break;
                case "Developer":
                    // Don't overwrite, keep color from OG method -- UserColor = (flag ? new Color32(240, 0, 40, 150) : new Color32(240, 0, 40, 50));
                    break;
            }
        }
        if (CustomNameplatesMod.showMicIcon.EditedValue) {
            __instance.GetComponent<CustomNameplateHelper>().SetMicImage(flag);
        }

        if (CustomNameplatesMod.noBackground.EditedValue)
        {
            __instance.transform.Find("Canvas/Content/TMP:Username").gameObject.GetComponent<TMPro.TextMeshProUGUI>().color = (
                __instance.player.userRank == "Developer" ||
                __instance.player.userRank == "Moderator" ||
                __instance.player.userRank == "Community Guide" ||
                __instance.player.userRank == "Legend" ||
                isFriend) ? UserColor : (Color32)Color.white;
            UserColor = new Color32(0, 0, 0, 0);
        }

        __instance.nameplateBackground.color = UserColor;
        __instance.staffplateBackground.color = UserColor;
    }
}

public class CustomNameplateHelper : MonoBehaviour {
    public Image micOnImage;
    public Image micOffImage;

    public void SetMicImage(bool state) {
        if (micOnImage != null && micOffImage != null) {
            micOnImage.gameObject.SetActive(state);
            micOffImage.gameObject.SetActive(!state);
        }
    }
}