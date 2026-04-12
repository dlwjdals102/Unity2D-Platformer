using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraTargetSetup : MonoBehaviour
{
    private void Start()
    {
        var cam = GetComponent<CinemachineCamera>();
        var player = FindFirstObjectByType<PlayerController>();

        if (cam != null && player != null)
        {
            cam.Target.TrackingTarget = player.transform;
        }
        else
        {
            Debug.LogWarning("[CameraTargetSetup] 씬에 카메라나 플레이어가 없어 타겟을 지정할 수 없습니다.");
        }
    }
}