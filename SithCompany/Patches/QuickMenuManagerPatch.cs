using HarmonyLib;
using SithCompany;
using SithCompany.Abilities;

// Target the QuickMenuManager class, which handles the in-game pause menu.
[HarmonyPatch(typeof(QuickMenuManager))]
internal class QuickMenuManagerPatch
{
    // Patch the method that runs when the player chooses to leave the game/lobby.
    [HarmonyPostfix]
    [HarmonyPatch(nameof(QuickMenuManager.LeaveGame))]
    static void CleanupOnManualExit()
    {
        // Call the cleanup method to destroy the Unity GameObject and clear the static C# reference.
        ForceAbility.CleanupIndicator();
        SithCompanyMod.mls.LogInfo("Indicator cleaned up due to manual game exit.");
    }
}