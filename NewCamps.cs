using System.Linq;
using System.Net.Mime;
using Eremite;
using Eremite.Buildings;
using Eremite.Model;
using Eremite.Model.Configs;
using Eremite.Model.Effects;
using Eremite.Services;
using HarmonyLib;
using QFSW.QC.Containers;
using UniRx;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Yoga;

namespace EyeOfTheStorm
{

    public static class GathererHutCreator {

        private static Settings Settings => Serviceable.Settings;

        public static void Patch(){
            CreatePrimitiveHut("Stonecutter's Camp");
            CreatePrimitiveHut("Harvester Camp");
            Done();
        }

        private static void CreatePrimitiveHut(string modelName){
            var originalModel = CloneModel(modelName);
            var model = (GathererHutModel) originalModel.replaces;
            AddSOsToSettings(model);
            AddAsBlueprint(originalModel);
            AddAsTraderPerk(originalModel);
        }

        private static GathererHutModel CloneModel(string modelName){
            var model = (GathererHutModel) Settings.GetBuilding(modelName);
            var result = model.Clone("Primitive " + model.Name);
            model.replaces = result;

            var primitiveModel = PrimitiveModel;
            result.description = primitiveModel.description;
            result.displayName = Utils.Text("Small " + result.displayName.Text);

            result.recipes = result.recipes.Select(recipe => CloneRecipe(recipe)).ToArray();
            ClonePrefab(result);
            return model;
        }

        public static int[] TO_HIDE_STONECUTTER => new int[]{6, 7, 8, 9, 35, 36, 66, 67, 111, 112, 116, 117, 124, 126, 127, 128};
        public static int[] TO_HIDE_HARVESTER => new int[]{7, 9, 11, 17, 18, 19, 21, 22, 23, 24, 25, 26, 63, 64, 65, 90, 91, 92, 93, 94, 95};

        private static void ClonePrefab(GathererHutModel model){
            var prefab = Object.Instantiate(model.prefab, null, false);
            Object.DontDestroyOnLoad(prefab);
            prefab.SetPosition(new Vector3(-1000, 0, -1000));
            model.prefab = prefab;
            prefab.name = model.Name;
            // Strangely, the model field for all Gatherer huts is set to the same stonecutter camp model
            // Which leads me to believe this field is unused. Thus, setting it is unneeded
            // prefab.model = model;
            var go = model.prefab.gameObject;
            var meshContent = go.transform.GetChild(0).GetChild(0);
            var toHide = prefab.name.Contains("Harvester")? TO_HIDE_HARVESTER : TO_HIDE_STONECUTTER;
            foreach (int child in toHide)  meshContent.GetChild(child).SetActive(false);

            if( prefab.name.Contains("Harvester")){
                AddComp(meshContent.GetChild(12), 0, 0, 0.45f);
                AddComp(meshContent.GetChild(12), 0, 0, 0.9f);
                AddComp(meshContent.GetChild(13), 0, 0, 0.45f);
                AddComp(meshContent.GetChild(13), 0, 0, 0.9f);
            } else {
                AddComp(meshContent.GetChild(118), 0.5f, 0, 0);
                AddComp(meshContent.GetChild(125), -0.5f, 0, 0);
            }
        }

        private static void AddComp(Transform content, float x, float y, float z){
            var clone = Object.Instantiate(content, content.parent, false);
            clone.position = clone.position + new Vector3(x, y, z);
        }

        private static GathererHutModel PrimitiveModel => (GathererHutModel) Settings.GetBuilding("Primitive Trapper's Camp");

        private static GathererHutRecipeModel CloneRecipe(GathererHutRecipeModel tier1) {
            var tier0 = PrimitiveModel.recipes[0];
            var result = tier1.Clone(tier1.Name + " T0");
            result.productionTime *= 2;
            result.grade = tier0.grade;
            result.gradeDesc = tier0.gradeDesc;
            return result;
        }

        private static void AddSOsToSettings(GathererHutModel model){
            Utils.AddInPlace(ref Settings.Buildings, model);
            Settings.gatherersHutsRecipes = Settings.gatherersHutsRecipes.AddRangeToArray(model.recipes);
            Settings.recipes = Settings.recipes.AddRangeToArray(model.recipes);

        }

        private static void AddAsBlueprint(GathererHutModel model){
            var ascensionBlueprintConfig = Settings.blueprintsConfigs[3];
            if(!ascensionBlueprintConfig.Name.Equals("Blueprints Config - Ascension"))
                Plugin.Error($"Ascension blueprint config is of wrong name: {ascensionBlueprintConfig.Name}");
            AddBlueprintToSet(model, ascensionBlueprintConfig, 0);
            AddBlueprintToSet(model, ascensionBlueprintConfig, 1);
            AddBlueprintToSet(model, ascensionBlueprintConfig, 11);
            if(model.Name.Contains("Harvester")){ // Stonecutter is added to the wildcards by default
                Utils.AddInPlace(ref ascensionBlueprintConfig.wildcards, new BuildingWeightedChance(){building=model});
            }
        }

        private static void AddBlueprintToSet(GathererHutModel model, BiomeBlueprintsConfig config, int setIndex){
            var set = config.blueprints[5].sets[setIndex];
            if(!set.Name.Contains("Food Camps") && !set.Name.Contains("_Master"))
                Plugin.Error($"Blueprint set is not of the Food Camps type: {set.Name}");
            Utils.AddInPlace(ref set.buildings, new BuildingWeightedChance(){building=model});
        }

        private static void AddAsTraderPerk(GathererHutModel model){
            var effect = CreateEffectFor(model);
            AddToTrader(effect, 0); // Human
            AddToTrader(effect, 6); // Birdman
            int otherTraderIndex = model.Name.Contains("Harvester")? 1 : 2; // Frog or Beaver
            AddToTrader(effect, otherTraderIndex);
        }

        private static void AddToTrader(EffectModel effect, int traderIndex){
            var trader = Settings.traders[traderIndex];
            var drop = new EffectDrop(){chance=50f, reward=effect};
            Utils.AddInPlace(ref trader.merchandise, drop);
        }

        private static EffectModel CreateEffectFor(GathererHutModel model){
            var effect = Content.NewEffect<BuildingEffectModel>("effect_" + model.Name, model.Name + " Blueprint", "{1}");
            effect.showBlueprintTooltip = true;
            effect.building = model;
            effect.CantToBePicked =  true;
            effect.HasToBePicked = false;
            effect.HasToBeUnlocked = false; // Eremite effects use true but I just want it to work
            effect.rarity = EffectRarity.Epic;
            effect.tradingBuyValue = 175;
            return effect;
        }

        private static void Done(){
            Settings.gatherersHutsRecipesCache.cache = null;
            Settings.buildingsCache.cache = null;
        }

        public static void UpdateEssentialBuildings(){
            var content = Serviceable.GameContentService;
            content.Lock(Settings.GetBuilding("Stonecutter's Camp"));
            content.Lock(Settings.GetBuilding("Harvester Camp"));
            content.Unlock(Settings.GetBuilding("Primitive Stonecutter's Camp"));
            content.Unlock(Settings.GetBuilding("Primitive Harvester Camp"));
        }

        public static void DumpBlueprints(int index){ // For debug purposes only
            var blueprints = Settings.blueprintsConfigs[index];
            Plugin.Log($"Dumping information for {blueprints.Name}");
            foreach(var bp in blueprints.blueprints){
                Plugin.Log($"BlueprintRange {bp.range.x}-{bp.range.y}: has sets:");
                foreach(var set in bp.sets){
                    Plugin.Log($"  {set.Name} ({set.weight}), buildings:");
                    foreach(var building in set.buildings){
                        Plugin.Log($"    {building.building.Name} ({building.weight})");
                    }
                }
            }
            Plugin.Log("Dumping wildcards:");
            foreach(var building in blueprints.wildcards){
                Plugin.Log($"{building.building.Name} ({building.weight})");
            }
        }
    }
}