using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BossRoomManager : MonoBehaviour
{
    [Header("Room Settings")]
    [Tooltip("닫힐 입구 문 오브젝트")]
    public GameObject entranceDoor;
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1단계: 플레이어가 방에 입장하면 문을 걸어 잠급니다.
        if (!isDoorClosed && collision.CompareTag("Player"))
        {
            isDoorClosed = true;
            playerTransform = collision.transform;

            if (entranceDoor != null)
                entranceDoor.SetActive(true); // 문 닫힘

            Debug.Log("입구 봉쇄! 긴장감 넘치는 BGM으로 변경할 타이밍입니다.");
            // TODO: BGM 정지, 컷씬 카메라 전환 등 연출 추가 가능
        }
    }

    private void Update()
    {
        // 2단계: 문이 닫혔고 아직 전투가 시작되지 않았다면, 플레이어와 보스의 거리를 잽니다.
        if (isDoorClosed && !hasBattleStarted && playerTransform != null)
        {
            float distanceToBoss = Vector2.Distance(playerTransform.position, boss.transform.position);

            if (distanceToBoss <= battleStartDistance)
            {
                hasBattleStarted = true;

                // UI 활성화 및 보스 데이터 연결
                if (bossHealthBar != null)
                {
                    bossHealthBar.gameObject.SetActive(true);
                    bossHealthBar.SetBossTarget(boss, boss.Data.bossName);
                }

                Debug.Log("전투 시작! 보스가 깨어납니다!");
                boss.WakeUp(); // 보스 강제 기상!

                // TODO: 보스전 전용 BGM 재생 시작
            }
        }

        // 전투 중 로직: 보스가 죽었는지 매 프레임 감시합니다.
        if (hasBattleStarted && !isRoomCleared)
        {
            // 보스의 체력이 0이하가 되어 죽었다면?
            if (boss.Health.IsDead)
            {
                ClearRoom();
            }
        }
    }

    // 방 클리어 처리 함수
    private void ClearRoom()
    {
        isRoomCleared = true;
        Debug.Log("보스전 클리어! 문이 열립니다.");

        // 1. 보스 체력바 UI 숨기기
        if (bossHealthBar != null) bossHealthBar.gameObject.SetActive(false);

        // 2. 닫혔던 입구 문 다시 열기 (혹은 다음 스테이지로 가는 출구 문 열기)
        if (entranceDoor != null) entranceDoor.SetActive(false);

        // TODO: 승리 BGM 재생, 보상 상자 소환 등 추가 연출
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