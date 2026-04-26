using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 싱글톤(Singleton) 구현: 누구나 GameManager.Instance로 접근 가능합니다.
    public static GameManager Instance { get; private set; }

    // ==========================================
    // 외부에 공개되는 플레이어 생명주기 이벤트
    // (Enemy, UI, Camera 등이 GameManager의 내부 구조를 알 필요 없이 이 이벤트만 구독합니다)
    // ==========================================
    /// <summary>새 씬 로드 후 플레이어가 준비되었음을 알립니다. (UI 연결, 적 타겟 설정 등)</summary>
    public event Action<PlayerController> OnPlayerReady;
    /// <summary>플레이어가 사망했음을 알립니다. (적의 패트롤 복귀, BGM 변경 등)</summary>
    public event Action OnPlayerDeath;
    /// <summary>플레이어가 부활을 완료했음을 알립니다.</summary>
    public event Action OnPlayerRespawn;

    [Header("Player Setup")]
    public PlayerController player;

    // 현재 저장된 부활 위치 (체크포인트)
    private Vector2 currentRespawnPoint;

    private bool isRespawning = false;
    private bool waitingForUserChoice = false; // 유저 선택 대기 상태

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ==========================================
    // 씬 로드 이벤트 구독
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
        // 1. 타이틀 씬이라면 플레이어를 찾지 않고 무시합니다.
        if (scene.name == Define.SceneNames.Title)
        {
            UnsubscribeFromPlayer(); // 이전 씬의 구독 해제
            player = null;
            return;
        }

        // 2. 인게임 씬이라면 새로운 씬에 배치된 플레이어를 찾습니다.
        UnsubscribeFromPlayer(); // 혹시 모를 이전 구독 해제
        player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            // 3. 플레이어 사망 이벤트 구독 (중계용)
            SubscribeToPlayer();

            // 4. 코루틴으로 안전하게 세션 복구 + Ready 이벤트 발행
            StartCoroutine(InitializePlayerRoutine());
        }
    }

    private IEnumerator InitializePlayerRoutine()
    {
        // 플레이어와 포탈들이 각자의 Awake/Start를 마칠 때까지 1프레임 기다려줍니다.
        yield return null;

        // 1. 저장된 데이터가 있다면 위치/스탯 복구
        if (DataManager.Instance != null && DataManager.Instance.hasSavedData)
        {
            // 저장된 스탯(체력, 마나) 주입
            player.ImportSessionData(DataManager.Instance.sessionData);

            // 위치 복원 우선순위: 체크포인트 > 포탈
            // (체크포인트 저장 시 lastPortalID는 빈 문자열로 초기화되므로 자연스럽게 분기됨)
            string checkpointID = DataManager.Instance.sessionData.lastCheckpointID;
            string portalID = DataManager.Instance.sessionData.lastPortalID;

            if (!string.IsNullOrEmpty(checkpointID))
            {
                // 체크포인트로 부활
                Checkpoint cp = Checkpoint.FindCheckpointByID(checkpointID);
                if (cp != null)
                {
                    player.transform.position = cp.SpawnPosition;
                }
            }
            else if (!string.IsNullOrEmpty(portalID))
            {
                // 포탈로 도착 (씬 전환)
                Portal exitPortal = Portal.FindPortalByID(portalID);
                if (exitPortal != null)
                {
                    player.transform.position = exitPortal.spawnPoint.position;
                }
            }
        }

        // 2. 부활 지점을 현재 위치로 갱신
        currentRespawnPoint = player.transform.position;

        // 3. 모든 준비가 끝났음을 외부에 알림 (UI, 카메라, 적이 이 신호를 기다리고 있습니다)
        OnPlayerReady?.Invoke(player);
    }

    private void SubscribeToPlayer()
    {
        if (player == null || player.Health == null) return;
        player.Health.OnDeath += HandlePlayerDeathInternal;
    }

    private void UnsubscribeFromPlayer()
    {
        if (player == null || player.Health == null) return;
        player.Health.OnDeath -= HandlePlayerDeathInternal;
    }

    private void OnDestroy()
    {
        UnsubscribeFromPlayer();
    }

    /// <summary>
    /// 플레이어의 OnDeath 이벤트를 받아 내부 부활 처리 + 외부 이벤트 발행을 중계합니다.
    /// </summary>
    private void HandlePlayerDeathInternal()
    {
        // 외부에 사망 사실을 알림 (적들이 이걸 듣고 패트롤 모드로 돌아갑니다)
        OnPlayerDeath?.Invoke();

        // 내부 부활 처리 시작
        RespawnPlayer();
    }

    // 체크포인트가 GameManager에게 "여기 새로운 부활 지점이야!"라고 알려주는 함수
    public void UpdateRespawnPoint(Vector2 newPoint)
    {
        currentRespawnPoint = newPoint;

    }

    // 플레이어가 죽었을 때 호출될 함수
    public void RespawnPlayer()
    {
        if (isRespawning) return;
        StartCoroutine(RespawnCoroutine());
    }

    // UIManager에서 버튼을 눌렀을 때 호출해줄 함수
    public void ResumeRespawn()
    {
        waitingForUserChoice = false;
    }

    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;

        // 1. 사망 메뉴 표시 후 유저 선택 대기
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowDeathMenu(true);
            waitingForUserChoice = true;
        }

        while (waitingForUserChoice)
        {
            yield return null;
        }

        // 2. 화면 암전
        yield return StartCoroutine(UIManager.Instance.FadeOut(1f));

        // 3. 어둠 속에서 위치 이동 및 체력 복구
        if (player != null)
        {
            player.transform.position = currentRespawnPoint;
            player.Respawn();
        }

        // 4. 화면 다시 밝히기
        if (UIManager.Instance != null)
        {
            yield return StartCoroutine(UIManager.Instance.FadeIn(1f));
        }

        isRespawning = false;

        // 5. 부활 완료 이벤트 발행
        OnPlayerRespawn?.Invoke();
    }

    /// <summary>
    /// 씬을 떠날 때, 씬 전환 매니저의 요청을 받아 플레이어의 현재 상태를 포장해주는 함수입니다.
    /// </summary>
    public DataManager.GameData ExportPlayerSession()
    {
        if (player != null)
        {
            return player.ExportSessionData();
        }
        return new DataManager.GameData();
    }
}