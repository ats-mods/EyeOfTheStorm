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
using Eremite.Buildings;
using UniRx;
using System.Net.Mime;

namespace EyeOfTheStorm
{
    public class Patches {

        [HarmonyPatch(typeof(MainController), nameof(MainController.OnServicesReady))]
        [HarmonyPostfix]
        private static void HookMainControllerSetup()
        {   
            Content.AddPrestigeDifficulties();
            GathererHutCreator.Patch();
        }

        [HarmonyPatch(typeof(GameController), nameof(GameController.StartGame))]
        [HarmonyPostfix]
        private static void HookEveryGameStart()
        {
            // Too difficult to predict when GameController will exist and I can hook observers to it
            // So just use Harmony and save us all some time
            var isNewGame = MB.GameSaveService.IsNewGame();

            if(isNewGame){
                GathererHutCreator.UpdateEssentialBuildings();
            }

            // Handle lingering or lacking firekeeper job slots from previous game.
            if(Utils.HasPerk("eots_cc_blazeit")){
                HearthFirekeeperEffectModel.UpgradeHearth();
            } else {
                HearthFirekeeperEffectModel.DowngradeHearth();
            }

            if(Utils.HasCondition("eots_prestige23")){
                var perks = Serviceable.PerksService;
                var corruptionMarkerName = "eots_prestige23_marker";

                if(isNewGame) perks.AddPerk(corruptionMarkerName, false);

                Serviceable.CornerstonesService.OnRewardsPicked
                    .Where(()=>perks.HasPerk(corruptionMarkerName))
                    .Subscribe<Unit>(()=>perks.RemovePerk(corruptionMarkerName));
            }
        }

        [HarmonyPatch(typeof(TradeService), nameof(TradeService.CanForceArrival))]
        [HarmonyPostfix]
        private static bool TradeService__CanForceArrival(bool canPay){
            if(canPay && Utils.HasCondition("eots_prestige24")){
                return Utils.HasAmber(Content.TRADER_CALL_AMBER_COST);
            }
            return canPay;
        }

        [HarmonyPatch(typeof(TradeService), nameof(TradeService.ForceArrival))]
        [HarmonyPostfix]
        private static void TradeService__ForceArrival(){
            if(Utils.HasCondition("eots_prestige24"))
            {
                SO.StorageService.Main.Remove(new Good(MB.Settings.tradeCurrency.Name, Content.TRADER_CALL_AMBER_COST));
            }
        }

        [HarmonyPatch(typeof(EmptyTraderPanel), nameof(EmptyTraderPanel.GetForceButtonTooltipTextKey))]
        [HarmonyPostfix]
        private static string EmptyTraderPanel__StartTooltipTrigger(string result){
            if(result.Equals("GameUI_TraderPanel_ForceButton_Tooltip_Desc_TooLate")){
                if(Utils.HasCondition("eots_prestige24") && !Utils.HasAmber(Content.TRADER_CALL_AMBER_COST)){
                    result = Content.KEY_REQUIRES_AMBER;
                }
            }
            return result;
        }

        [HarmonyPatch(typeof(ResolveService), nameof(ResolveService.GetMinResolveForReputation), typeof(RaceModel))]
        [HarmonyPostfix]
        private static void ResolveService__GetMinResolveForReputation(ref int __result, RaceModel model){
             if(Utils.HasCondition("eots_prestige26"))
            {
                __result = Mathf.Min(5+__result, model.resolveForReputationTreshold.y).RoundToIntMath();
            }
        }

        [HarmonyPatch(typeof(CornerstonesService), nameof(CornerstonesService.FindRewardsFor))]
        [HarmonyPostfix]
        private static void CornerstonesService__FindRewardsFor(ref SeasonRewardModel __result){
            var corruptionMarkerName = "eots_prestige23_marker";
            if(__result != null && Utils.HasPerk(corruptionMarkerName))
            {
                __result = CorruptedSeasonRewardBuilder.Make(__result);
            }
        }

        [HarmonyPatch(typeof(RewardPickPopup), nameof(RewardPickPopup.Show))]
        [HarmonyPostfix]
        private static void RewardPickPopup__Show(ref Button ___skipButton){
            if(Utils.HasPerk("eots_prestige23_marker")){
                ___skipButton.interactable = false;
                return;
            }
            ___skipButton.interactable = true;
        }

        [HarmonyPatch(typeof(Hearth), nameof(Hearth.Corrupt))]
        [HarmonyPostfix]
        private static void Hearth__Corrupt(){
            if(Utils.HasPerk("eots_cc_cystmenace_nested")){
                if(!Serviceable.ReputationService.IsGameFinished())
                    Serviceable.ReputationService.Abandon();
            }
        }
    }
    
}