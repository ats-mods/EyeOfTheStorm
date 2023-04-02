using HarmonyLib;
using Eremite;
using Eremite.Model;
using Eremite.Services;
using System.Configuration;
using Eremite.Controller;
using Eremite.Buildings.UI.Trade;
using Eremite.View.HUD;
using UnityEngine;
using UnityEngine.UI;
using System;
using Eremite.Model.State;
using UniRx;

namespace EyeOfTheStorm
{
    public class Patches {

        [HarmonyPatch(typeof(MainController), nameof(MainController.OnServicesReady))]
        [HarmonyPostfix]
        private static void HookMainControllerSetup()
        {   
            Content.AddPrestigeDifficulties();
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.StartGame))]
        [HarmonyPostfix]
        private static void HookEveryGameStart()
        {
            // Too difficult to predict when GameController will exist and I can hook observers to it
            // So just use Harmony and save us all some time
            if(Utils.HasPerk("eots_cc_blazeit")){
                HearthFirekeeperEffectModel.UpgradeHearth();
            } else {
                HearthFirekeeperEffectModel.DowngradeHearth();
            }
        }

        [HarmonyPatch(typeof(TradeService), nameof(TradeService.CanForceArrival))]
        [HarmonyPostfix]
        private static bool TradeService__CanForceArrival(bool canPay){
            if(canPay && Utils.HasCondition("eots_prestige24")){
                //TODO: dehardcode this. Use the Perk amount
                return Utils.HasAmber(15);
            }
            return canPay;
        }

        [HarmonyPatch(typeof(TradeService), nameof(TradeService.ForceArrival))]
        [HarmonyPostfix]
        private static void TradeService__ForceArrival(){
            if(Utils.HasCondition("eots_prestige24"))
            {
                SO.StorageService.Main.Remove(new Good(MB.Settings.tradeCurrency.Name, 15));
            }
        }

        [HarmonyPatch(typeof(EmptyTraderPanel), nameof(EmptyTraderPanel.GetForceButtonTooltipTextKey))]
        [HarmonyPostfix]
        private static string EmptyTraderPanel__StartTooltipTrigger(string result){
            if(result.Equals("GameUI_TraderPanel_ForceButton_Tooltip_Desc_TooLate")){
                if(Utils.HasCondition("eots_prestige24") && !Utils.HasAmber(15)){
                    result = Content.KEY_REQUIRES_AMBER;
                }
            }
            return result;
        }

        [HarmonyPatch(typeof(ResolveService), nameof(ResolveService.GetMinResolveForReputation), typeof(RaceModel))]
        [HarmonyPostfix]
        private static void ResolveService__GetMinResolveForReputation(ref int __result, RaceModel model){
             if(Utils.HasCondition("eots_prestige25"))
            {
                __result = Mathf.Min(5+__result, model.resolveForReputationTreshold.y).RoundToIntMath();
            }
        }

        [HarmonyPatch(typeof(CornerstonesService), nameof(CornerstonesService.FindRewardsFor))]
        [HarmonyPostfix]
        private static void CornerstonesService__FindRewardsFor(ref SeasonRewardModel __result){
            if(Utils.HasCondition("eots_prestige26"))
            {
                if(__result != null && __result.year == 1 && __result.season == Season.Drizzle){
                    __result = CorruptedSeasonRewardBuilder.Make();
                }
            }
        }

        [HarmonyPatch(typeof(RewardPickPopup), nameof(RewardPickPopup.Show))]
        [HarmonyPostfix]
        private static void RewardPickPopup__Show(ref Button ___skipButton){
            if(Utils.HasCondition("eots_prestige26")){
                var lastPickDate = Serviceable.StateService.Gameplay.lastCornerstonePickDate;
                if(lastPickDate == null || lastPickDate < new GameDate(){year=1, season=Season.Drizzle, quarter=SeasonQuarter.Second}){
                    ___skipButton.interactable = false;
                    return;
                }
            }
            ___skipButton.interactable = true;
        }
    }
    
}