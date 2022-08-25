﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using UnityEngine;
using System.Reflection;

namespace Tayou
{

    public class Json
    {
        public int[] DefaultColor { get; set; }
        public int[] FriendsColor { get; set; }
        public int[] Legend { get; set; }
        public int[] Guide { get; set; }
        public int[] Mod { get; set; }
        public int[] Dev { get; set; }
        public string Background { get; set; }
        public string Icon { get; set; }
        public string MicIconOn { get; set; }
        public string MicIconOff { get; set; }
        public string Friend { get; set; }
        public bool DistanceScale { get; set; }

    }


    internal class Config
    {
        public static Config Instance;
        public Json Js { get; set; }
        public static Color DefaultColor { get; set; }
        public static Color FriendsColor { get; set; }
        public static Color Legend { get; set; }
        public static Color Guide { get; set; }
        public static Color Mod { get; set; }
        public static Color Dev { get; set; }
        private  int[] s_fArr { get; set; }

        public Config()
        {
            Instance = this;
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "//Nocturnal"))
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "//Nocturnal");

            if (!File.Exists(Directory.GetCurrentDirectory() + "//Nocturnal//PlatesManagerConfig1.2.Json"))
            {
                using (WebClient wc = new WebClient())
                {
                    File.WriteAllText(Directory.GetCurrentDirectory() + "//Nocturnal//PlatesManagerConfig1.2.Json", JsonConvert.SerializeObject(new Json()
                    {
                        DefaultColor = new int[] { 255, 8, 90 },
                        FriendsColor = new int[] { 255, 251, 0 },
                        Legend = new int[] { 227, 129, 0 },
                        Guide = new int[] { 0, 199, 7 },
                        Mod = new int[] { 158, 0, 29 },
                        Dev = new int[] { 77, 0, 14 },
                        Background = Convert.ToBase64String(wc.DownloadData("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/nameplate.png")),
                        Icon = Convert.ToBase64String(wc.DownloadData("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/iconbackground.png")),
                        MicIconOn = Convert.ToBase64String(wc.DownloadData("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/Mic%20On.png")),
                        MicIconOff = Convert.ToBase64String(wc.DownloadData("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/micoff.png")),
                        Friend = Convert.ToBase64String(wc.DownloadData("https://raw.githubusercontent.com/TayouVR/CVR-Stylish-Nameplates/master/Icons/friendIcon.png")),
                        DistanceScale = true,
                    }));
                    wc.Dispose();
                }
            }
            Js = JsonConvert.DeserializeObject<Json>(File.ReadAllText(Directory.GetCurrentDirectory() + "//Nocturnal//PlatesManagerConfig1.2.Json"));
            DefaultColor  = new Color32(byte.Parse(Js.DefaultColor[0].ToString()), byte.Parse(Js.DefaultColor[1].ToString()), byte.Parse(Js.DefaultColor[2].ToString()), 170);
            FriendsColor = new Color32(byte.Parse(Js.FriendsColor[0].ToString()), byte.Parse(Js.FriendsColor[1].ToString()), byte.Parse(Js.FriendsColor[2].ToString()), 170);
            PropertyInfo[] ConfigProps = typeof(Config).GetProperties(BindingFlags.Public | BindingFlags.Static);
            PropertyInfo[] JsonProps = Js.GetType().GetProperties();
            for (int i = 0; i < ConfigProps.Length; i++)
            {
                s_fArr = (int[])JsonProps.FirstOrDefault(x => x.Name == ConfigProps[i].Name).GetValue(Js);
                ConfigProps[i].SetValue(typeof(Config), (Color)new Color32(byte.Parse(s_fArr[0].ToString()), byte.Parse(s_fArr[1].ToString()), byte.Parse(s_fArr[2].ToString()), 255));
            }
        }

    }
}
