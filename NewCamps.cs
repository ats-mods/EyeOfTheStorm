using System;
using System.Linq;
using Eremite.Buildings;
using Eremite.Model;
using Eremite.Model.Configs;
using Eremite.Services;
using HarmonyLib;
using UniRx;
using UnityEngine;

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
        }

        private static GathererHutModel CloneModel(string modelName){
            var model = (GathererHutModel) Settings.GetBuilding(modelName);
            var result = model.Clone("Primitive " + model.Name);
            model.replaces = result;

            var primitiveModel = PrimitiveModel;
            result.description = primitiveModel.description;
            result.displayName = Utils.Text("Small " + result.displayName.Text);

            result.recipes = result.recipes.Select(recipe => CloneRecipe(recipe)).ToArray();
            return model;
        }

        private static void ClonePrefab(GathererHutModel model){
            var prefab = (GathererHut) model.prefab.Clone();
            model.prefab = prefab;
            // Strangely, the model field for all Gatherer huts is set to the same stonecutter camp model
            // Which leads me to believe this field is unused. Thus, setting it is unneeded
            // prefab.model = model;
        }

        private static GathererHutModel PrimitiveModel => (GathererHutModel) Settings.GetBuilding("Primitive Trapper's Camp");

        private static GathererHutRecipeModel CloneRecipe(GathererHutRecipeModel tier1) {
            var tier0 = PrimitiveModel.recipes[0];
            var result = tier1.Clone(tier1.Name + " T0");
            result.productionTime = tier0.productionTime;
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