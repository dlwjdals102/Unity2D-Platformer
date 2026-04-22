using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // 싱글톤(Singleton) 구현: 누구나 GameManager.Instance로 접근 가능합니다.
    public static GameManager Instance { get; private set; }

    [Header("Player Setup")]
    public PlayerController player;
    public float respawnDelay = 2f; // 죽고 나서 부활할 때까지의 대기 시간

    // 현재 저장된 부활 위치 (체크포인트)
    private Vector2 currentRespawnPoint;

    private bool isRespawning = false;

    private void Awake()
    {
        // 싱글톤 중복 생성 방지 로직
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 인스펙터에 무엇이 들어있든 무시하고, 씬(Scene)에 살아 숨쉬는 단 하나의 플레이어를 무조건 찾아냅니다.
        player = FindFirstObjectByType<PlayerController>();

        if (player == null) Debug.LogError("[GameManager] 씬에 PlayerController가 존재하지 않습니다! 플레이어를 맵에 배치해주세요.");
    }

    private void Start()
    {
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

        RestoreSession();
    }

    // 메모리 안전을 위한 이벤트 해제
    private void OnDestroy()
    {
        if (player != null && player.Health != null)
        {
            player.Health.OnDeath -= RespawnPlayer;
        }
    }

    private void RestoreSession()
    {
        // 저장된 데이터가 아예 없거나 창고가 없으면 복구 절차를 진행하지 않습니다.
        if (DataManager.Instance == null || !DataManager.Instance.hasSavedData) return;

        DataManager.GameData data = DataManager.Instance.sessionData;
        if (data == null) return;

        player.Health.LoadSavedHealth(data.currentHealth);

        // 플레이어 위치 복구 (포탈 ID 매칭)
        if (!string.IsNullOrEmpty(data.lastPortalID))
        {
            Portal[] portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
            foreach (Portal p in portals)
            {
                if (p.portalID == data.lastPortalID)
                {
                    player.transform.position = p.spawnPoint.position;
                    currentRespawnPoint = p.spawnPoint.position;
                    break;
                }
            }
        }

        // 데이터 복구가 끝났으니, 다음 씬 오작동을 막기 위해 스스로 스위치를 끕니다.
        DataManager.Instance.hasSavedData = false;

        Debug.Log($"[GameManager] '{data.lastPortalID}' 지점으로 데이터 복구 및 배치를 완료했습니다.");
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

    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;
        Debug.Log("플레이어 사망! 부활 프로세스 시작...");

        if (UIManager.Instance != null)
        {
            // 1. 화면 암전 및 "YOU DIED" 출력
            yield return StartCoroutine(UIManager.Instance.FadeOut(1f));
            yield return StartCoroutine(UIManager.Instance.ShowDeathText(1f));
        }

        // 2. 유저가 죽음을 체감할 수 있도록 지정된 시간만큼 대기
        yield return new WaitForSeconds(respawnDelay);

        if (player != null)
        {
            // 3. 어둠 속에서 몰래 위치 이동 및 체력 복구
            player.transform.position = currentRespawnPoint;
            player.Respawn();
            //player.gameObject.SetActive(true);

            Debug.Log("플레이어 재배치 완료.");
        }

        if (UIManager.Instance != null)
        {
            // 4. 텍스트 지우고 화면 다시 밝히기
            yield return StartCoroutine(UIManager.Instance.HideDeathText(0.5f));
            yield return StartCoroutine(UIManager.Instance.FadeIn(1f));
        }

        Debug.Log("부활 시퀀스 종료.");
        isRespawning = false;
    }
}