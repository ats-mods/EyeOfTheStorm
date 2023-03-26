using System;
using System.Runtime.CompilerServices;
using Eremite.Buildings;
using Eremite.Model;
using Eremite.Model.Effects;
using Eremite.Services;
using QFSW.QC.Containers;
using UnityEngine;

namespace EyeOfTheStorm
{
    public class CorruptedSeasonRewardBuilder {
        private static EffectsTable effectsTable = null; // Create during Setup, use during Make()
        private static EffectModel loseGameEffect;

        public static void Setup(){
            loseGameEffect = Content.NewEffect<LoseGameEffectModel>("cc_losegame", "", "");
            effectsTable = ScriptableObject.CreateInstance<EffectsTable>();
            // guaranteedEffects allowed to be null. Not used for cornerstone picks
            effectsTable.effects = new EffectsTableEntity[]{ 
                Cowardice(),
                Blightrot(),
                Lose(),
                NoneLeftBehind(),
            };
        }

        private static EffectsTableEntity Cowardice(){
            var effect = Content.NewEffect<ReputationPenaltyRateEffectModel>(
                "cc_cowardice", "Cowardice",
                "Refuse to deal with the corrupted Cornerstone. The Queen will not be pleased. Impatience grows {0} faster."
            );
            effect.isPositive = false;
            effect.frameColorByPositive = true;
            effect.amount = 0.33f;
            return Wrap(effect, 1_000_000);
        }

        private static EffectsTableEntity Blightrot(){
            var effect = Content.NewEffect<SpawnCystsEffectModel>(
                "cc_blight", "Blightrot", "Instantly spawn {0} Blightrot Cysts."
            );
            effect.amount = 5;
            effect.overrideIcon = Utils.GetSpriteOfEffect("Blightrot Resolve");
            return Wrap(effect);
        }

        private static EffectsTableEntity Lose(){
            var effect = Content.NewHookedEffect(
                "cc_lose", "Judgment Deferred", "You will lose the game on the 8th year"
            );
            effect.clearEffectsOnRemove = true;
            effect.overrideIcon = Utils.GetSpriteOfEffect("Hearth Sacrifice Block");
            effect.frameColorByPositive = true;
            effect.isPositive = false;
            effect.hooks = new HookLogic[]{new YearChangeHook(){amount=8}};
            effect.hookedEffects = new EffectModel[]{ loseGameEffect };
            return Wrap(effect);
        }

        private static EffectsTableEntity NoneLeftBehind(){
            var effect = Content.NewHookedEffect(
                "cc_risky", "No One Left Behind", "+5 to Global Resolve. If any villager leaves or dies, you lose the game"
            );
            effect.clearEffectsOnRemove = true;
            effect.overrideIcon = Utils.LoadSprite("SGI_48_modified.png");
            effect.instantEffects = new EffectModel[]{
                Serviceable.Settings.GetEffect("Ancient Artifact 3"),
                Serviceable.Settings.GetEffect("Ancient Artifact 2")
            };
            effect.hooks = new HookLogic[]{new VillagerDeathHook()};
            effect.hookedEffects = new EffectModel[]{ loseGameEffect };
            return Wrap(effect);
        }

        public static SeasonRewardModel Make(){
            return new SeasonRewardModel() {
                year = 1, 
                season = Season.Drizzle, 
                quarter = SeasonQuarter.Second,
                effectsTable = effectsTable  
            };
        }
        private static EffectsTableEntity Wrap(EffectModel effect, int chance = 10) 
            => new EffectsTableEntity(){chance=chance, effect=effect};
    }
}