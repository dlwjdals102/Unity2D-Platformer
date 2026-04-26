using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraTargetSetup : MonoBehaviour
{
    /*private void Start()
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
    }*/

    private CinemachineCamera cam;

    private void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReady += HandlePlayerReady;

            // 이미 준비된 플레이어가 있다면 즉시 연결
            if (GameManager.Instance.player != null)
            {
                AssignTarget(GameManager.Instance.player.transform);
            }
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReady -= HandlePlayerReady;
        }
    }

    private void HandlePlayerReady(PlayerController player)
    {
        if (player != null) AssignTarget(player.transform);
    }

    private void AssignTarget(Transform target)
    {
        if (cam != null && target != null)
        {
            cam.Target.TrackingTarget = target;
        }
    }
}