using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;
using Eremite.Model.Effects;
using Eremite.Model.Effects.Events;
using Eremite.Model.State;
using Eremite.Services;
using HarmonyLib;
using QFSW.QC.Utilities;
using UnityEngine;

namespace EyeOfTheStorm
{
    public class DummyEffectModel : EffectModel {
        
        public override string GetAmountText()
        {
           return "";
        }

        public override void OnApply(EffectContextType contextType, int contextId)
        {
        }

        public override bool HasImpactOn(BuildingModel building)
        {
            return false;
        }

        public override bool IsPerk => isPerk;
        public bool isPerk = false;

        public override Sprite GetDefaultIcon()
        {
           return this.overrideIcon;
        }

        public override Color GetTypeColor()
        {
           return base.Settings.RewardColorCommonNegative;
        }

    }

    public class RandomLootBoxEffectModel : DummyEffectModel {
        public RandomLootBoxEffectModel() {
            this.isPerk = false;
        }

        private class WeightedString : IWeightable<String>{
            public int Weight { get; internal set; }
            public string Value{ get; internal set; }
        }

        public override void OnApply(EffectContextType contextType, int contextId)
        {
            var options = new WeightedString[] {
                new WeightedString() { Weight = 3, Value = "Rewards Pack Small"},
                new WeightedString() { Weight = 2, Value = "Rewards Pack Medium"},
                new WeightedString() { Weight = 1, Value = "Rewards Pack Big"},
            };

            var pick = options.RandomElement().Value;
            if ( UnityEngine.Random.value > 0.5) pick += " 1";
            var effect = (RewardsPackEffectModel) Serviceable.Settings.GetEffect(pick);
            var pack = effect.GetRewards();
            pack.popupDesc = "The deal has been struck";
            pack.popupHeader = "Trickster's Bargain";
            effect.Grant(pack, contextType, contextId);
        }
    }

    public class GrantCorruptedPickEffectModel : DummyEffectModel {

        public override void OnApply(EffectContextType contextType, int contextId)
        {
            ((CornerstonesService) Serviceable.CornerstonesService).GenerateRewardsFor(CorruptedSeasonRewardBuilder.Make(), true);
        }
    }

    public class NoMoreCornerstonesEffectModel : DummyEffectModel {
        public override void OnApply(EffectContextType contextType, int contextId)
        {
            Serviceable.StateService.Gameplay.lastCornerstonePickDate = new GameDate(20, Season.Storm, SeasonQuarter.First);
        }
    }

    public class LoseGameEffectModel : DummyEffectModel {
        public override void OnApply(EffectContextType contextType, int contextId)
        {
            if(!Serviceable.ReputationService.IsGameFinished())
                Serviceable.ReputationService.Abandon();
        }
    }

    public class HearthFirekeeperEffectModel : DummyEffectModel {
        public override void OnApply(EffectContextType contextType, int contextId)
        {

        }

        public override void OnRemove(EffectContextType contextType, int contextId)
        {
            
        }

        public static void UpgradeHearth(){
            var hearth = GetAncientHearth();
            hearth.state.workers = hearth.state.workers.ForceAdd(0);
            if (hearth.reservedWorkplaces != null){
                hearth.reservedWorkplaces = hearth.reservedWorkplaces.ForceAdd(false);
            }
            var model = hearth.model;
            model.workplaces = model.workplaces.ForceAdd(new WorkplaceModel(){allowedRaces=Serviceable.Settings.Races});
        }

        public static void DowngradeHearth(){
            var hearth = GetAncientHearth();
            Truncate(ref hearth.state.workers);
            Truncate(ref hearth.reservedWorkplaces);
            Truncate(ref hearth.model.workplaces);
        }

        private static void Truncate<T>(ref T[] arr) {
            if (arr != null && arr.Length > 1) {
                Array.Resize(ref arr, 1);
            }
        }

        private static Hearth GetAncientHearth(){
            return Serviceable.BuildingsService.Hearths.Values.OrderBy(h=>h.Id).First();
        }
    }
}