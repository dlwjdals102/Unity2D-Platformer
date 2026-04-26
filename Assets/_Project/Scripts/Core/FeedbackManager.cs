using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    private CinemachineImpulseSource impulseSource;
    private bool isHitStopping = false;

    [Header("Camera Zoom Settings (Cinemachine 3)")]
    [Tooltip("보스전에서 사용할 시네머신 카메라를 연결해주세요.")]
    public CinemachineCamera bossVirtualCamera;
    private float defaultOrthoSize;
    private Coroutine zoomCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 컴포넌트 자동 할당
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    // ==========================================
    // 씬 로드 이벤트 구독 (새로운 카메라 자동 추적)
    // ==========================================
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == Define.SceneNames.Title) return; // 타이틀 씬은 무시합니다.

        // 새로운 씬에 배치된 시네머신 카메라를 찾아냅니다.
        bossVirtualCamera = FindFirstObjectByType<CinemachineCamera>();

        if (bossVirtualCamera != null)
        {
            defaultOrthoSize = bossVirtualCamera.Lens.OrthographicSize;
            Debug.Log($"[{scene.name}] 피드백 매니저가 새 시네머신 카메라를 찾았습니다.");
        }
        else
        {
            Debug.LogWarning($"[{scene.name}] 시네머신 카메라가 없습니다!");
        }
    }


    private void Start()
    {
        // 시작할 때 카메라의 기본 시야(줌) 값을 기억해둡니다.
        if (bossVirtualCamera != null)
        {
            defaultOrthoSize = bossVirtualCamera.Lens.OrthographicSize;
        }
    }


    // 1. 타격 정지 (Hit-Stop)
    public void TriggerHitStop(float duration = 0.05f)
    {
        if (isHitStopping) return;
        StartCoroutine(HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        isHitStopping = true;
        // 완전히 0으로 멈추기보다 0.1 정도로 아주 느리게 흘러가게 하는 것이 타격감이 더 쫀득합니다.
        Time.timeScale = 0.1f;

        // Time.timeScale의 영향을 받지 않는 '현실 시간' 기준으로 대기합니다.
        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        isHitStopping = false;
    }

    // 2. 카메라 흔들림 (Camera Shake)
    public void TriggerCameraShake(float force = 1f)
    {
        if (impulseSource != null)
        {
            // 설정된 진동 패턴에 힘을 곱해서 발생시킵니다.
            impulseSource.GenerateImpulseWithForce(force);
        }
    }

    // ==========================================
    // 2. 보스 포효 통합 연출 (흔들림 + 줌아웃)
    // ==========================================
    public void TriggerBossRoarFeedback(float shakeVelocity, float targetZoomSize, float zoomDuration)
    {
        // 1. 임펄스 소스를 통한 화면 흔들림 발생
        if (impulseSource != null)
        {
            // 시네머신 3의 규격에 맞게 기본 형태의 임펄스 생성
            impulseSource.GenerateImpulseWithForce(shakeVelocity);
        }

        // 2. 부드러운 줌 아웃 코루틴 실행
        if (bossVirtualCamera != null)
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(ZoomCameraRoutine(targetZoomSize, zoomDuration));
        }
    }

    // 전투 종료 시 시야 복구용 함수
    public void ResetCameraZoom(float zoomDuration = 1f)
    {
        if (bossVirtualCamera != null)
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(ZoomCameraRoutine(defaultOrthoSize, zoomDuration));
        }
    }

    // 시네머신 3의 Lens 구조체를 안전하게 조작하는 부드러운 줌 코루틴
    private IEnumerator ZoomCameraRoutine(float targetSize, float duration)
    {
        float startSize = bossVirtualCamera.Lens.OrthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 시네머신 3에서는 Lens 구조체를 복사해서 값을 바꾸고 다시 덮어씌워야 합니다.
            var lens = bossVirtualCamera.Lens;
            lens.OrthographicSize = Mathf.SmoothStep(startSize, targetSize, elapsed / duration);
            bossVirtualCamera.Lens = lens;

            yield return null;
        }

        // 최종 값 확정
        var finalLens = bossVirtualCamera.Lens;
        finalLens.OrthographicSize = targetSize;
        bossVirtualCamera.Lens = finalLens;
    }

    // VFX 소환 및 2D 방향 처리 래퍼 함수
    public void SpawnVFX(string poolTag, Vector3 position, float facingDirection)
    {
        if (ObjectPoolManager.Instance == null) return;

        // 풀에서 오브젝트를 꺼냅니다.
        GameObject vfx = ObjectPoolManager.Instance.SpawnFromPool(poolTag, position, Quaternion.identity);

        if (vfx != null)
        {
            // 바라보는 방향(facingDirection)이 -1이면 스케일을 반전시켜 VFX도 왼쪽을 보게 만듭니다.
            Vector3 localScale = vfx.transform.localScale;
            vfx.transform.localScale = new Vector3(Mathf.Abs(localScale.x) * facingDirection, localScale.y, localScale.z);
        }
    }
}