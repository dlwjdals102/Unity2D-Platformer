using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 싱글톤(Singleton) 구현: 누구나 GameManager.Instance로 접근 가능합니다.
    public static GameManager Instance { get; private set; }

    [Header("Player Setup")]
    public PlayerController player;

    // 현재 저장된 부활 위치 (체크포인트)
    private Vector2 currentRespawnPoint;

    private bool isRespawning = false;
    private bool waitingForUserChoice = false; //  유저 선택 대기 상태

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
        }
    }

    // ==========================================
    // 씬 로드 이벤트 구독
    // ==========================================
    private void OnEnable()
    {
        // 매니저가 활성화될 때, 씬이 로드될 때마다 'OnSceneLoaded' 함수를 실행하라고 예약합니다.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 매니저가 꺼질 때 예약을 취소합니다 (메모리 누수 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. 타이틀 씬이라면 플레이어를 찾지 않고 무시합니다.
        // (디렉터님의 타이틀 씬 이름이 정확히 "Title"이라고 가정합니다. 다르다면 맞춰서 수정해주세요)
        if (scene.name == "Title")
        {
            player = null;
            Debug.Log("[GameManager] 타이틀 씬입니다. 플레이어를 찾지 않습니다.");
            return;
        }

        // 2. 인게임 씬이라면 새로운 씬에 배치된 플레이어를 찾습니다.
        player = FindFirstObjectByType<PlayerController>();

        if (player != null)
        {
            // 플레이어를 찾았다면, 저장된 데이터를 적용하고 위치를 이동시킵니다!
            // 그냥 호출하면 순서 문제가 생길 수 있으니 코루틴으로 안전하게 실행합니다.
            StartCoroutine(RestoreSessionRoutine());
        }
    }

    private IEnumerator Start()
    {
        // 1. 플레이어가 자신의 Start()에서 SO를 읽어 자가 초기화를 할 때까지 1프레임 대기합니다.
        yield return null;

        // 게임 시작 시, 플레이어의 처음 위치를 기본 부활 지점으로 설정합니다.
        if (player != null)
        {
            currentRespawnPoint = player.transform.position;

            // 플레이어 사망 이벤트 구독
            // 플레이어의 HealthComponent가 OnDeath를 알리면 RespawnPlayer를 즉시 실행합니다.
            if (player.Health != null)
            {
                player.Health.OnDeath += RespawnPlayer;
            }
        }

        //StartCoroutine(RestoreSessionRoutine());
    }

    // 메모리 안전을 위한 이벤트 해제
    private void OnDestroy()
    {
        if (player != null && player.Health != null)
        {
            player.Health.OnDeath -= RespawnPlayer;
        }
    }

    public IEnumerator RestoreSessionRoutine()
    {
        // 플레이어와 포탈들이 각자의 Awake/Start를 마칠 때까지 1프레임 기다려줍니다.
        yield return null;

        if (DataManager.Instance != null && DataManager.Instance.hasSavedData)
        {
            Debug.Log("[GameManager] 세션 복구를 시작합니다.");

            // 1. 저장된 스탯(체력, 마나) 주입
            player.ImportSessionData(DataManager.Instance.sessionData);

            // 2. 저장된 포탈 ID로 위치 이동
            string targetID = DataManager.Instance.sessionData.lastPortalID;
            Portal exitPortal = Portal.FindPortalByID(targetID);

            if (exitPortal != null)
            {
                // 플레이어 위치를 포탈의 스폰 지점으로 순간이동
                player.transform.position = exitPortal.spawnPoint.position;
                currentRespawnPoint = player.transform.position; // 체크포인트 갱신
                Debug.Log($"[GameManager] 포탈 '{targetID}'로 텔레포트 완료.");
            }
        }
    }


    // 체크포인트가 GameManager에게 "여기 새로운 부활 지점이야!"라고 알려주는 함수
    public void UpdateRespawnPoint(Vector2 newPoint)
    {
        currentRespawnPoint = newPoint;
        Debug.Log("체크포인트 갱신 완료!");
    }

    // 플레이어가 죽었을 때 호출될 함수
    public void RespawnPlayer()
    {
        if (isRespawning) return;

        // 코루틴을 사용해 시간 차(Delay)를 두고 부활시킵니다.
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
        Debug.Log("플레이어 사망! 부활 프로세스 시작...");

        if (UIManager.Instance != null)
        {
            // 지정된 시간 대기가 아니라, UI 메뉴를 띄우고 입력을 기다립니다.
            UIManager.Instance.ShowDeathMenu(true);
            waitingForUserChoice = true;
        }

        // 유저가 Continue 버튼을 눌러 ResumeRespawn()이 호출될 때까지 무한 대기
        while (waitingForUserChoice)
        {
            yield return null;
        }

        // 화면 암전
        yield return StartCoroutine(UIManager.Instance.FadeOut(1f));

        if (player != null)
        {
            // 3. 어둠 속에서 몰래 위치 이동 및 체력 복구
            player.transform.position = currentRespawnPoint;
            player.Respawn();

            Debug.Log("플레이어 재배치 완료.");
        }

        if (UIManager.Instance != null)
        {
            // 4. 텍스트 지우고 화면 다시 밝히기
            yield return StartCoroutine(UIManager.Instance.FadeIn(1f));
        }

        Debug.Log("부활 시퀀스 종료.");
        isRespawning = false;
    }

    /// <summary>
    /// 씬을 떠날 때, 씬 전환 매니저의 요청을 받아 플레이어의 현재 상태를 포장해주는 함수입니다.
    /// </summary>
    public DataManager.GameData ExportPlayerSession()
    {
        if (player != null)
        {
            // 플레이어 본인에게 데이터를 포장하라고 시킵니다.
            return player.ExportSessionData();
        }

        // 플레이어가 없다면(예외 상황) 빈 상자를 보냅니다.
        return new DataManager.GameData();
    }
}