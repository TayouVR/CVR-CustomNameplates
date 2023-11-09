using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Tayou.CustomNameplateMod {
    public class Helper : MonoBehaviour {
        public Image micOnImage;
        public Image micOffImage;

        public Image profileIconBackground;

        public bool isInitialized;

        private PlayerNameplate playerNameplate;

        public void Init(PlayerNameplate playerNameplateInstance) {
            playerNameplate = playerNameplateInstance;
            Mod.ConsoleLog("Patching Nameplate for " + playerNameplate.player.userName + "[" + playerNameplate.player.name + "] Role: " + (Friends.FriendsWith(playerNameplate.player.ownerId) ? "Friend" : playerNameplate.player.userRank), true);
            SaveOriginalData();
            ToggleModEnabledState(Mod.enabled.EditedValue);
            
            Mod.enabled.OnValueChanged += (a, b) => ToggleModEnabledState(Mod.enabled.EditedValue);
        }

        public void OnDestroy() {
            Mod.CustomNameplateHelpers.Remove(playerNameplate);
        }

        private void SaveOriginalData() {
            Mod.DataOriginal.backgroundImage = playerNameplate.nameplateBackground.sprite;
            Mod.DataOriginal.backgroundImageScale = playerNameplate.nameplateBackground.transform.localScale;
            Mod.DataOriginal.backgroundImagePos = playerNameplate.nameplateBackground.transform.localPosition;

            Mod.DataOriginal.staffBackgroundImage = playerNameplate.staffplateBackground.sprite;
            Mod.DataOriginal.staffBackgroundImageScale = playerNameplate.staffplateBackground.transform.localScale;
            Mod.DataOriginal.staffBackgroundImagePos = playerNameplate.staffplateBackground.transform.localPosition;

            Image maskImage = playerNameplate.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask").GetComponent<Image>();
            Mod.DataOriginal.profileBackgroundImage = maskImage.sprite;
            Mod.DataOriginal.profileBackgroundImageScale = maskImage.transform.localScale;
            Mod.DataOriginal.profileBackgroundImagePos = maskImage.transform.localPosition;

            Mod.DataOriginal.friendImage = playerNameplate.friendsImage.sprite;
            Mod.DataOriginal.friendImageScale = playerNameplate.friendsImage.transform.localScale;
            Mod.DataOriginal.friendImagePos = playerNameplate.friendsImage.transform.localPosition;

        }

        /// <summary>
        /// One time Setup for Setting up nameplate sprites and locations
        /// </summary>
        public void InitializeNameplate(NameplateStyleData styleData) {
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
            GameObject maskGameObject = playerNameplate.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask").gameObject;
            maskGameObject.GetComponent<Image>().sprite = styleData.profileBackgroundImage;
            maskGameObject.transform.localScale = styleData.profileBackgroundImageScale;

            profileIconBackground = playerNameplate.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask (1)").GetComponent<Image>();
            profileIconBackground.sprite = styleData.profileBackgroundImage;
            profileIconBackground.color = playerNameplate.nameplateBackground.color;
            profileIconBackground.transform.SetSiblingIndex(0); // move up, so that the (prior frame, now background) is behind the image
            profileIconBackground.transform.localScale = styleData.profileImageMaskScale;

            // patch friends image / icon
            playerNameplate.friendsImage.sprite = styleData.friendImage;
            playerNameplate.friendsImage.transform.localScale = styleData.friendImageScale;
            playerNameplate.friendsImage.transform.localPosition = styleData.friendImagePos;
            playerNameplate.friendsImage.gameObject.SetActive(false); // initially disable, Update will take care of this

            // set up mic icon
            if ((object)micOffImage == null)
                micOffImage = Instantiate(playerNameplate.friendsImage.gameObject, playerNameplate.friendsImage.transform.parent.transform).GetComponent<Image>();
            micOffImage.gameObject.name = "MicOffIndicator";
            micOffImage.sprite = styleData.micOffImage;
            micOffImage.enabled = true;
            micOffImage.transform.localPosition = styleData.micImagePos;
            micOffImage.gameObject.SetActive(false); // initially disable, Update will take care of this

            if ((object)micOnImage == null)
                micOnImage = Instantiate(micOffImage, playerNameplate.friendsImage.transform.parent.transform).GetComponent<Image>();
            micOnImage.gameObject.name = "MicOnIndicator";
            micOnImage.sprite = styleData.micOnImage;
            micOnImage.transform.localPosition = styleData.micImagePos;
            micOnImage.gameObject.SetActive(false); // initially disable, Update will take care of this

            isInitialized = true;
        }

        /// <summary>
        /// Update Function for nameplate, called every frame via PlayerNameplate.UpdateNamePlate();
        /// </summary>
        public void UpdateNameplate() {
            // wait for InitializeNameplate to finish
            if (!isInitialized)
                return;

            GetNameplateColor(out Color32 backgroundColor, out Color32 textColor, playerNameplate.wasTalking);

            playerNameplate.friendsImage.gameObject.SetActive(Mod.showFriendIcon.EditedValue);
            SetMicImage(playerNameplate.wasTalking && Mod.showMicIcon.EditedValue);

            playerNameplate.transform.Find("Canvas/Content/TMP:Username").gameObject.GetComponent<TextMeshProUGUI>().color = textColor;

            playerNameplate.nameplateBackground.color = backgroundColor;
            playerNameplate.staffplateBackground.color = backgroundColor;
            profileIconBackground.color = backgroundColor;
            //MelonLogger.Msg("\n should be Color: " + backgroundColor.ToString() + "\n is Color: " + playerNameplate.nameplateBackground.color);
        }

        public void GetNameplateColor(out Color32 backgroundColor, out Color32 textColor, bool isTalking) {

            // get default color, which was defined in original UpdateNamePlateColor()
            Color32 originalColor = playerNameplate.nameplateBackground.color;

            Color32 nameplateColor;
            if (Friends.FriendsWith(playerNameplate.player.ownerId)) {
                nameplateColor = isTalking ? Mod.talkingFriendsColor.EditedValue : Mod.friendsColor.EditedValue;
            } else {
                switch (playerNameplate.player.userRank) {
                    default:
                        nameplateColor = isTalking ? Mod.talkingDefaultColor.EditedValue : Mod.defaultColor.EditedValue;
                        break;
                    case "Legend":
                        nameplateColor = isTalking ? Mod.talkingLegendColor.EditedValue : Mod.legendColor.EditedValue;
                        break;
                    case "Community Guide":
                        nameplateColor = isTalking ? Mod.talkingGuideColor.EditedValue : Mod.guideColor.EditedValue;
                        break;
                    case "Moderator":
                        nameplateColor = originalColor; // Don't overwrite, keep color from OG method -- nameplateColor = isTalking ? new Color32(221, 0, 118, 150) : new Color32(221, 0, 118, 50);
                        break;
                    case "Developer":
                        nameplateColor = originalColor; // Don't overwrite, keep color from OG method -- nameplateColor = isTalking ? new Color32(240, 0, 40, 150) : new Color32(240, 0, 40, 50);
                        break;
                }
            }

            backgroundColor = Mod.noBackground.EditedValue ? new Color32(0, 0, 0, 0) : nameplateColor;
            textColor = Mod.noBackground.EditedValue ? nameplateColor : (Color32)Color.white;
        }

        public void SetMicImage(bool state) {
            if (Mod.showMicIcon.EditedValue) {
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
        /// <param name="modEnabled">whether or not the mod is enabled (set custom colors & sprites, reset to default)</param>
        public void ToggleModEnabledState(bool modEnabled) {
            InitializeNameplate(modEnabled ? Mod.DataCustom : Mod.DataOriginal);
        }
    }
}