// SithCompany/Patches/StartOfRoundPatch.cs
using HarmonyLib;
using SithCompany.Abilities;

namespace SithCompany.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(StartOfRound.ShipLeave))]
        static void CleanupOnRoundEnd()
        {
            // Call the cleanup method when the ship leaves the planet.
            ForceAbility.CleanupIndicator();
        }
    }
}