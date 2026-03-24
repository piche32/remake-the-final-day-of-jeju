using UnityEngine;
using Unity.Netcode;

[DefaultExecutionOrder(0)] //ClientNetworkTransform 전에 실행
public class ServerPlayerMove : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        SpawnPlayer();
        base.OnNetworkSpawn();
    }

    void SpawnPlayer()
    {
        GameObject spawnPoint = ServerPlayerSpawnPoints.Instance.GetRandomSpawnPoint();
        Vector3 spawnPosition = spawnPoint ? spawnPoint.transform.position : Vector3.zero;
        transform.position = spawnPosition;
    }
}
