using Eremite.Controller;
using Eremite.Model;
using Eremite.Model.Configs;
using Eremite.Model.Effects;
using Eremite.Model.Orders;
using Eremite.Services;
using Eremite.WorldMap.Conditions;
using Eremite.WorldMap.UI;
using HarmonyLib;
using QFSW.QC.Containers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EyeOfTheStorm
{
    public class Content {
        public static readonly string KEY_REQUIRES_AMBER = "sp_eots_requires_amber";

        private static List<EffectModel> effectsToAdd = new List<EffectModel>();

        public static void AddPrestigeDifficulties(){
            Prestige21();
            Prestige22();
            Prestige23();
            Prestige24();
            Prestige25();
            Done();
        }

        private static void Prestige21(){
            var diff = NewDifficulty("Trade routes require more provisions");
            var effect = NewEffect<TradeRoutesBonusFuelEffectModel>(
                "prestige21", 
                "The Longer Road", 
                "Your ambition leads you to places few dare to tread. Each trade route requires an additional 2 provisions."
            );
            effect.amount = 2;
            effect.overrideIcon = Utils.LoadSprite("tree.png");
            diff.modifiers.Last().effect = effect;
        }

        private static void Prestige22(){
            var diff = NewDifficulty("Removed buildings refund half.");
            var effect = NewEffect<BuildingsRefundRateEffectModel>(
                "prestige22",
                "Rust and Rot",
                "Removing a building only refunds 50% of the original cost."
            );
            effect.amount = -0.5f;
            effect.overrideIcon = Utils.GetSpriteOfEffect("[Mod] No Crude Workstation");
            diff.modifiers.Last().effect = effect;
        }

        private static void Prestige23(){
            var diff = NewDifficulty("Blightrot appears every second Clearance season instead");
            var blightrotMod = diff.modifiers[3];
            var modsList = diff.modifiers.ToList();
            modsList.RemoveAt(3);

            var effect = blightrotMod.effect.Clone() as HookedEffectModel;
            SetupEffect(effect, "prestige23", null, null);
            effect.hooks[0] = new SeasonChangeHook() { season = Season.Clearance, yearsInterval = 2};
            effect.overrideIcon = Utils.LoadSprite("plaguedoctor.png");
            effect.description = Utils.Text("Large swarms of blightrot migrate across the realm. Every second Clearance season, 5 Blightrot Cysts will appear in the settlement");
            diff.modifiers = modsList.ToArray();
            diff.modifiers.Last().effect = effect;
        }

        private static void Prestige24(){
            var diff = NewDifficulty("Calling traders costs amber");
            var effect = NewEffect<DummyEffectModel>(
                "prestige24", "Price Gouging", 
                $"A friend in need is a customer indeed. Calling a trader costs 15 {Utils.LOCA_AMBER} Amber."
                );
            effect.overrideIcon = Utils.GetSpriteOfEffect("Reputation from Trade");
            diff.modifiers.Last().effect = effect;
            Utils.Text($"Requires 15 {Utils.LOCA_AMBER} Amber", KEY_REQUIRES_AMBER);
        }

        private static void Prestige25(){
            var diff = NewDifficulty("Higher resolve to gain Reputation");
            var effect = NewEffect<DummyEffectModel>(
                "prestige25", "Promised Land", 
                "The people have heard many tales of a legendary Viceroy and expect great things from you. +5 to Resolve thresholds for gaining Reputation."
                );
            effect.overrideIcon = Utils.LoadSprite("crowdgathers.png");
            diff.modifiers.Last().effect = effect;
        }


        private static void Prestige27(){
            var diff = NewDifficulty("Shorter Drizzle and Clearance");
            var effect = NewEffect<CompositeEffectModel>(
                "prestige27", "Darkening Clouds",
                "The forest whispered, \"you cannot withstand the storm\". The viceroy whispered back, \"I am the storm\". -25% to Drizzle and Clearance duration."
            );
            effect.overrideIcon = Utils.LoadSprite("clouds.png");
            var shortDrizzle = NewEffect<SeasonLengthEffectModel>("prestige23_drizzle", "[eots] shorter drizzle", "");
            shortDrizzle.amount = -0.25f;
            shortDrizzle.season = Season.Drizzle;
            var shortClearance = NewEffect<SeasonLengthEffectModel>("prestige23_clearance", "[eots] shorter clearance", "");
            shortClearance.amount = -0.25f;
            shortClearance.season = Season.Clearance;
            effect.rewards = new EffectModel[]{ shortDrizzle, shortClearance };
            effect.dynamicDescriptionArgs = new TextArg[0];
            diff.modifiers.Last().effect = effect;
        }

        private static void Done(){
            var settings = Serviceable.Settings;
            settings.effectsCache.cache = null;
            settings.effects.AddRangeToArray(effectsToAdd.ToArray());
            effectsToAdd.Clear();
            Serviceable.Settings.effectsCache.cache = null;
        }

        private static DifficultyModel NewDifficulty(string desc, bool addModifier = true){
            var settings = MainController.Instance.Settings;
            var diff = settings.difficulties.Last().Clone();
            diff.index += 1;
            diff.ascensionIndex += 1;
            diff.rewardsMultiplier += 0.1f;
            diff.scoreMultiplier += 0.1f;
            diff.expMultiplier += 0.1f;
            diff.name = $"EOTS Prestige {diff.ascensionIndex - 19}";
            var newMod = diff.modifiers.Last().Clone();
            newMod.name = $"[EOTS] {desc}";
            newMod.shortDesc = Utils.Text(desc);
            diff.modifiers = diff.modifiers.AddToArray(newMod);
            settings.difficulties = settings.difficulties.AddToArray(diff);
            return diff;
        }

        private static T NewEffect<T>(string key, string name, string desc) where T: EffectModel, new() {
            T effect = ScriptableObject.CreateInstance<T>();
            return SetupEffect(effect, key, name, desc);
        }

        private static T SetupEffect<T>(T effect, string key, string name, string desc) where T: EffectModel {
            var settings = MainController.Instance.Settings;
            effect.name = $"eots_{key}";
            if(name != null) effect.displayName = Utils.Text(name);
            if(desc != null) effect.description = Utils.Text(desc);
            effect.label = LabelModifier();
            settings.effects = settings.effects.AddToArray(effect);
            return effect;
        }

        private static LabelModel LabelModifier(){
            return MainController.Instance.Settings.difficulties[2].modifiers[0].effect.label;
        }

    }
}