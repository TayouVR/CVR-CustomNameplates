using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ABI_RC.Core.Networking.IO.Social;
using MelonLoader;
using UnityEngine.UI;

namespace Tayou
{
    internal class NamePlateHandler : MonoBehaviour, IDisposable
    {
        private GameObject _maskGameObject { get; set; }
        private UnityEngine.UI.Image _maskImageComp { get; set; }
        private GameObject _backgroundGameObject { get; set; }
        public UnityEngine.UI.Image BackgroundImageComp { get; set; }
        public UnityEngine.UI.Image BackgroundMask { get; set; }
        private GameObject _backgroundGameObj{ get; set; }
        private GameObject _friendIcon { get; set; }
        public GameObject MicOn { get; set; }
        public GameObject MicOff { get; set; }
        public Color UserColor { get; set; }
        private Color UserColorMaxAlpha { get; set; }

        private UnityEngine.UI.Image _micOffImage{ get; set; }
        private UnityEngine.UI.Image _micOnImage { get; set; }
        private UnityEngine.UI.Image _friend { get; set; }
        
        private Transform _canvas { get; set; }

        void Start() => InvokeRepeating(nameof(Setup), -1, 0.3f);
        private void Setup()
        {
            try
            {
                if (this.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask (1)") == null) return;
            }
            catch { return; }

            _canvas = this.transform.Find("Canvas").transform;
            _canvas.localScale = new Vector3(0.45f, 0.45f, 1);
            if (ABI_RC.Core.InteractionSystem.ViewManager.Instance.FriendList.FirstOrDefault(x => x.UserId == this.transform.parent.gameObject.name) != null)
                UserColor = StylishNameplatesMod.ourFriendsColor.EditedValue;

            else
            {
                switch (this.transform.parent.gameObject.GetComponent<ABI_RC.Core.Player.PlayerDescriptor>().userRank)
                {
                    case "Legend":
                        UserColor = StylishNameplatesMod.ourLegendColor.EditedValue;
                        break;
                    case "Community Guide":
                        UserColor = StylishNameplatesMod.ourGuideColor.EditedValue;
                        break;
                    case "Moderator":
                        UserColor = StylishNameplatesMod.ourModeratorColor.EditedValue;
                        break;
                    case "Developer":
                        UserColor = StylishNameplatesMod.ourDeveloperColor.EditedValue;
                        break;
                    default:
                        UserColor = StylishNameplatesMod.ourDefaultColor.EditedValue;
                        break;
                }
            }
            UserColorMaxAlpha = new Color(UserColor.r, UserColor.g, UserColor.b, 1);

            _backgroundGameObject = this.transform.Find("Canvas/Content/Image").gameObject;
            Component.DestroyImmediate(_backgroundGameObject.GetComponent<Image>());
            ConfigureImage(_backgroundGameObject.AddComponent<Image>(), UserColor, StylishNameplatesMod.backgroundImage, Image.Type.Sliced, 500);

            _maskGameObject = this.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask").gameObject;
            _maskGameObject.GetComponent<Image>().sprite = StylishNameplatesMod.profileBackgroundImage;
            _maskGameObject.transform.localScale = new Vector3(1.25f, 1.1f, 1);

            _backgroundGameObj = this.transform.Find("Canvas/Content/Image/ObjectMaskSlave/UserImageMask (1)").gameObject;
            ConfigureImage(_backgroundGameObj.GetComponent<Image>(), UserColor, StylishNameplatesMod.profileBackgroundImage);

            _backgroundGameObj.transform.SetSiblingIndex(0);
            _backgroundGameObj.transform.localScale = new Vector3(1.45f, 1.25f, 1);

            /*_maskGameObject.transform.Find("UserImage").transform.localScale = new Vector3(1.05f, 1.05f, 1);*/

            _friendIcon = _backgroundGameObject.transform.Find("FriendsIndicator").gameObject;
            _friendIcon.transform.localScale = new Vector3(0.9f, 0.6f, 1);
            _friendIcon.transform.localPosition = new Vector3(0.60f, 0.39f, 0);

            _friend = _friendIcon.GetComponent<Image>();
            _friend.sprite = StylishNameplatesMod.friendImage;
            _friend.enabled = false;

            MicOff = GameObject.Instantiate(_friendIcon, _friendIcon.transform.parent.transform);
            _micOffImage = MicOff.GetComponent<Image>();
            _micOffImage.sprite = StylishNameplatesMod.micOffImage;
            _micOffImage.enabled = true;
            MicOff.transform.localPosition = new Vector3(0.944f, 0.39f, 0);

            MicOn = GameObject.Instantiate(MicOff, _friendIcon.transform.parent.transform);
            MicOn.GetComponent<UnityEngine.UI.Image>().sprite = StylishNameplatesMod.micOnImage;
            MicOn.transform.localPosition = new Vector3(0.944f, 0.39f, 0);
            MicOn.gameObject.SetActive(false);

            GameObject staffPanel = this.transform.Find("Canvas/Content/Image/Image").gameObject;
            Component.DestroyImmediate(staffPanel.GetComponent<Image>());
            ConfigureImage(staffPanel.AddComponent<Image>(), UserColor, StylishNameplatesMod.backgroundImage, Image.Type.Sliced);

            this.transform.Find("Canvas/Content/Image/Image/TMP:StaffRank").localPosition = new Vector3(-0.08f, 0.2902f, 0f);

            if (UserColor == StylishNameplatesMod.ourFriendsColor.EditedValue) _friend.enabled = true;
            CancelInvoke(nameof(Setup));
            Dispose();
            /*if (Config.Instance.Js.DistanceScale) 
                InvokeRepeating(nameof(GetDistance), -1, 0.1f);*/
        }

        private void ConfigureImage(Image image, Color color, Sprite sprite, Image.Type type = Image.Type.Simple, int pixelsPerUnitMultiplier = 1)
        {
            BackgroundImageComp = image;
            BackgroundImageComp.type = type;
            BackgroundImageComp.pixelsPerUnitMultiplier = pixelsPerUnitMultiplier;
            BackgroundImageComp.color = color;
            BackgroundImageComp.sprite = sprite;
        }

        private float _distance { get; set; }
        private float _distance2 { get; set; }

        private void GetDistance()
        {
            _distance = 0.1f + Vector3.Distance(this.transform.parent.transform.position, StylishNameplatesMod.LocalPlayerTransform.position) / 9;
            _distance2 = _distance > 0.9f ? 0.9f : _distance;
            _canvas.localScale = new Vector3(_distance2, _distance2, 1);
        }


        public void Dispose()
        {
            _friend = null;
            _micOnImage = null;
            _micOffImage = null;
            _maskGameObject = null;
            _maskImageComp = null;
            _backgroundGameObject = null;
            _backgroundGameObj = null;
            _friendIcon = null;
        }
    }
}
