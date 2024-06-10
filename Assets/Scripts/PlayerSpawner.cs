using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Basic player spawn based on the main shared mode sample.
/// </summary>
public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            var resultingPlayer = Runner.Spawn(PlayerPrefab);
            
            FusionConnector connector = GameObject.FindObjectOfType<FusionConnector>();
            if (connector != null)
            {
                var testPlayer = resultingPlayer.GetComponent<ASBPlayer>();

                string playerName = connector.LocalPlayerName;

                if (string.IsNullOrEmpty(playerName))
                    testPlayer.PlayerName = "Player " + resultingPlayer.StateAuthority.PlayerId;
                else
                    testPlayer.PlayerName = playerName;

                // TODO Active assign random avatar 3D model
                // Assigns a random avatar
                testPlayer.ChosenAvatar = Random.Range(0, testPlayer.avatarSprites.Length);

                //testPlayer.transform.SetParent(FusionConnector.Instance.playerContainer[resultingPlayer.StateAuthority.PlayerId - 1], false);
                Vector3 position = FusionConnector.Instance.playerContainer[resultingPlayer.StateAuthority.PlayerId - 1].position;
                testPlayer.transform.position = position;
            }
        }

        FusionConnector.Instance?.OnPlayerJoin(Runner);
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (ASBPlayer.LocalPlayer != null)
            ASBPlayer.LocalPlayer.IsMasterClient = Runner.IsSharedModeMasterClient;
    }
}
