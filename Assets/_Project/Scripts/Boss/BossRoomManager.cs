using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BossRoomManager : MonoBehaviour
{
    [Header("Room Settings")]
    [Tooltip("닫힐 입구 문 오브젝트")]
    public GameObject[] entranceDoors;
    [Tooltip("전투를 시작할 보스 객체")]
    public Boss boss;

    [Tooltip("하이어라키에 배치된 BossHealthBarUI 오브젝트를 연결하세요.")]
    public BossHealthBarUI bossHealthBar;

    [Header("Battle Trigger Settings")]
    [Tooltip("문이 닫힌 후, 보스와 이 거리만큼 가까워지면 보스가 깨어납니다.")]
    public float battleStartDistance = 8f;

    private bool isDoorClosed = false;
    private bool hasBattleStarted = false;
    private Transform playerTransform;
    private bool isRoomCleared = false; // 클리어 여부 확인용 변수 추가

    public bool isGizmoDraw = true;

    private void Start()
    {
        if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);

        if (entranceDoors != null)
        {
            foreach (var door in entranceDoors)
            {
                door.SetActive(false);
            }
        }

        // 이미 보스가 처치된 방이라면 즉시 개방 상태로
        // (DataManager와 sessionData 모두 null 가드)
        if (DataManager.Instance != null
            && DataManager.Instance.sessionData != null
            && DataManager.Instance.sessionData.isBossDefeated)
        {
            isRoomCleared = true;
            isDoorClosed = true;

            if (entranceDoors != null)
            {
                foreach (var door in entranceDoors)
                {
                    door.SetActive(false);
                }
            }
        }
    }

    // ==========================================
    // 보스 사망 이벤트 구독 (매 프레임 폴링 대신 이벤트 기반)
    // ==========================================
    private void OnEnable()
    {
        if (boss != null && boss.Health != null)
        {
            boss.Health.OnDeath += HandleBossDeath;
        }
    }

    private void OnDisable()
    {
        if (boss != null && boss.Health != null)
        {
            boss.Health.OnDeath -= HandleBossDeath;
        }
    }

    private void HandleBossDeath()
    {
        if (isRoomCleared) return;
        ClearRoom();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1단계: 플레이어가 방에 입장하면 문을 걸어 잠급니다.
        if (!isDoorClosed && collision.CompareTag(Define.GameTags.Player))
        {
            isDoorClosed = true;
            playerTransform = collision.transform;

            // 문 닫힘
            if (entranceDoors != null)
            {
                foreach (var door in entranceDoors)
                {
                    door.SetActive(true);
                }
            }

            // TODO: BGM 정지, 컷씬 카메라 전환 등 연출 추가 가능
        }
    }

    private void Update()
    {
        // 2단계: 문이 닫혔고 아직 전투가 시작되지 않았다면, 플레이어와 보스의 거리 측정
        if (!isDoorClosed || hasBattleStarted) return;
        if (playerTransform == null || boss == null) return;

        float distanceToBoss = Vector2.Distance(playerTransform.position, boss.transform.position);

        if (distanceToBoss <= battleStartDistance)
        {
            StartBattle();
        }
    }

    private void StartBattle()
    {
        hasBattleStarted = true;

        // UI 활성화 및 보스 데이터 연결 (null 가드 강화)
        if (bossHealthBar != null && boss.Data != null)
        {
            bossHealthBar.gameObject.SetActive(true);
            bossHealthBar.SetBossTarget(boss, boss.Data.bossName);
        }

        boss.WakeUp();
        // TODO: 보스전 전용 BGM 재생 시작
        FeedbackManager.Instance.TriggerBossRoarFeedback(1f, 7f, 1f);
        AudioManager.Instance.PlayBGM("BGM_Battle");
    }

    // 방 클리어 처리 함수
    private void ClearRoom()
    {
        isRoomCleared = true;

        // 1. 보스 체력바 UI 숨기기
        if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);

        // 2. 닫혔던 입구 문 다시 열기 (혹은 다음 스테이지로 가는 출구 문 열기)
        if (entranceDoors != null)
        {
            foreach (var door in entranceDoors)
            {
                door.SetActive(false);
            }
        }

        // TODO: 승리 BGM 재생, 보상 상자 소환 등 추가 연출
        FeedbackManager.Instance.ResetCameraZoom();
        AudioManager.Instance.PlayBGM("BGM_Stage");
    }

    private void OnDrawGizmos()
    {
        if (!isGizmoDraw) return;

        // 에디터에서 보스가 깨어나는 거리를 시각적으로 볼 수 있게 해줍니다!
        if (boss != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(boss.transform.position, battleStartDistance);
        }
    }
}