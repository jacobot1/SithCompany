using GameNetcodeStuff;
using SithCompany.Util;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SithCompany.Abilities
{
    internal class LightningAbility
    {
        public static void ForceLightning(PlayerControllerB player)
        {
            if (player.sprintMeter < SithCompanyMod.configLightningMinimumStamina.Value)
            {
                return;
            }
            // Perform point emote
            Scripts.PerformEmoteProgrammatically(player, 2);

            // Simplifications
            Vector3 playerPosition = player.transform.position;
            Vector3 strikeOrigin = playerPosition + player.transform.forward * 1f + Vector3.up * 2f;
            Vector3 strikePosition = player.gameplayCamera.transform.position + player.gameplayCamera.transform.forward * SithCompanyMod.configLightningLength.Value;

            // Damage Selector
            if (SithCompanyMod.configKillEnemies.Value)
            {
                EZDamage.API.KillEnemies(strikePosition, SithCompanyMod.configLightningDamageRadius.Value);
            }
            if (SithCompanyMod.configKillPlayers.Value)
            {
                EZDamage.API.KillPlayers(strikePosition, SithCompanyMod.configLightningDamageRadius.Value, CauseOfDeath.Electrocution);
            }

            // Fire lightning bolt
            EZLightning.API.Strike(strikePosition, strikeOrigin, 1f, 0.5f, 0.5f, 0, -1f, minCount: 0, maxCount: 1);

            // Dock sprint
            player.sprintMeter = Mathf.Clamp(player.sprintMeter - SithCompanyMod.configLightningStaminaDock.Value, 0f, 1f);
        }
    }
}
