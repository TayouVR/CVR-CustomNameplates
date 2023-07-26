using ABI_RC.Core.Player;
using UnityEngine;
using UnityEngine.UI;
using ABI_RC.Core.Networking.IO.Social;
using ABI_RC.Core.Vivox.Components;
using MelonLoader;

namespace Tayou {
    public class CustomNameplateHelper : MonoBehaviour {
        public Image micOnImage;
        public Image micOffImage;

        public Image profileIconBG;

        public bool isInitialized = false;

        private PlayerNameplate playerNameplate;

        public void Init(PlayerNameplate playerNameplateInstance) {
            playerNameplate = playerNameplateInstance;
            if (CustomNameplatesMod.debugLogging.EditedValue) MelonLogger.Msg("Patching Nameplate for " + playerNameplate.player.userName + "[" + playerNameplate.player.name + "] Role: " + (Friends.FriendsWith(playerNameplate.player.ownerId) ? "Friend" : playerNameplate.player.userRank));
            SaveOriginalData();
            ToggleModEnabledState(CustomNameplatesMod.enabled.EditedValue);
            
            CustomNameplatesMod.enabled.OnValueChanged += (a, b) => ToggleModEnabledState(CustomNameplatesMod.enabled.EditedValue);
        }

        public void OnDestroy() {
            CustomNameplatesMod.customNameplateHelpers.Remove(playerNameplate);
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
        /// Update Function for nameplate, called every frame via PlayerNameplate.UpdateNamePlate();
        /// </summary>
        public void UpdateNameplate() {
            // wait for InitializeNameplate to finish
            if (!isInitialized)
                return;
            
            bool isTalking = playerNameplate._tracker != null && 
                             (VivoxAudioPipeline)CustomNameplatesMod.GetProperty(playerNameplate._tracker, "pipeline") != null && 
                             ((VivoxAudioPipeline)CustomNameplatesMod.GetProperty(playerNameplate._tracker, "pipeline")).IsActiveSmooth;

            GetNameplateColor(out Color32 backgroundColor, out Color32 textColor, isTalking);

            playerNameplate.friendsImage.gameObject.SetActive(CustomNameplatesMod.showFriendIcon.EditedValue);
            SetMicImage(isTalking && CustomNameplatesMod.showMicIcon.EditedValue);

            playerNameplate.transform.Find("Canvas/Content/TMP:Username").gameObject.GetComponent<TMPro.TextMeshProUGUI>().color = textColor;

            playerNameplate.nameplateBackground.color = backgroundColor;
            playerNameplate.staffplateBackground.color = backgroundColor;
            profileIconBG.color = backgroundColor;
            //MelonLogger.Msg("\n should be Color: " + backgroundColor.ToString() + "\n is Color: " + playerNameplate.nameplateBackground.color);
        }

        public void GetNameplateColor(out Color32 backgroundColor, out Color32 textColor, bool isTalking) {

            // get default color, which was defined in original UpdateNamePlateColor()
            Color32 originalColor = playerNameplate.nameplateBackground.color;

            Color32 nameplateColor;
            if (Friends.FriendsWith(playerNameplate.player.ownerId)) {
                nameplateColor = (isTalking ? CustomNameplatesMod.talkingFriendsColor.EditedValue : CustomNameplatesMod.friendsColor.EditedValue);
            } else {
                switch (playerNameplate.player.userRank) {
                    default:
                        nameplateColor = (isTalking ? CustomNameplatesMod.talkingDefaultColor.EditedValue : CustomNameplatesMod.defaultColor.EditedValue);
                        break;
                    case "Legend":
                        nameplateColor = (isTalking ? CustomNameplatesMod.talkingLegendColor.EditedValue : CustomNameplatesMod.legendColor.EditedValue);
                        break;
                    case "Community Guide":
                        nameplateColor = (isTalking ? CustomNameplatesMod.talkingGuideColor.EditedValue : CustomNameplatesMod.guideColor.EditedValue);
                        break;
                    case "Moderator":
                        nameplateColor = originalColor; // Don't overwrite, keep color from OG method -- nameplateColor = (playerNameplate.player.IsTalking ? new Color32(221, 0, 118, 150) : new Color32(221, 0, 118, 50));
                        break;
                    case "Developer":
                        nameplateColor = originalColor; // Don't overwrite, keep color from OG method -- nameplateColor = (playerNameplate.player.IsTalking ? new Color32(240, 0, 40, 150) : new Color32(240, 0, 40, 50));
                        break;
                }
            }

            backgroundColor = CustomNameplatesMod.noBackground.EditedValue ? new Color32(0, 0, 0, 0) : nameplateColor;
            textColor = CustomNameplatesMod.noBackground.EditedValue ? nameplateColor : (Color32)Color.white;
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
}