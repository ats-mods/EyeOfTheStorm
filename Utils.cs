using System.IO;
using System.Linq;
using Eremite;
using Eremite.Controller;
using Eremite.Model;
using Eremite.Services;
using HarmonyLib;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace EyeOfTheStorm
{
    public static class Utils{

        public static readonly string LOCATEXT_KEY_PREFIX = "sp_eots";
        private static int LOCATEXT_INDEX = 0;
        public static readonly string LOCA_AMBER = "<sprite name=\"[valuable] amber\">";

        public static T Clone<T>(this T unityObject, string newName) where T : SO
        {
            var result = Object.Instantiate<T>(unityObject);
            result.name = result.cachedName = newName;
            return result;
        }

        public static T Clone<T>(this T unityObject) where T : Object
        {
            return Object.Instantiate<T>(unityObject);
        }

        public static Sprite GetSpriteOfEffect(string effectName){
            return Serviceable.Settings.GetEffect(effectName).GetIcon();
        }

        public static Sprite GetSpriteOfMystery(string effectName){
            var s = Serviceable.Settings;
            if(s.ContainsSimpleSeasonalEffect(effectName)){
                return s.GetSimpleSeasonalEffect(effectName).Icon;
            }
            if(s.ContainsConditionalSeasonalEffect(effectName)){
                return s.GetConditionalSeasonalEffect(effectName).Icon;
            }
            return GetSpriteOfEffect("Construction Cost -40");  // Placeholder_DEV
        }

        public static LocaText Text(string value, string key = null){
            if (key == null){
                key = $"{LOCATEXT_KEY_PREFIX}_{LOCATEXT_INDEX++}";
            }
            var ts = (TextsService) MainController.Instance.AppServices.TextsService;
            ts.texts.Add(key, value);
            return new LocaText(){ key = key };
        }

        public static Sprite LoadSprite(string file) {
            var path = Path.Combine(BepInEx.Paths.PluginPath, "assets", file);
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT5, false);
            tex.LoadImage(fileData);
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 50.0f);
            return sprite;
        }

        public static bool HasCondition(string conditionName){
            return Serviceable.BiomeService.Difficulty.modifiers.Where(e => e.effect.name.Equals(conditionName)).Any()
                || Serviceable.StateService.Conditions.lateEffects.Contains(conditionName);
        }
        
        public static bool HasPerk(string perkName){
            return Serviceable.PerksService.HasPerk(perkName);
        }

        public static bool HasAmber(int amount){
            return GameMB.StorageService.GetStorage().IsAvailable(new Good(MB.Settings.tradeCurrency.Name, amount));
        }

        public static void AddInPlace<T>(ref T[] array, T item){
            array = array.AddToArray(item);
        }
    }
}