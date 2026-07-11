using HarmonyLib;
using Pigeon.Movement;
using UnityEngine;

[HarmonyPatch]
public static class ToggleAimPatches
{
    [HarmonyPatch(typeof(PlayerInput), "Initialize")]
    [HarmonyPostfix]
    public static void PlayerInputInitializePostfix()
    {
        MovementTogglesPlugin.aimAction = PlayerInput.Controls?.Player.Aim;
        MovementTogglesPlugin.ConfigureAimSubscription();
    }

    [HarmonyPatch(typeof(Gun), "OnAimInputPerformed")]
    [HarmonyPrefix]
    public static bool SkipPrefix()
    {
        return !MovementTogglesPlugin.toggleAim.Value;
    }

    [HarmonyPatch(typeof(Gun), "OnAimInputCancelled")]
    [HarmonyPrefix]
    public static bool SkipPrefixCancelled()
    {
        return !MovementTogglesPlugin.toggleAim.Value;
    }

    [HarmonyPatch(typeof(Gun), "HandleAim")]
    [HarmonyPrefix]
    public static void HandleAimPrefix(Gun __instance)
    {
        if (MovementTogglesPlugin.toggleAim.Value)
        {
            MovementTogglesPlugin.isAimInputHeldField.SetValue(__instance, MovementTogglesPlugin.isAimToggled);
            if (MovementTogglesPlugin.isAimToggled)
            {
                MovementTogglesPlugin.lastPressedAimTimeField.SetValue(__instance, Time.time);
            }
        }
    }

    [HarmonyPatch(typeof(Gun), "Update")]
    [HarmonyPostfix]
    public static void UpdatePostfix(Gun __instance)
    {
        if (MovementTogglesPlugin.toggleAim.Value)
        {
            bool isAiming = (bool)MovementTogglesPlugin.isAimingGetter.Invoke(__instance, null);
            bool wantsToFire = (bool)MovementTogglesPlugin.wantsToFireGetter.Invoke(__instance, null);
            float lastFireTime = (float)MovementTogglesPlugin.lastFireTimeGetter.Invoke(__instance, null);
            float lastPressedFireTime = (float)MovementTogglesPlugin.lastPressedFireTimeField.GetValue(__instance);
            Player player = (Player)MovementTogglesPlugin.playerField.GetValue(__instance);
            if (player != null && !isAiming && !wantsToFire && Time.time - Mathf.Max(lastFireTime, lastPressedFireTime) > 0.5f)
            {
                player.ResumeSprint();
            }
        }
    }

    [HarmonyPatch(typeof(Player), "Resurrect_ClientRpc")]
    [HarmonyPostfix]
    public static void ResetTogglePostfix()
    {
        MovementTogglesPlugin.isAimToggled = false;
    }
}
