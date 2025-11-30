using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text;

namespace SithCompany.Util
{
    internal class Scripts
    {
        public static void PerformEmoteProgrammatically(PlayerControllerB player, int emoteID)
        {
            if (((player.IsOwner && player.isPlayerControlled && (!player.IsServer || player.isHostPlayerObject)) || player.isTestingPlayer) && player.CheckConditionsForEmote() && !(player.timeSinceStartingEmote < 0.5f))
            {
                player.timeSinceStartingEmote = 0f;
                player.performingEmote = true;
                player.playerBodyAnimator.SetInteger("emoteNumber", emoteID);
                player.StartPerformingEmoteServerRpc();
            }
        }
    }
}
