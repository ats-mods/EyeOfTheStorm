using System;
using System.Linq;
using Eremite.Buildings;
using Eremite.Characters.Villagers;
using Eremite.Model;
using Eremite.Services;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace EyeOfTheStorm
{

    public static class GathererHutCreator {

        private static Settings Settings => Serviceable.Settings;

        public static void Patch(){
            var primitiveStonecutter = CloneModel("Stonecutter's Camp");
            var primitiveHarvester = CloneModel("Harvester Camp");
            AddSOsToSettings(primitiveStonecutter);
            AddSOsToSettings(primitiveHarvester);
            Done();
        }

        private static GathererHutModel CloneModel(string modelName){
            var model = (GathererHutModel) Settings.GetBuilding(modelName);
            var result = model.Clone("Primitive " + model.Name);
            model.replaces = result;

            var primitiveModel = PrimitiveModel;
            result.description = primitiveModel.description;
            result.displayName = Utils.Text("Small " + result.displayName.Text);

            result.recipes = result.recipes.Select(recipe => CloneRecipe(recipe)).ToArray();
            
            return result;
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
            Settings.Buildings = Settings.Buildings.AddToArray(model);
            Settings.gatherersHutsRecipes = Settings.gatherersHutsRecipes.AddRangeToArray(model.recipes);
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
        }
    }
}