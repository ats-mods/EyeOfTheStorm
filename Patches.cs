using HarmonyLib;
using Eremite;
using Eremite.Model;
using Eremite.Services;
using System.Configuration;
using Eremite.Controller;
using Eremite.Buildings.UI.Trade;
using Eremite.View.HUD;
using UnityEngine;
using System;

namespace EyeOfTheStorm
{
    public class Patches {

        [HarmonyPatch(typeof(MainController), nameof(MainController.OnServicesReady))]
        [HarmonyPostfix]
        private static void HookMainControllerSetup()
        {   
            Content.AddPrestigeDifficulties();
        }

        [HarmonyPatch(typeof(TradeService), nameof(TradeService.CanForceArrival))]
        [HarmonyPostfix]
        private static bool TradeService__CanForceArrival(bool canPay){
            if(canPay && Serviceable.PerksService.HasPerk("eots_prestige24")){
                //TODO: dehardcode this. Use the Perk amount
                return Utils.HasAmber(15);
            }
            return canPay;
        }

        [HarmonyPatch(typeof(TradeService), nameof(TradeService.ForceArrival))]
        [HarmonyPostfix]
        private static void TradeService__ForceArrival(){
            if(Utils.HasPerk("eots_prestige24"))
            {
                SO.StorageService.Main.Remove(new Good(MB.Settings.tradeCurrency.Name, 15));
            }
        }

        [HarmonyPatch(typeof(EmptyTraderPanel), nameof(EmptyTraderPanel.GetForceButtonTooltipTextKey))]
        [HarmonyPostfix]
        private static string EmptyTraderPanel__StartTooltipTrigger(string result){
            if(result.Equals("GameUI_TraderPanel_ForceButton_Tooltip_Desc_TooLate")){
                if(Utils.HasPerk("eots_prestige24") && !Utils.HasAmber(15)){
                    result = Content.KEY_REQUIRES_AMBER;
                }
            }
            return result;
        }

        [HarmonyPatch(typeof(ResolveService), nameof(ResolveService.GetMinResolveForReputation), typeof(RaceModel))]
        [HarmonyPostfix]
        private static void ResolveService__GetMinResolveForReputation(ref int __result, RaceModel model){
             if(Utils.HasPerk("eots_prestige25"))
            {
                __result = Mathf.Min(5+__result, model.resolveForReputationTreshold.y).RoundToIntMath();
            }
        }

        [HarmonyPatch(typeof(CornerstonesService), nameof(CornerstonesService.FindRewardsFor))]
        [HarmonyPostfix]
        private static void CornerstonesService__FindRewardsFor(ref SeasonRewardModel __result){
            if(Utils.HasPerk("eots_prestige26"))
            {
                if(__result != null && __result.year == 1 && __result.season == Season.Drizzle){
                    Plugin.Log("Firing");
                    __result = CorruptedSeasonRewardBuilder.Make();
                    foreach (var e in __result.effectsTable.effects){
                        Plugin.Log(e);
                    }
                }
            }
        }
    }
    
}