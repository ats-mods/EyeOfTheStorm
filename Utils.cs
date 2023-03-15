using System.IO;
using Eremite.Model;
using UnityEngine;

namespace EyeOfTheStorm
{
    public static class Utils{

        public static readonly string LOCATEXT_KEY_PREFIX = "sp_eots";

        public static T Clone<T>(this T unityObject) where T : Object
        {
            return Object.Instantiate<T>(unityObject);
        }

        public static LocaText Text(string value){
            var result = new LocaText();
            result.key = LOCATEXT_KEY_PREFIX + value;
            return result;
        }

        public static Sprite LoadSprite(string file) {
            var path = Path.Combine(BepInEx.Paths.PluginPath, "assets", file);
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.DXT5, false);
            tex.LoadImage(fileData);
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 50.0f);
            return sprite;
        } 
    }
}