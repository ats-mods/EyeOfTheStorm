using HarmonyLib;
using Eremite;
using Eremite.Model;
using Eremite.Services;
using Eremite.Controller;
using Eremite.Buildings.UI.Trade;
using Eremite.View.HUD;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Linq;
using Eremite.View;
using Eremite.Controller.Villagers;
using Eremite.Model.Effects;

namespace EyeOfTheStorm
{
    public class Patches {

        [HarmonyPatch(typeof(MainController), nameof(MainController.OnServicesReady))]
        [HarmonyPostfix]
        private static void HookMainControllerSetup()
        {   
            //GathererHutCreator.Patch();
            Content.AddPrestigeDifficulties();
        }

        [HarmonyPatch(typeof(RaceRevealEffectsController), nameof(RaceRevealEffectsController.ApplyEffectFor))]
        [HarmonyPrefix]
        private static bool ApplyRaceReveal(RaceRevealEffectsController __instance, RaceModel race){
            if(!MB.MetaPerksService.IsRevealEffectUnlocked(race.Name) 
                || race.revealEffect == null
                || !Utils.HasCondition("eots_prestige23")){
                return true;
            }
            var allRaces = GameMB.RacesService.Races.Select(r=>r.Name);
            var checkedRaces = __instance.State;
            var hasEmbarkFunc = MB.MetaPerksService.IsRevealEffectUnlocked;
            //toCheck: all races which CAN still provide an embark bonus
            var toCheck = allRaces.Where(hasEmbarkFunc).Except(checkedRaces);
            int roll = Random.RandomRangeInt(1,  toCheck.Count());
            if(roll == 1){
                __instance.State.Clear();
                __instance.State.AddRange(allRaces);
                race.revealEffect.Apply(EffectContextType.None, 0);
                GameMB.NewsService.PublishNews($"Received embarkation bonus of {race.GetDisplayNameFor(2)}");
            } else {
                __instance.State.Add(race.Name);
                //Nothing else happens
            }
            return false;
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
        private static void ResolveService__GetMinResolveForReputation(ResolveService __instance, ref int __result, RaceModel model){
             if(Utils.HasCondition("eots_prestige25"))
            {
                int num = __instance.GetReputationGainFor(model.Name).FloorToInt();
                __result = Mathf.Min(num+__result, model.resolveForReputationTreshold.y).RoundToIntMath();
            }
        }
    }
}