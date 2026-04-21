using System.Collections.Generic;
using UnityEngine;

public class ServerPlayerSpawnPoints : Singleton<ServerPlayerSpawnPoints>
{
    [SerializeField]
    private List<GameObject> m_SpawnPoints;

    public GameObject GetRandomSpawnPoint()
    {
        if (m_SpawnPoints == null || m_SpawnPoints.Count == 0)
        {
            SetSpawnPoints();
        }

        if (m_SpawnPoints == null || m_SpawnPoints.Count == 0)
        {
            return null;
        }
        return m_SpawnPoints[Random.Range(0, m_SpawnPoints.Count)];
    }

    private void SetSpawnPoints()
    {
        if (m_SpawnPoints == null)
        {
            m_SpawnPoints = new();
        }
        m_SpawnPoints.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            m_SpawnPoints.Add(transform.GetChild(i).gameObject);
        }
    }
}
