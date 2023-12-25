using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;

namespace DanceHealingOnShip.Patches;

internal class PlayerControllerBPatch
{
    private const float HealingCooldownTime = 60f;

    
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.StartPerformingEmoteClientRpc)), HarmonyPostfix]
    private static async void OnDancingOnShip(PlayerControllerB __instance)
    {
        string playerUsername = __instance.playerUsername;
        
        if (__instance.performingEmote && __instance.health < 99 && __instance.isInHangarShipRoom && 
            (!DanceHealingOnShip.ExecutedInstances.ContainsKey(playerUsername) || __instance.timeSincePlayerMoving - DanceHealingOnShip.ExecutedInstances[playerUsername] >= HealingCooldownTime))
        {
            
            if (DanceHealingOnShip.TokenSources.ContainsKey(playerUsername))
            {
                DanceHealingOnShip.TokenSources[playerUsername].Cancel();
            }
            
            
            if (!DanceHealingOnShip.HasShownMessage.ContainsKey(playerUsername) || !DanceHealingOnShip.HasShownMessage[playerUsername])
            {
                HUDManager.Instance.DisplayTip("Keep Dancing!", "Keep dancing to recover health!");
                DanceHealingOnShip.HasShownMessage[playerUsername] = true;
            }
            
            
            var cts = new CancellationTokenSource();
            DanceHealingOnShip.TokenSources[playerUsername] = cts;

            try
            {
                while (__instance.health < 99)
                {
                    __instance.health += 2;
                    await Task.Delay(1000, cts.Token);
                }
                HUDManager.Instance.DisplayTip("Full health!", "You are now at full health!");
                HUDManager.Instance.UpdateHealthUI(__instance.health, false);
                DanceHealingOnShip.ExecutedInstances[playerUsername] = __instance.timeSincePlayerMoving;
                DanceHealingOnShip.Mls.LogInfo(playerUsername +" has been healed");
            }
            catch (TaskCanceledException)
            {
                HUDManager.Instance.UpdateHealthUI(__instance.health);
            }
        }
    }
    
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.StopPerformingEmoteClientRpc)), HarmonyPostfix]
    private static async void CancelAnimation(PlayerControllerB __instance)
    {
        
        string playerUsername = __instance.playerUsername;
        await Task.Delay(1000);
        if (DanceHealingOnShip.TokenSources.ContainsKey(playerUsername) && !__instance.performingEmote && __instance.isInHangarShipRoom)
        {
            DanceHealingOnShip.TokenSources[playerUsername].Cancel();
        }
        
    }
    
    
}