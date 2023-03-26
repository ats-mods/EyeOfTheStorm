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
        private static EffectsTable effectsTable = null; // Create during Setup, use during Make()
        public static void Setup(){
            effectsTable = ScriptableObject.CreateInstance<EffectsTable>();
            // guaranteedEffects allowed to be null
            // table.guaranteedEffects = new EffectModel[];
            effectsTable.effects = new EffectsTableEntity[]{ 
                Wrap("Construction Speed Slower 25"), 
                Wrap("FuelConsumption +33"),
                Wrap("Longer Storm +10"),
                Wrap("ReputationPenaltyRate 5")
            };
        }

        public static SeasonRewardModel Make(){
            return new SeasonRewardModel() {
                year = 1, 
                season = Season.Drizzle, 
                quarter = SeasonQuarter.Second,
                effectsTable = effectsTable  
            };
        }

        private static EffectsTableEntity Wrap(string effectName){
            return new EffectsTableEntity(){chance=100, effect= Serviceable.Settings.GetEffect(effectName)};
        }
    }
}