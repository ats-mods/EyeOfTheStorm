using System;
using System.Runtime.CompilerServices;
using Eremite.Buildings;
using Eremite.Model;
using Eremite.Model.Effects;
using Eremite.Model.State;
using Eremite.Services;
using Eremite.Services.Monitors;
using Eremite.View.HUD;
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
            effectsTable.amounts = new Vector2Int(2, 2);
            // guaranteedEffects allowed to be null. Not used for cornerstone picks
            effectsTable.effects = new EffectsTableEntity[]{ 
                Refusal(),
                OnYourOwn(),
                BlazeIt(),
                Rerolls(),
                Blightrot(),
                Storm(),
                Lose(),
                BlightMenace(),
                NoneLeftBehind(),
            };
        }

        private static EffectsTableEntity Refusal(){
            var effect = Content.NewEffect<ReputationPenaltyRateEffectModel>(
                "cc_cowardice", "Refuse",
                "Let someone else deal with the corrupted Cornerstone. The Queen will not be pleased. Impatience grows {0} faster."
            );
            effect.isPositive = false;
            effect.frameColorByPositive = true;
            effect.amount = 0.33f;
            return Wrap(effect, 1_000_000);
        }

        private static EffectsTableEntity OnYourOwn(){
            var effect = Content.NewEffect<CompositeEffectModel>(
                "cc_onyourown", "On Your Own",
                "You no longer feel the Queen's presence. Hostility does not reduce from impatience," +
                " but villagers dying or leaving no longer increases impatience."
            );
            effect.overrideIcon = Utils.GetSpriteOfEffect("VillagerDeathEffectBlock");
            effect.isPositive = false;
            effect.frameColorByPositive = true;

            var hostEffect = Content.NewEffect<HostilitySourceChangeEffectModel>(
                "cc_onyourown_hostility", "", ""
            );
            hostEffect.block = true;
            hostEffect.showAmount = false;
            hostEffect.source = HostilitySource.ReputationPenalty;
            effect.rewards = new EffectModel[]{ 
                Serviceable.Settings.GetEffect("VillagerDeathEffectBlock"), hostEffect};
            effect.dynamicDescriptionArgs = new TextArg[0];
            return Wrap(effect);
        }

        private static EffectsTableEntity BlazeIt(){
            var effect = Content.NewEffect<CompositeEffectModel>(
                "cc_blazeit", "Blazing Flame",
                "Fuel consumption is increased by {0}, But your hearth gains an additional firekeeper."
            );
            effect.overrideIcon = Utils.GetSpriteOfEffect("Tree Wood Lost");
            effect.isPositive = false;
            effect.frameColorByPositive = true;

            var fuelEffect = Content.NewEffect<FuelRateEffectModel>(
                "cc_blazeit_fuelrate", "", ""
            );
            fuelEffect.amount = 1f;
            var hearthEffect = Content.NewEffect<HearthFirekeeperEffectModel>(
                "cc_blazeit_hearth", "", ""
            );
            effect.rewards = new EffectModel[]{ 
                fuelEffect, hearthEffect
            };
            effect.dynamicDescriptionArgs = new TextArg[]{
                new TextArg(){sourceIndex = 0, type = TextArgType.Amount},
            };
            return Wrap(effect);
        }

        private static EffectsTableEntity Rerolls(){
            var effect = Content.NewEffect<CompositeEffectModel>(
                "cc_rerolls", "Luck of the Draw",
                "You can choose from {0} fewer cornerstones, but receive {1} additional rerolls."
            );
            effect.overrideIcon = Utils.GetSpriteOfEffect("Rewards Pack Big");
            var moreRerolls = Content.NewEffect<CornerstonesRerollsEffectModel>(
                "cc_rerolls_rerolls", "", ""
            );
            moreRerolls.amount = 5;
            moreRerolls.isPositive = true;
            var Effect = Serviceable.Settings.GetEffect;
            effect.rewards = new EffectModel[]{ Effect("[Map Mod] One Perk"), moreRerolls};
            effect.dynamicDescriptionArgs = new TextArg[]{
                new TextArg(){sourceIndex = 0, type = TextArgType.RawAmount},
                new TextArg(){sourceIndex = 1, type = TextArgType.Amount}
            };
            return Wrap(effect);
        }

        private static EffectsTableEntity Blightrot(){
            var effect = Content.NewEffect<SpawnCystsEffectModel>(
                "cc_blight", "Blightrot", "Instantly spawn {0} Blightrot Cysts."
            );
            effect.amount = 5;
            effect.overrideIcon = Utils.GetSpriteOfEffect("Blightrot Resolve");
            return Wrap(effect);
        }

        private static EffectsTableEntity BlightMenace(){
            var effect = Content.NewHookedEffect(
                "cc_cystmenace", "Blightrot Menace",
                "Receive both \"Baptism of Fire\" and \"Burnt to a Crisp\"." +
                " Blightrot spawns each Drizzle, the amount increasing by 6 cysts each time."
            );
            effect.overrideIcon = Utils.GetSpriteOfEffect("SE Marrow Mine");
            effect.showInstantRewardsAsPerks = true;
            effect.showHookedRewardsAsPerks = false;
            var Effect = Serviceable.Settings.GetEffect;
            effect.instantEffects = new EffectModel[]{ Effect("Coal for Cysts"), Effect("Hostility for Removed Cysts")};
            effect.hooks = new HookLogic[]{new YearChangeHook(){amount=1}};

            var triggerEffect = Content.NewEffect<CystMenaceEffectModel>("cc_cystmenace_trigger", "", "");
            triggerEffect.amount = -4;
            triggerEffect.amountToIncrease = 3;
            triggerEffect.publishNews = true;
            triggerEffect.news = Utils.Text("The Blightrot menace grows");
            triggerEffect.newsDescription = Utils.Text("Reason: Blightrot Menace");
            triggerEffect.newsSeverity = AlertSeverity.Warning;

            effect.hookedEffects = new EffectModel[]{ triggerEffect };
            return Wrap(effect);
        }

        private static EffectsTableEntity Storm(){
            var effect = Content.NewEffect<SeasonLengthEffectModel>(
                "cc_storm", "Blessing of the Sealed Ones", "The {0} lasts another {1} longer."
            );
            effect.season = Season.Storm;
            effect.amount = 1f;
            effect.overrideIcon = Utils.GetSpriteOfEffect("Remove Buildings Thunder");
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
                "cc_risky", "No One Left Behind", "+4 to Global Resolve. If any villager leaves or dies, you lose the game"
            );
            effect.clearEffectsOnRemove = true;
            effect.overrideIcon = Utils.LoadSprite("SGI_48_modified.png");
            effect.instantEffects = new EffectModel[]{
                Serviceable.Settings.GetEffect("Ancient Artifact 3"),
                Serviceable.Settings.GetEffect("Ancient Artifact 1"),
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
                effectsTable = effectsTable,
            };
        }
        private static EffectsTableEntity Wrap(EffectModel effect, int chance = 10) 
            => new EffectsTableEntity(){chance=chance, effect=effect};
    }
}