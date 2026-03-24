using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;


[DefaultExecutionOrder(0)] //ClientNetworkTransform 전에 실행
public class PlayerManager : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private GameObject m_playerCameraPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        SetFollowCamera();
        base.OnNetworkSpawn();
    }

    void SetFollowCamera()
    {
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
