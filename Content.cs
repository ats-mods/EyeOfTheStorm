using Eremite.Controller;
using Eremite.Model;
using Eremite.Model.Configs;
using Eremite.Model.Effects;
using Eremite.Model.Orders;
using Eremite.WorldMap.UI;
using HarmonyLib;
using QFSW.QC.Containers;
using System.Linq;

namespace EyeOfTheStorm
{
    public class Content {
        public static void AddPrestigeDifficulties(){
            Prestige21();
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

        private static DifficultyModel NewDifficulty(string desc){
            var settings = MainController.Instance.Settings;
            var diff = settings.difficulties.Last().Clone();
            diff.index += 1;
            diff.ascensionIndex += 1;
            diff.name = $"EOTS Prestige {diff.ascensionIndex - 19}";
            var newMod = diff.modifiers.Last().Clone();
            newMod.name = $"[EOTS] {desc}";
            newMod.shortDesc = Utils.Text(desc);
            diff.modifiers = diff.modifiers.AddToArray(newMod);
            settings.difficulties = settings.difficulties.AddToArray(diff);
            return diff;
        }

        private static T NewEffect<T>(string key, string name, string desc) where T: EffectModel, new() {
            T effect = new T();
            var settings = MainController.Instance.Settings;
            effect.name = $"eots_{key}";
            effect.displayName = Utils.Text(name);
            effect.description = Utils.Text(desc);
            effect.label = LabelModifier();
            settings.effects = settings.effects.AddToArray(effect);
            return effect;
        }

        private static LabelModel LabelModifier(){
            return MainController.Instance.Settings.difficulties[2].modifiers[0].effect.label;
        }

    }
}