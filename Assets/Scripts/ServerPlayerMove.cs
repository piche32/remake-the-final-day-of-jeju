using UnityEngine;
using Unity.Netcode;

public class ServerPlayerMove : NetworkBehaviour, INetworkInitializable
{
    public int InitializationPriority => 0; //PlayerMove보다 먼저 실행되어야 함.
    public void NetworkInitialize()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        GameObject spawnPoint = ServerPlayerSpawnPoints.Instance.GetRandomSpawnPoint();
        Vector3 spawnPosition = spawnPoint ? spawnPoint.transform.position : Vector3.zero;
        transform.position = spawnPosition;
    }
}
