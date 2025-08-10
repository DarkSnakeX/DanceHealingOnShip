using System.Threading;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace DanceHealingOnShip.Patches;

internal class PlayerControllerBPatch
{
    private const float HealingCooldownTime = 60f;

    [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.StartPerformingEmoteClientRpc)), HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static async void OnDancingOnShip(PlayerControllerB __instance)
    {
        string playerUsername = __instance.playerUsername;
        
        bool isCooldownActive = DanceHealingOnShip.ExecutedInstances.TryGetValue(playerUsername, out float lastExecutionTime) &&
                                Time.time - lastExecutionTime < HealingCooldownTime;

        if (__instance.performingEmote && __instance.health < 100 && __instance.isInHangarShipRoom && !isCooldownActive)
        {
            if (__instance.isPlayerDead)
            {
                DanceHealingOnShip.Mls.LogInfo(playerUsername + " is dead and cannot heal.");
                return;
            }

            if (DanceHealingOnShip.TokenSources.TryGetValue(playerUsername, out var existingCts))
            {
                existingCts.Cancel();
                DanceHealingOnShip.Mls.LogInfo(playerUsername + " cancelled their previous healing task.");
            }

            var cts = new CancellationTokenSource();
            DanceHealingOnShip.TokenSources[playerUsername] = cts;

            try
            {
                while (__instance.health < 100 && __instance.performingEmote && !cts.Token.IsCancellationRequested)
                {
                    await Task.Delay(1000, cts.Token);
                    if (!__instance.performingEmote || cts.Token.IsCancellationRequested) break;

                    __instance.health += 10;
                    if (__instance.health > 100) __instance.health = 100;
                    
                    __instance.DamagePlayer(-10, false, true, CauseOfDeath.Unknown, 0, false, Vector3.zero);

                    if (__instance.health >= 20)
                    {
                        __instance.criticallyInjured = false;
                    }
                    
                    if (__instance == GameNetworkManager.Instance.localPlayerController)
                    {
                        HUDManager.Instance.UpdateHealthUI(__instance.health, false);
                    }
                }

                if (__instance.health >= 100)
                {
                    DanceHealingOnShip.ExecutedInstances[playerUsername] = Time.time;
                    DanceHealingOnShip.Mls.LogInfo(playerUsername + " has been completed healed. Cooldown for 60s.");
                    if (__instance == GameNetworkManager.Instance.localPlayerController)
                    {
                        HUDManager.Instance.DisplayTip("Full health by dancing!", "You are now at full health! Try to avoid damage for 60 seconds to heal again.");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                DanceHealingOnShip.Mls.LogInfo(playerUsername + " healing was cancelled or is currently on progress.");
            }
            finally
            {
                if (__instance == GameNetworkManager.Instance.localPlayerController)
                {
                    HUDManager.Instance.UpdateHealthUI(__instance.health, false);
                }
                DanceHealingOnShip.TokenSources.Remove(playerUsername);
            }
        }
        else
        {
            DanceHealingOnShip.Mls.LogInfo(playerUsername + " is either not dancing, at full health, on cooldown, or not in the hangar room.");
        }
    }
}
