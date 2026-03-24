using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;
using System.Collections.Generic;
using System.Linq;


public class PlayerManager : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private GameObject m_playerCameraPrefab;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SetFollowCamera();

        List<INetworkInitializable> networkInitializables = GetComponentsInChildren<INetworkInitializable>().ToList();
        networkInitializables.Sort((a, b) => a.InitializationPriority - b.InitializationPriority);
        foreach (INetworkInitializable networkInitializable in networkInitializables)
        {
            networkInitializable.NetworkInitialize();
        }
    }

    void SetFollowCamera()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        // 인스펙터에서 카메라 프리팹을 잘 할당했는지 확인합니다.
        if (m_playerCameraPrefab != null)
        {
            GameObject playerCamera = Instantiate(m_playerCameraPrefab);
            CinemachineCamera cinemachineCamera = playerCamera.GetComponent<CinemachineCamera>();
            cinemachineCamera.Target.TrackingTarget = transform;
            cinemachineCamera.Target.LookAtTarget = transform;
        }
    }
}
