using HarmonyLib;
using Eremite;
using Eremite.Model;
using Eremite.Services;
using System.Configuration;
using Eremite.Controller;

namespace EyeOfTheStorm
{
    public class Patches {

        [HarmonyPatch(typeof(MainController), nameof(MainController.InitSettings))]
        [HarmonyPostfix]
        private static void MainController__InitSettings()
        {   
            Content.AddPrestigeDifficulties();
        }

        [HarmonyPatch(typeof(TextsService), nameof(TextsService.GetLocaText))]
        [HarmonyPrefix]
        private static bool TextsService__GetLocaText(ref string __result, string key){
            if(key.StartsWith(Utils.LOCATEXT_KEY_PREFIX)){
                __result = key.Substring(Utils.LOCATEXT_KEY_PREFIX.Length);
                return false;
            }
            return true;
        }
    }
    
}