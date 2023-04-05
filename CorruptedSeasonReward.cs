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
                Gamble(),
                Loan(),
                NoGrace(),
                Explorers(),
                OnYourOwn(),
                BlazeIt(),
                Unrustled(),
                Rerolls(),
                Blightrot(),
                Storm(),
                Lose(),
                PlayingWithFire(),
                NoneLeftBehind(),
            };
        }

        private static EffectsTableEntity Refusal(){
            var effect = Content.NewEffect<ReputationPenaltyRateEffectModel>(
                "cc_cowardice", "Cowardice",
                "Let someone else deal with the corrupted Cornerstone. The Queen will not be pleased. Impatience grows {0} faster."
            );
            effect.isPositive = false;
            effect.frameColorByPositive = true;
            effect.amount = 0.25f;
            return Wrap(effect, 1_000_000);
        }

        private static EffectsTableEntity Gamble(){
            var effect = Content.NewEffect<CompositeEffectModel>(
                "cc_gamble", "Trickster's Bargain",
                "Receive a random Mystery Box. You will have two pick two additional corrupted cornerstones."
            );
            effect.overrideIcon = Utils.GetSpriteOfEffect("Rewards Pack Medium");
            effect.isPositive = false;
            effect.frameColorByPositive = true;
            effect.isPerk = false;
            var pickEffect = Content.NewEffect<ExtraCornerstonePickEffectModel>("cc_gamble_pick", "", "");
            pickEffect.year = 1;
            effect.rewards = new EffectModel[]{
                pickEffect,
                pickEffect,
                Serviceable.Settings.GetEffect("Rewards Pack Medium"),
            };
            effect.dynamicDescriptionArgs = new TextArg[]{};
            return Wrap(effect);
        }

        private static EffectsTableEntity Loan(){
            var effect = Content.NewEffect<CompositeEffectModel>(
                "cc_loan", "Harpy in the hand",
                "Instantly receive 3 cornerstone picks. This settlement receives no more cornerstones for the remainder of the game."
            );
            effect.overrideIcon = Utils.GetSpriteOfEffect("Chest Working Time -30");
            effect.isPositive = false;
            effect.frameColorByPositive = true;
            var blockEffect = Content.NewEffect<NoMoreCornerstonesEffectModel>("cc_loan_block", "", "");
            var pickEffect1 = Content.NewEffect<ExtraCornerstonePickEffectModel>("cc_loan_pick1", "", "");
            var pickEffect2 = Content.NewEffect<ExtraCornerstonePickEffectModel>("cc_loan_pick2", "", "");
            var pickEffect3 = Content.NewEffect<ExtraCornerstonePickEffectModel>("cc_loan_pick3", "", "");
            pickEffect1.year = 3;
            pickEffect2.year = 2;
            pickEffect2.year = 5;
            effect.rewards = new EffectModel[]{ 
                blockEffect,
                pickEffect1,
                pickEffect2,
                pickEffect3
            };
            effect.dynamicDescriptionArgs = new TextArg[0];
            return Wrap(effect);
        }

        private static EffectsTableEntity NoGrace(){
            var effect = Content.NewEffect<GracePeriodEffectModel>(
                "cc_nograce", "Lack of Grace",
                "Failure will not be tolerated. You receive no time to save your settlment after reaching maximum impatience"
            );
            effect.overrideIcon = Utils.LoadSprite("queen.png");
            effect.amount = -180;
            effect.isPositive = false;
            effect.frameColorByPositive = true;
            return Wrap(effect);
        }

        private static EffectsTableEntity Explorers(){
            var effect = Content.NewEffect<CompositeEffectModel>(
                "cc_explorers", "Whispers of the Forest",
                "The people seem single-mindedly obsessed with venturing into the forest." +
                " Gain 10 to Global Resolve, but Reputation gain from Resolve is disabled."
            );
            effect.overrideIcon = Utils.GetSpriteOfEffect("[Mod] Dangerous Glades Info Block");
            effect.isPositive = false;
            effect.frameColorByPositive = true;

            var blockEffect = Content.NewEffect<ReputationFromResolveBlockEffectModel>(
                "cc_explorers_block", "", ""
            );
            effect.rewards = new EffectModel[]{ 
                Serviceable.Settings.GetEffect("Joy of Discovery"),
                Serviceable.Settings.GetEffect("Explorers Boredom"),
                blockEffect
            };
            effect.dynamicDescriptionArgs = new TextArg[0];
            return Wrap(effect);
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

        private static EffectsTableEntity Unrustled(){
            var effect = Content.NewEffect<CompositeEffectModel>(
                "cc_unrustled", "Unrustled Exploration",
                "Opening Dangerous or Forbidden glades incurs an additional {0} hostility, but small glades no longer increase hostility."
            );
            effect.overrideIcon = Utils.GetSpriteOfEffect("[Mod] Memory of the Forest");
            effect.isPositive = false;
            effect.frameColorByPositive = true;

            var smallEffect = Content.NewEffect<HostilitySourceChangeEffectModel>(
                "cc_unrustled_small", "", ""
            );
            smallEffect.block = true;
            smallEffect.source = HostilitySource.Glade;
            var bigEffect = Content.NewEffect<HostilitySourceChangeEffectModel>(
                "cc_unrustled_big", "", ""
            );
            bigEffect.change = 10;
            bigEffect.source = HostilitySource.DangerousGlade;
            effect.rewards = new EffectModel[]{ 
                smallEffect, bigEffect
            };
            effect.dynamicDescriptionArgs = new TextArg[]{
                new TextArg(){sourceIndex = 1, type = TextArgType.Amount},
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

        private static EffectsTableEntity PlayingWithFire(){
            var name = "Playing with Fire";
            var desc = "Blightrot cysts burn for an additional {0} seconds." +
                " If the Hearth gets fully corrupted, you lose the game";
            var effect = Content.NewEffect<CompositeEffectModel>(
                "cc_cystmenace", name, 
                "Receive both \"Baptism of Fire\" and \"Burnt to a Crisp\". " + desc
            );
            effect.isPositive = false;
            effect.frameColorByPositive = true;
            effect.isPerk = false;
            effect.overrideIcon = Utils.GetSpriteOfEffect("SE Marrow Mine");
            effect.showEffectsAsPerks = true;
            var nestedEffect = Content.NewEffect<CystsBonusBurningTimeEffectModel>("cc_cystmenace_nested", name, desc);
            nestedEffect.amount = 10;
            nestedEffect.isPositive = false;
            nestedEffect.frameColorByPositive = true;
            nestedEffect.overrideIcon = effect.overrideIcon;
            var Effect = Serviceable.Settings.GetEffect;
            effect.rewards = new EffectModel[]{nestedEffect, Effect("Coal for Cysts"), Effect("Hostility for Removed Cysts")};
            effect.dynamicDescriptionArgs = new TextArg[]{
                new TextArg(){sourceIndex = 0, type = TextArgType.Amount}
            };
            return Wrap(effect);
        }

        private static EffectsTableEntity Storm(){
            var effect = Content.NewEffect<SeasonLengthEffectModel>(
                "cc_storm", "Blessing of the Sealed Ones", "The {0} lasts another {1} longer."
            );
            effect.season = Season.Storm;
            effect.amount = 1f;
            effect.overrideIcon = Utils.GetSpriteOfEffect("Remove Buildings Thunder");
            effect.isPositive = false;
            effect.frameColorByPositive = false;
            return Wrap(effect);
        }

        private static EffectsTableEntity Lose(){
            var effect = Content.NewHookedEffect(
                "cc_lose", "Judgment Day", "Impatience does not rise with time, but on Drizzle of the 8th year, you lose the game."
            );
            effect.clearEffectsOnRemove = true;
            // effect.overrideIcon = Utils.GetSpriteOfEffect("Hearth Sacrifice Block");
            effect.overrideIcon = Utils.GetSpriteOfEffect("Humans Killed 3 - Missiles");
            effect.frameColorByPositive = true;
            effect.isPositive = false;
            var impatienceEffect = Content.NewEffect<ReputationPenaltyRateEffectModel>(
                "cc_lose_impatience", "", ""
            );
            impatienceEffect.amount = -2.0f;
            effect.instantEffects = new EffectModel[] { impatienceEffect };
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
                Serviceable.Settings.GetEffect("Ancient Artifact 2"),
                Serviceable.Settings.GetEffect("Ancient Artifact 2"),
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