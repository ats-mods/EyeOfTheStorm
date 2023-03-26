using System.Runtime.CompilerServices;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;
using Eremite.Model.Effects;
using Eremite.Services;
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
        public bool isPerk = true;

        public override Sprite GetDefaultIcon()
        {
           return this.overrideIcon;
        }

        public override Color GetTypeColor()
        {
           return base.Settings.RewardColorCommonNegative;
        }

    }

    public class LoseGameEffectModel : DummyEffectModel {
        public override void OnApply(EffectContextType contextType, int contextId)
        {
            if(!Serviceable.ReputationService.IsGameFinished())
                Serviceable.ReputationService.Abandon();
        }
    }
}