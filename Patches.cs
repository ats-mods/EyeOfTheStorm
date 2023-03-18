using HarmonyLib;
using Eremite;
using Eremite.Model;
using Eremite.Services;
using System.Configuration;
using Eremite.Controller;
using Eremite.Buildings.UI.Trade;
using Eremite.View.HUD;

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

        [HarmonyPatch(typeof(TradeService), nameof(TradeService.CanForceArrival))]
        [HarmonyPostfix]
        private static bool TradeService__CanForceArrival(bool canPay){
            if(canPay && Serviceable.PerksService.HasPerk("eots_prestige24")){
                //TODO: dehardcode this. Use the Perk amount
                return Utils.HasAmber(15);
            }
            Plugin.Log($"Checking prestige24 and {canPay} and {Serviceable.PerksService.HasPerk("eots_prestige24")}");
            return canPay;
        }

        [HarmonyPatch(typeof(EmptyTraderPanel), nameof(EmptyTraderPanel.StartTooltipTrigger))]
        [HarmonyPrefix]
        private static bool EmptyTraderPanel__StartTooltipTrigger(EmptyTraderPanel __instance, ref SimpleTooltipRemoteTrigger ___forceButtonTooltipTrigger){
            if("GameUI_TraderPanel_ForceButton_Tooltip_Desc_TooLate".Equals(__instance.GetForceButtonTooltipTextKey())){
                if(Serviceable.PerksService.HasPerk("eots_prestige24") && !Utils.HasAmber(15)){
                    ___forceButtonTooltipTrigger.SetUp(() => __instance.GetText("GameUI_TraderPanel_ForceButton_Tooltip_Header"), () => $"Requires 15 {Utils.LOCA_AMBER} Amber");
                    return false;
                }
            }
            return true;
        }

    }
    
}