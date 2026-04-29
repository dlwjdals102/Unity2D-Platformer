using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ==========================================
    // 외부에 공개되는 플레이어 생명주기 이벤트
    // (Enemy, UI, Camera 등이 GameManager의 내부 구조를 알 필요 없이 이 이벤트만 구독합니다)
    // ==========================================
    /// <summary>새 씬 로드 후 플레이어가 준비되었음을 알립니다. (UI 연결, 적 타겟 설정 등)</summary>
    public event Action<PlayerController> OnPlayerReady;
    /// <summary>플레이어가 사망했음을 알립니다. (적의 패트롤 복귀 등)</summary>
    public event Action OnPlayerDeath;
    /// <summary>플레이어가 부활을 완료했음을 알립니다.</summary>
    public event Action OnPlayerRespawn;

    [Header("Player Setup")]
    public PlayerController player;

    // 현재 저장된 부활 위치 (체크포인트가 갱신해줌)
    private Vector2 currentRespawnPoint;

    private bool isRespawning = false;
    private bool waitingForUserChoice = false;

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
        // 1. 타이틀 씬은 플레이어 처리 스킵
        if (scene.name == Define.SceneNames.Title)
        {
            UnsubscribeFromPlayer();
            player = null;
            return;
        }

        // 2. 인게임 씬: 새 플레이어 찾고 사망 이벤트 구독
        UnsubscribeFromPlayer();
        player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            SubscribeToPlayer();
            StartCoroutine(InitializePlayerRoutine());
        }
    }

    /// <summary>
    /// 새 씬 진입 시 플레이어 데이터를 복원하고 외부에 알립니다.
    /// FadeIn 같은 시각 효과는 SceneTransitionManager가 책임지므로 여기서 처리하지 않습니다.
    /// </summary>
    private IEnumerator InitializePlayerRoutine()
    {
        // 플레이어와 포탈/체크포인트들의 Awake/Start 완료를 위해 1프레임 대기
        yield return null;

        // 1. 저장된 데이터가 있다면 위치/스탯 복구
        if (DataManager.Instance != null && DataManager.Instance.hasSavedData)
        {
            // 저장된 스탯(체력, 마나) 주입
            player.ImportSessionData(DataManager.Instance.sessionData);

            // 위치 복원 우선순위: 체크포인트 > 포탈
            string checkpointID = DataManager.Instance.sessionData.lastCheckpointID;
            string portalID = DataManager.Instance.sessionData.lastPortalID;

            if (!string.IsNullOrEmpty(checkpointID))
            {
                Checkpoint cp = Checkpoint.FindCheckpointByID(checkpointID);
                if (cp != null)
                {
                    player.transform.position = cp.SpawnPosition;
                }
            }
            else if (!string.IsNullOrEmpty(portalID))
            {
                Portal exitPortal = Portal.FindPortalByID(portalID);
                if (exitPortal != null)
                {
                    player.transform.position = exitPortal.spawnPoint.position;
                }
            }
        }

        // 2. 부활 지점을 현재 위치로 갱신
        currentRespawnPoint = player.transform.position;

        // 3. 모든 준비가 끝났음을 외부에 알림 (UI, 카메라, 적이 이 신호를 기다림)
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

    // ==========================================
    // 플레이어 사망 / 부활 처리
    // ==========================================
    private void HandlePlayerDeathInternal()
    {
        // 외부에 사망 사실을 알림 (적들이 패트롤 모드로 돌아감)
        OnPlayerDeath?.Invoke();

        // 부활 흐름 시작
        RespawnPlayer();
    }

    /// <summary>체크포인트가 자신의 위치를 부활 지점으로 등록할 때 호출.</summary>
    public void UpdateRespawnPoint(Vector2 newPoint)
    {
        currentRespawnPoint = newPoint;
    }

    public void RespawnPlayer()
    {
        if (isRespawning) return;
        StartCoroutine(RespawnCoroutine());
    }

    /// <summary>UIManager의 사망 메뉴 "Respawn" 버튼이 호출.</summary>
    public void ResumeRespawn()
    {
        waitingForUserChoice = false;
    }

    /// <summary>
    /// 부활 흐름:
    /// 1. 사망 메뉴 표시 후 유저 선택 대기
    /// 2. 부활 데이터를 sessionData에 저장 (체력/마나 풀 회복)
    /// 3. SceneTransitionManager에 씬 재로드 위임 (페이드/로드/페이드인 모두 처리)
    /// </summary>
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

        // 2. 부활 데이터 저장 (체력/마나 풀 회복 + 부활 위치 정보)
        SaveRespawnDataToSession();

        // 3. 씬 재로드를 SceneTransitionManager에 위임
        // SceneTransitionManager가 FadeOut → Load → FadeIn 까지 모두 처리
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.ReloadCurrentScene();
        }
        else
        {
            // 폴백: 씬 매니저 직접 호출
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        isRespawning = false;

        OnPlayerRespawn?.Invoke();
    }

    /// <summary>
    /// 부활 위치 정보를 sessionData에 저장합니다.
    /// 씬 재로드 후 InitializePlayerRoutine이 이 정보로 플레이어를 적절한 위치에 부활시킵니다.
    ///
    /// 위치 복원 우선순위 (InitializePlayerRoutine에서):
    /// 1. lastCheckpointID가 있으면 → 마지막 체크포인트 위치
    /// 2. 없으면 lastPortalID 사용 → 씬 진입 포탈 위치
    /// </summary>
    private void SaveRespawnDataToSession()
    {
        if (DataManager.Instance == null || DataManager.Instance.sessionData == null) return;
        if (player == null) return;

        DataManager.GameData package = ExportPlayerSession();

        // 부활 시 체력/마나 풀 회복 (사망 직후 0인 값을 명시적으로 풀로 덮어씀)
        package.currentHealth = package.maxHealth;
        package.currentMana = package.maxMana;

        // 현재 씬 명시 (다른 값으로 오염되지 않도록)
        package.lastSceneName = SceneManager.GetActiveScene().name;

        package.lastPortalID = DataManager.Instance.sessionData.lastPortalID;
        package.lastCheckpointID = DataManager.Instance.sessionData.lastCheckpointID;

        // SaveTransitionData가 lastCheckpointID, lastPortalID, isBossDefeated 등
        // 영구 진행 정보를 기존 sessionData에서 자동 병합해줍니다
        DataManager.Instance.SaveTransitionData(package);
    }

    /// <summary>
    /// 씬을 떠날 때, SceneTransitionManager가 플레이어 상태를 포장하기 위해 호출합니다.
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