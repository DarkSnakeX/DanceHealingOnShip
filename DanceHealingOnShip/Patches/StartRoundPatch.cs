using HarmonyLib;

namespace DanceHealingOnShip.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartRoundPatch
{
    

    [HarmonyPatch("StartGame")]
    [HarmonyPostfix]
    private static void StartGame()
    {
        Fullreset();
    }

    [HarmonyPatch("EndOfGame")]
    [HarmonyPostfix]
    private static void EndOfGame()
    {
        Fullreset();
    }

    [HarmonyPatch("EndOfGameClientRpc")]
    [HarmonyPostfix]
    private static void EndOfGameClientRpc()
    {
        Fullreset();
    }

    private static void Fullreset()
    {
        DanceHealingOnShip.ExecutedInstances.Clear();
        DanceHealingOnShip.TokenSources.Clear();
        DanceHealingOnShip.Mls.LogInfo("Diccionary reseted");
    }
        
    
    
}