using System.Collections.Generic;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using DanceHealingOnShip.Patches;
using HarmonyLib;

namespace DanceHealingOnShip
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DanceHealingOnShip : BaseUnityPlugin
    {
        private readonly Harmony _harmony = new Harmony(PluginInfo.PLUGIN_GUID);

        public static DanceHealingOnShip Instance;

        internal static ManualLogSource Mls;
        
        public static Dictionary<string, float> ExecutedInstances;
        public static Dictionary<string, CancellationTokenSource> TokenSources;
        public static Dictionary<string, bool> HasShownMessage;
        

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            Mls = BepInEx.Logging.Logger.CreateLogSource(PluginInfo.PLUGIN_GUID);
            Mls.LogInfo("DanceHealingOnShip is loaded - version " + PluginInfo.PLUGIN_VERSION);
            
            _harmony.PatchAll(typeof(DanceHealingOnShip));
            _harmony.PatchAll(typeof(PlayerControllerBPatch));
            _harmony.PatchAll(typeof(StartRoundPatch));

            ExecutedInstances = new Dictionary<string, float>();
            HasShownMessage = new Dictionary<string, bool>();
            TokenSources = new Dictionary<string, CancellationTokenSource>();


        }
        
    }
}