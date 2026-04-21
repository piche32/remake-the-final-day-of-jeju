using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    [SerializeField] GameObject m_playerPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoadComplete;
        }
    }

    void OnSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        SpawnPlayerForClient(clientId);
    }

    void SpawnPlayerForClient(ulong clientId)
    {
        if (m_playerPrefab != null)
        {
            GameObject playerInstance = Instantiate(m_playerPrefab);

            if (playerInstance.TryGetComponent(out NetworkObject networkObject))
            {
                networkObject.SpawnAsPlayerObject(clientId, true);
            }
            else
            {
                Debug.LogError("Cannot find NetworkObject Component.");
            }
        }
        else
        {
            Debug.LogError("Player Prefab is not set.");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
        {
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoadComplete;
        }

        base.OnNetworkDespawn();
    }
}
