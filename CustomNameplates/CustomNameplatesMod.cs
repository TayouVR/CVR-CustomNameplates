using System;
using MelonLoader;
using ABI_RC.Core.Player;
using UnityEngine;
using UnityEngine.UI;
using HarmonyLib;
using System.IO;
using Tayou;
using ABI_RC.Core.Networking.IO.Social;

[assembly: MelonInfo(typeof(CustomNameplatesMod), "Custom Nameplates", "1.0.5", "Tayou")]
[assembly: MelonColor(ConsoleColor.Yellow)]
[assembly: MelonGame("Alpha Blend Interactive", "ChilloutVR")]
namespace Tayou {
    public class CustomNameplatesMod : MelonMod {
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

        public static NameplateStyleData data_original;
        public static NameplateStyleData data_custom;


        private static HarmonyLib.Harmony Instance  = new HarmonyLib.Harmony(Guid.NewGuid().ToString());
        public static Transform LocalPlayerTransform { get; set; }
        public override void OnApplicationStart() {
            data_original = new NameplateStyleData();
            data_custom = new CustomNameplateStyleData();

            var category = MelonPreferences.CreateCategory(SettingsCategory, "Custom Nameplates");
            enabled =               category.CreateEntry(SettingEnableMod,              true, "Enabled");
            category.CreateEntry("----- Colors -----", false, "----- Colors -----", true);
            defaultColor =          category.CreateEntry(SettingDefaultColor,           new Color32(75, 75, 75, 150), "Default Color");
            friendsColor =          category.CreateEntry(SettingFriendsColor,           new Color32(153, 153, 0, 50), "Friends Color");
            legendColor =           category.CreateEntry(SettingLegendColor,            new Color32(50, 150, 147, 50), "Legend Color");
            guideColor =            category.CreateEntry(SettingGuideColor,             new Color32(221, 90, 0, 50), "Guide Color");
            talkingDefaultColor =   category.CreateEntry(SettingDefaultColorTalking,    new Color32(100, 100, 100, 200), "Default Color when talking");
            talkingFriendsColor =   category.CreateEntry(SettingFriendsColorTalking,    new Color32(153, 153, 0, 150), "Friends Color when talking");
            talkingLegendColor =    category.CreateEntry(SettingLegendColorTalking,     new Color32(50, 150, 147, 150), "Legend Color when talking");
            talkingGuideColor =     category.CreateEntry(SettingGuideColorTalking,      new Color32(221, 90, 0, 150), "Guide Color when talking");
            category.CreateEntry("----- Misc Settings -----", false, "----- Misc Settings -----", true);
            showFriendIcon =        category.CreateEntry(SettingShowFriendIcon,         false, "Show Friend Icon", "Shows a icon next to the nameplate, that indicates that the player is a friend.");
            showMicIcon =           category.CreateEntry(SettingShowMicIcon,            false, "Show Mic Icon", "[kinda broken]Shows an extra mic icon next to the nameplate, which indicates if the player is talking.");
            noBackground =          category.CreateEntry(SettingNoBackground,           false, "No Background", "Hide Background Panels and color Nameplate Text instead. This might be hard to see in some circumstances.");
            imagesUnpacked =        category.CreateEntry(SettingImagesUnpacked,         false, "Images Cached", "Indicates if the packaged images have been extracted, only set to true if you want to overwrite your custom images, or if you (accidently) removed them.", true);

            if (!imagesUnpacked.EditedValue)
                CacheImages();

            LoadImages();
            Instance.PatchAll();
        }

        private void LoadImages()
        {
            CustomNameplatesMod.data_custom.backgroundImage = LoadImage("UserData/CustomNameplates/" + "background.png", new Vector4(255, 0, 255, 0));
            CustomNameplatesMod.data_custom.staffBackgroundImage = LoadImage("UserData/CustomNameplates/" + "background.png", new Vector4(255, 0, 255, 0)); //TODO: add extra image for staff plate
            CustomNameplatesMod.data_custom.profileBackgroundImage = LoadImage("UserData/CustomNameplates/" + "profileIcon.png");
            CustomNameplatesMod.data_custom.micOnImage = LoadImage("UserData/CustomNameplates/" + "micOn.png");
            CustomNameplatesMod.data_custom.micOffImage = LoadImage("UserData/CustomNameplates/" + "micOff.png");
            CustomNameplatesMod.data_custom.friendImage = LoadImage("UserData/CustomNameplates/" + "friend.png");
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

    [HarmonyPatch]
    class PlayerNameplatePatch {

        static CustomNameplateHelper customNameplateHelper;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerNameplate), nameof(PlayerNameplate.UpdateNamePlate))]
        static void UpdateNamePlatePatch(PlayerNameplate __instance) {
            if (__instance.gameObject.GetComponent<CustomNameplateHelper>() == null) {
                customNameplateHelper = __instance.gameObject.AddComponent<CustomNameplateHelper>();
                customNameplateHelper.PlayerNameplate = __instance;
                customNameplateHelper.Start();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerNameplate), nameof(PlayerNameplate.TalkerState))]
        static void TalkerStatePatch(PlayerNameplate __instance, float amplitude) {
            if (__instance.gameObject.GetComponent<CustomNameplateHelper>() == null) {
                customNameplateHelper = __instance.gameObject.AddComponent<CustomNameplateHelper>();
                customNameplateHelper.PlayerNameplate = __instance;
                customNameplateHelper.Start();
            }

            customNameplateHelper.UpdateNameplate(amplitude);
        }
    }

    public class CustomNameplateHelper : MonoBehaviour {
        public Image micOnImage;
        public Image micOffImage;

        public Image profileIconBG;

        public bool isInitialized = false;

        private PlayerNameplate playerNameplate;

        public PlayerNameplate PlayerNameplate { set => playerNameplate = value; }

        public void Start() {
            SaveOriginalData();
            ToggleModEnabledState(CustomNameplatesMod.enabled.EditedValue);
            
            //CustomNameplatesMod.enabled.OnValueChanged += (a, b) => ToggleModEnabledState(CustomNameplatesMod.enabled.EditedValue);
        }

        private void SaveOriginalData() {
            CustomNameplatesMod.data_original.backgroundImage = playerNameplate.nameplateBackground.sprite;
            CustomNameplatesMod.data_original.backgroundImageScale = playerNameplate.nameplateBackground.transform.localScale;
            CustomNameplatesMod.data_original.backgroundImagePos = playerNameplate.nameplateBackground.transform.localPosition;

            CustomNameplatesMod.data_original.staffBackgroundImage = playerNameplate.staffplateBackground.sprite;
            CustomNameplatesMod.data_original.staffBackgroundImageScale = playerNameplate.staffplateBackground.transform.localScale;
            CustomNameplatesMod.data_original.staffBackgroundImagePos = playerNameplate.staffplateBackground.transform.localPosition;

            Image _maskImage = playerNameplate.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask").GetComponent<Image>();
            CustomNameplatesMod.data_original.profileBackgroundImage = _maskImage.sprite;
            CustomNameplatesMod.data_original.profileBackgroundImageScale = _maskImage.transform.localScale;
            CustomNameplatesMod.data_original.profileBackgroundImagePos = _maskImage.transform.localPosition;

            CustomNameplatesMod.data_original.friendImage = playerNameplate.friendsImage.sprite;
            CustomNameplatesMod.data_original.friendImageScale = playerNameplate.friendsImage.transform.localScale;
            CustomNameplatesMod.data_original.friendImagePos = playerNameplate.friendsImage.transform.localPosition;

        }

        /// <summary>
        /// One time Setup for Setting up nameplate sprites and locations
        /// </summary>
        public void InitilizeNameplate(NameplateStyleData styleData) {
            // patch background image
            playerNameplate.nameplateBackground.sprite = styleData.backgroundImage;
            playerNameplate.nameplateBackground.type = Image.Type.Sliced;
            playerNameplate.nameplateBackground.pixelsPerUnitMultiplier = 500;

            // patch staff part of background image
            playerNameplate.staffplateBackground.sprite = styleData.staffBackgroundImage;
            playerNameplate.staffplateBackground.type = Image.Type.Sliced;
            playerNameplate.staffplateBackground.pixelsPerUnitMultiplier = 500;
            // staff text
            playerNameplate.staffText.transform.localPosition = styleData.staffBackgroundImagePos;

            // patch profile mask & background image
            GameObject _maskGameObject = playerNameplate.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask").gameObject;
            _maskGameObject.GetComponent<Image>().sprite = styleData.profileBackgroundImage;
            _maskGameObject.transform.localScale = styleData.profileBackgroundImageScale;

            profileIconBG = playerNameplate.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask (1)").GetComponent<Image>();
            profileIconBG.sprite = styleData.profileBackgroundImage;
            profileIconBG.color = playerNameplate.nameplateBackground.color;
            profileIconBG.transform.SetSiblingIndex(0); // move up, so that the (prior frame, now background) is behind the image
            profileIconBG.transform.localScale = styleData.profileImageMaskScale;

            // patch friends image / icon
            playerNameplate.friendsImage.sprite = styleData.friendImage;
            playerNameplate.friendsImage.transform.localScale = styleData.friendImageScale;
            playerNameplate.friendsImage.transform.localPosition = styleData.friendImagePos;
            playerNameplate.friendsImage.gameObject.SetActive(false); // initially disable, Update will take care of this

            // set up mic icon
            if ((object)micOffImage == null)
                micOffImage = GameObject.Instantiate(playerNameplate.friendsImage.gameObject, playerNameplate.friendsImage.transform.parent.transform).GetComponent<Image>();
            micOffImage.gameObject.name = "MicOffIndicator";
            micOffImage.sprite = styleData.micOffImage;
            micOffImage.enabled = true;
            micOffImage.transform.localPosition = styleData.micImagePos;
            micOffImage.gameObject.SetActive(false); // initially disable, Update will take care of this

            if ((object)micOnImage == null)
                micOnImage = GameObject.Instantiate(micOffImage, playerNameplate.friendsImage.transform.parent.transform).GetComponent<Image>();
            micOnImage.gameObject.name = "MicOnIndicator";
            micOnImage.sprite = styleData.micOnImage;
            micOnImage.transform.localPosition = styleData.micImagePos;
            micOnImage.gameObject.SetActive(false); // initially disable, Update will take care of this

            isInitialized = true;
        }

        /// <summary>
        /// Update Function for namplate, called every frame via PlayerNameplate.TalkerState();
        /// </summary>
        public void UpdateNameplate(float amplitude) {

            // wait for InitializeNameplate to finish
            if (!isInitialized)
                return;

            bool flag = amplitude > 0f;
            GetNameplateColor(out Color32 backgroundColor, out Color32 textColor, flag);

            playerNameplate.friendsImage.gameObject.SetActive(CustomNameplatesMod.showFriendIcon.EditedValue);
            SetMicImage(flag && CustomNameplatesMod.showMicIcon.EditedValue);

            playerNameplate.transform.Find("Canvas/Content/TMP:Username").gameObject.GetComponent<TMPro.TextMeshProUGUI>().color = textColor;

            playerNameplate.nameplateBackground.color = backgroundColor;
            playerNameplate.staffplateBackground.color = backgroundColor;
            profileIconBG.color = backgroundColor;
            //MelonLogger.Msg("\n should be Color: " + backgroundColor.ToString() + "\n is Color: " + playerNameplate.nameplateBackground.color);
        }

        public void GetNameplateColor(out Color32 backgroundColor, out Color32 textColor, bool talking = false) {

            // get default color, which was defined in original TalkerState()
            Color32 UserColor = playerNameplate.nameplateBackground.color;

            bool isFriend = Friends.FriendsWith(playerNameplate.player.ownerId);

            if (!CustomNameplatesMod.noBackground.EditedValue) {
                if (isFriend) {
                    backgroundColor = (talking ? CustomNameplatesMod.talkingFriendsColor.EditedValue : CustomNameplatesMod.friendsColor.EditedValue);
                } else {
                    switch (playerNameplate.player.userRank) {
                        default:
                            backgroundColor = (talking ? CustomNameplatesMod.talkingDefaultColor.EditedValue : CustomNameplatesMod.defaultColor.EditedValue);
                            break;
                        case "Legend":
                            backgroundColor = (talking ? CustomNameplatesMod.talkingLegendColor.EditedValue : CustomNameplatesMod.legendColor.EditedValue);
                            break;
                        case "Community Guide":
                            backgroundColor = (talking ? CustomNameplatesMod.talkingGuideColor.EditedValue : CustomNameplatesMod.guideColor.EditedValue);
                            break;
                        case "Moderator":
                            backgroundColor = UserColor; // Don't overwrite, keep color from OG method -- UserColor = (talking ? new Color32(221, 0, 118, 150) : new Color32(221, 0, 118, 50));
                            break;
                        case "Developer":
                            backgroundColor = UserColor; // Don't overwrite, keep color from OG method -- UserColor = (talking ? new Color32(240, 0, 40, 150) : new Color32(240, 0, 40, 50));
                            break;
                    }
                }
                textColor = (Color32)Color.white;
            } else {
                backgroundColor = new Color32(0, 0, 0, 0);
                if (isFriend) {
                    textColor = (talking ? CustomNameplatesMod.talkingFriendsColor.EditedValue : CustomNameplatesMod.friendsColor.EditedValue);
                } else {
                    switch (playerNameplate.player.userRank) {
                        default:
                            textColor = (talking ? new Color32(255, 255, 255, 255) : new Color32(210, 210, 210, 210));
                            break;
                        case "Legend":
                            textColor = (talking ? CustomNameplatesMod.talkingLegendColor.EditedValue : CustomNameplatesMod.legendColor.EditedValue);
                            break;
                        case "Community Guide":
                            textColor = (talking ? CustomNameplatesMod.talkingGuideColor.EditedValue : CustomNameplatesMod.guideColor.EditedValue);
                            break;
                        case "Moderator":
                            textColor = UserColor; // Don't overwrite, keep color from OG method -- UserColor = (talking ? new Color32(221, 0, 118, 150) : new Color32(221, 0, 118, 50));
                            break;
                        case "Developer":
                            textColor = UserColor; // Don't overwrite, keep color from OG method -- UserColor = (talking ? new Color32(240, 0, 40, 150) : new Color32(240, 0, 40, 50));
                            break;
                    }
                }
            }
        }

        public void SetMicImage(bool state) {
            if (CustomNameplatesMod.showMicIcon.EditedValue) {
                micOnImage.gameObject.SetActive(state);
                micOffImage.gameObject.SetActive(!state);
            } else {
                micOffImage.gameObject.SetActive(false);
                micOnImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Toggle Mod Enabled State
        /// </summary>
        /// <param name="enabled">whether or not the mod is enabled (set custom colors & sprites, reset to default)</param>
        public void ToggleModEnabledState(bool enabled) {
            if (enabled) {
                InitilizeNameplate(CustomNameplatesMod.data_custom);
            } else {
                InitilizeNameplate(CustomNameplatesMod.data_original);
            }
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