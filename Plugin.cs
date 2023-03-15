﻿using BepInEx;
using HarmonyLib;

namespace EyeOfTheStorm
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony;
        private static Plugin Instance;
        public static void Log(object obj) => Instance.Logger.LogInfo(obj);

        private void Awake()
        {
            Instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            harmony = new Harmony("EyeOfTheStorm");
            harmony.PatchAll(typeof(Patches));
        }
    }
}
