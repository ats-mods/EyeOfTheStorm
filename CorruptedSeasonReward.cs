using System;
using System.Runtime.CompilerServices;
using Eremite.Buildings;
using Eremite.Model;
using Eremite.Model.Effects;
using Eremite.Services;
using UnityEngine;

namespace EyeOfTheStorm
{
    public class CorruptedSeasonRewardBuilder {

        public static SeasonRewardModel Make(){
            var model = new SeasonRewardModel() {
                year = 1, 
                season = Season.Drizzle, 
                quarter = SeasonQuarter.Second,
                
            };

            var table = ScriptableObject.CreateInstance<EffectsTable>();
            // guaranteedEffects allowed to be null
            // table.guaranteedEffects = new EffectModel[];
            var eff = Serviceable.Settings.GetEffect;
            table.effects = new EffectsTableEntity[]{ 
                Wrap("Construction Speed Slower 25"), 
                Wrap("FuelConsumption +33"),
                Wrap("Longer Storm +10"),
                Wrap("ReputationPenaltyRate 5")
            };
            model.effectsTable = table;
            return model;
        }


        private static EffectsTableEntity Wrap(string effectName){
            return new EffectsTableEntity(){chance=100, effect= Serviceable.Settings.GetEffect(effectName)};
        }
    }
}