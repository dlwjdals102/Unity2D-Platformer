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

    private void Awake()
    {
        // 싱글톤 중복 생성 방지 로직
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 게임 시작 시, 플레이어의 처음 위치를 기본 부활 지점으로 설정합니다.
        if (player != null)
        {
            currentRespawnPoint = player.transform.position;
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
        // 코루틴을 사용해 시간 차(Delay)를 두고 부활시킵니다.
        StartCoroutine(RespawnCoroutine());
    }

    private IEnumerator RespawnCoroutine()
    {
        // 1. 지정된 시간만큼 대기 (이때 화면이 어두워지는 효과를 넣기 좋습니다)
        yield return new WaitForSeconds(respawnDelay);

        // 2. 플레이어 위치를 체크포인트로 이동
        player.transform.position = currentRespawnPoint;

        // 3. 플레이어 물리 속도 초기화 및 체력 회복
        player.Respawn();
    }
}