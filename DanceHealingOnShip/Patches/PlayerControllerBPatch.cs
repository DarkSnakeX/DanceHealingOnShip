using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;

namespace DanceHealingOnShip.Patches;

internal class PlayerControllerBPatch
{
    private const float HealingCooldownTime = 60f;

    
    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.PerformEmote)), HarmonyPostfix]
    private static async void OnDancingOnShip(PlayerControllerB __instance)
    {
        string playerUsername = __instance.playerUsername;
        
        
        if (__instance.performingEmote && __instance.health < 95 && __instance.isInHangarShipRoom && 
            (!DanceHealingOnShip.ExecutedInstances.ContainsKey(playerUsername) || __instance.timeSincePlayerMoving - DanceHealingOnShip.ExecutedInstances[playerUsername] >= HealingCooldownTime))
        {
            
            if (DanceHealingOnShip.TokenSources.ContainsKey(playerUsername))
            {
                DanceHealingOnShip.TokenSources[playerUsername].Cancel();
                DanceHealingOnShip.Mls.LogInfo(playerUsername +" removed before healing action");
            }
            
            
            
            if (!DanceHealingOnShip.HasShownMessage.ContainsKey(playerUsername) || !DanceHealingOnShip.HasShownMessage[playerUsername])
            {
                /*HUDManager.Instance.DisplayTip("Keep Dancing!", "Keep dancing to recover health!");*/
                DanceHealingOnShip.HasShownMessage[playerUsername] = true;
            }
            
            
            var cts = new CancellationTokenSource();
            DanceHealingOnShip.TokenSources[playerUsername] = cts;

            try
            {
                while (__instance.health <= 95 && __instance.performingEmote)
                {
                    __instance.health += 5;
                    await Task.Delay(1000, cts.Token);
                }

                if (__instance.health == 100)
                {
                    DanceHealingOnShip.ExecutedInstances[playerUsername] = __instance.timeSincePlayerMoving;
                }
                if (__instance.health >= 20)
                {
                    __instance.criticallyInjured = false;
                }
                /*HUDManager.Instance.DisplayTip("Full health!", "You are now at full health!");*/
                /*HUDManager.Instance.UpdateHealthUI(__instance.health);*/
                DanceHealingOnShip.Mls.LogInfo(playerUsername +" has been healed");
            }
            catch (TaskCanceledException)
            {
                /*HUDManager.Instance.UpdateHealthUI(__instance.health);*/
                DanceHealingOnShip.Mls.LogInfo(playerUsername +" has stopped healing");
                DanceHealingOnShip.TokenSources.Remove(playerUsername);
            }
        }
        else
        {
            DanceHealingOnShip.Mls.LogInfo(playerUsername +" is not in the ship or is not injured");
        }
    }
    


}