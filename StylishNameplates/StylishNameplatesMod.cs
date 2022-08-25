using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using ABI_RC.Core.Player;
using System.Reflection;
using ABI_RC.Core;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using HarmonyLib;
using System.Net;
using System.IO;

namespace Tayou
{
    public class StylishNameplatesMod : MelonMod
    {
        private const string SettingsCategory = "StylishNameplates";
        private const string SettingEnableMod = "Enabled";
        private const string SettingDefaultColor = "DefaultColor";
        private const string SettingFriendsColor = "FriendsColor";
        private const string SettingLegendColor = "LegendColor";
        private const string SettingGuideColor = "GuideColor";
        private const string SettingModeratorColor = "ModeratorColor";
        private const string SettingDeveloperColor = "DeveloperColor";
        private const string SettingImagesCached = "ImagesCached";

        public static MelonPreferences_Entry<bool> ourEnabled;
        public static MelonPreferences_Entry<Color> ourDefaultColor;
        public static MelonPreferences_Entry<Color> ourFriendsColor;
        public static MelonPreferences_Entry<Color> ourLegendColor;
        public static MelonPreferences_Entry<Color> ourGuideColor;
        public static MelonPreferences_Entry<Color> ourModeratorColor;
        public static MelonPreferences_Entry<Color> ourDeveloperColor;
        public static MelonPreferences_Entry<bool> imagesCached;
        public static Sprite backgroundImage;
        public static Sprite profileBackgroundImage;
        public static Sprite micOnImage;
        public static Sprite micOffImage;
        public static Sprite friendImage;


        private static HarmonyLib.Harmony Instance  = new HarmonyLib.Harmony(Guid.NewGuid().ToString());
        public static Transform LocalPlayerTransform { get; set; }
        public override void OnApplicationStart()
        {

            var category = MelonPreferences.CreateCategory(SettingsCategory, "Stylish Nameplates");
            ourEnabled =           category.CreateEntry(SettingEnableMod,      true, "Enabled");
            ourDefaultColor =      category.CreateEntry(SettingDefaultColor,   new Color(0.3f, 0.3f, 0.3f, 0.8f), "Default Color");
            ourFriendsColor =      category.CreateEntry(SettingFriendsColor,   new Color(1f, 1f, 0f, 0.8f), "Friends Color");
            ourLegendColor =       category.CreateEntry(SettingLegendColor,    new Color(0.5f, 0.5f, 0.125f, 0.8f), "Legend Color");
            ourGuideColor =        category.CreateEntry(SettingGuideColor,     new Color(1f, 0.3f, 0f, 0.8f), "Guide Color");
            ourModeratorColor =    category.CreateEntry(SettingModeratorColor, new Color(0.5f, 0f, 0f, 0.8f), "Moderator Color");
            ourDeveloperColor =    category.CreateEntry(SettingDeveloperColor, new Color(1f, 0f, 0f, 0.8f), "Developer Color");
            imagesCached =         category.CreateEntry(SettingImagesCached,   false, "Images Cached");

            if (!imagesCached.EditedValue)
                CacheImages();

            LoadImages();
            HPatch();
            MelonCoroutines.Start(WaitForUi());
        }

        private void LoadImages()
        {
            Texture2D tex = new Texture2D(256, 256);
            ImageConversion.LoadImage(tex, File.ReadAllBytes("UserData/CustomNameplates/" + "background.png"));
            backgroundImage = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 200, 1000u, SpriteMeshType.FullRect, new Vector4(255, 0, 255, 0), false);
            ImageConversion.LoadImage(tex, File.ReadAllBytes("UserData/CustomNameplates/" + "profileIcon.png"));
            profileBackgroundImage = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 0, 1000u, SpriteMeshType.FullRect, new Vector4(0, 0, 0, 0), false);
            ImageConversion.LoadImage(tex, File.ReadAllBytes("UserData/CustomNameplates/" + "micOn.png"));
            micOnImage = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 0, 1000u, SpriteMeshType.FullRect, new Vector4(0, 0, 0, 0), false);
            ImageConversion.LoadImage(tex, File.ReadAllBytes("UserData/CustomNameplates/" + "micOff.png"));
            micOffImage = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 0, 1000u, SpriteMeshType.FullRect, new Vector4(0, 0, 0, 0), false);
            ImageConversion.LoadImage(tex, File.ReadAllBytes("UserData/CustomNameplates/" + "friend.png"));
            friendImage = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0), 0, 1000u, SpriteMeshType.FullRect, new Vector4(0, 0, 0, 0), false);
        }

        private void CacheImages()
        {
            if (!Directory.Exists("UserData/CustomNameplates"))
                Directory.CreateDirectory("UserData/CustomNameplates");
            using (WebClient wc = new WebClient()) {
                wc.DownloadFile("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/nameplate.png",  "UserData/CustomNameplates/" + "background.png");
                wc.DownloadFile("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/nameplate.png",  "UserData/CustomNameplates/" + "profileIcon.png");
                wc.DownloadFile("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/micon.png",      "UserData/CustomNameplates/" + "micOn.png");
                wc.DownloadFile("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/micoff.png",     "UserData/CustomNameplates/" + "micOff.png");
                wc.DownloadFile("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/friendIcon.png", "UserData/CustomNameplates/" + "friend.png");
            }
            imagesCached.EditedValue = true;
        }

        private IEnumerator WaitForUi()
        {
            while (RootLogic.Instance == null) yield return new WaitForSeconds(1f);
            RootLogic.Instance.comms.OnPlayerStartedSpeaking += PlayerTalking;
            RootLogic.Instance.comms.OnPlayerStoppedSpeaking += PlayerStopTalking;


            LocalPlayerTransform = RootLogic.Instance.localPlayerRoot.transform;
            yield break;
        }

        private NamePlateHandler _handler { get; set; }

        private void PlayerStopTalking(Dissonance.VoicePlayerState obj)
        {      
            _handler = GameObject.Find($"/{obj.Name}").transform.Find("[NamePlate]").gameObject.GetComponent<NamePlateHandler>();
            _handler.BackgroundMask.color = new Color(_handler.UserColor.r, _handler.UserColor.g, _handler.UserColor.b, 0.4f);
            _handler.BackgroundImageComp.color = new Color(_handler.UserColor.r, _handler.UserColor.g, _handler.UserColor.b, 0.4f);
            _handler.MicOn.SetActive(false);
            _handler.MicOff.SetActive(true);
        }
        private void PlayerTalking(Dissonance.VoicePlayerState obj)
        {
            _handler = GameObject.Find($"/{obj.Name}").transform.Find("[NamePlate]").gameObject.GetComponent<NamePlateHandler>();
            _handler.MicOn.SetActive(true);
            _handler.MicOff.SetActive(false);
            if (obj.Amplitude > 0.1f) return;
            _handler.BackgroundMask.color = new Color(_handler.UserColor.r, _handler.UserColor.g, _handler.UserColor.b, 1f);
            _handler.BackgroundImageComp.color = new Color(_handler.UserColor.r, _handler.UserColor.g, _handler.UserColor.b, 1f);
        }

        private static void HPatch() =>
            Instance.Patch(typeof(PlayerNameplate).GetMethod(nameof(PlayerNameplate.UpdateNamePlate)),null, typeof(StylishNameplatesMod).GetMethod(nameof(PostFix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).ToNewHarmonyMethod());
        
        private static void PostFix(PlayerNameplate __instance) =>       
            __instance.gameObject.AddComponent<NamePlateHandler>();
        
    }
}
