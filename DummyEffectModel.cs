using Eremite.Buildings;
using Eremite.Model;
using Eremite.Model.Effects;
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

        public override bool IsPerk => true;

        public override Sprite GetDefaultIcon()
        {
           return this.overrideIcon;
        }

        public override Color GetTypeColor()
        {
           return base.Settings.RewardColorCommonNegative;
        }

    }
}