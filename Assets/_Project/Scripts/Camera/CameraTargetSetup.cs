using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraTargetSetup : MonoBehaviour
{
    [Header("Camera Bounds Settings")]
    [Tooltip("씬에서 카메라 영역 제한용 Collider를 가진 오브젝트의 태그")]
    [SerializeField] private string cameraBoundsTag = "CameraBounds";

    private CinemachineCamera cam;
    private CinemachineConfiner2D confiner;


    private void Awake()
    {
        cam = GetComponent<CinemachineCamera>();

        confiner = GetComponent<CinemachineConfiner2D>();
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

        // 씬에 배치된 카메라 영역 콜라이더 자동 연결
        AssignBounds();
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

    /// <summary>
    /// 씬에서 cameraBoundsTag를 가진 오브젝트의 Collider2D를 찾아
    /// CinemachineConfiner2D의 Bounding Shape에 할당합니다.
    /// </summary>
    private void AssignBounds()
    {
        if (confiner == null) return;

        // 태그로 검색 (Find보다 빠르고 명시적)
        GameObject boundsObject = GameObject.FindGameObjectWithTag(cameraBoundsTag);

        if (boundsObject == null)
        {
            // 이 씬에는 Bounds가 없을 수도 있음 (예: 타이틀 씬)
            // 경고만 출력하고 계속 진행
            Debug.LogWarning($"[CameraTargetSetup] '{cameraBoundsTag}' 태그를 가진 오브젝트가 없습니다. 카메라 영역 제한이 비활성화됩니다.");
            confiner.BoundingShape2D = null;
            return;
        }

        Collider2D boundsCollider = boundsObject.GetComponent<Collider2D>();
        if (boundsCollider == null)
        {
            Debug.LogError($"[CameraTargetSetup] '{boundsObject.name}'에 Collider2D가 없습니다.");
            return;
        }

        // Confiner에 할당 + 캐시 무효화 (씬 변경 시 이전 영역 잔재 제거)
        confiner.BoundingShape2D = boundsCollider;
        confiner.InvalidateBoundingShapeCache();
    }
}