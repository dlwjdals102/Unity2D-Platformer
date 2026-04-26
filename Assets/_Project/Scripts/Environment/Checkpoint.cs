using UnityEngine;


/// <summary>
/// 플레이어가 가까이 와서 Interact 키(↑)를 누르면:
/// 1. 부활 지점을 이 체크포인트로 갱신
/// 2. 체력/마나 풀 회복
/// 3. 현재 진행 상황을 JSON 파일에 저장 (영구 보존)
///
/// Portal과 동일한 ID 기반 시스템을 사용해, 게임 재시작 시 마지막 체크포인트로 복원됩니다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint ID")]
    [Tooltip("이 체크포인트의 고유 식별자 (씬 안에서 유일해야 합니다). 예: \"Castle_Stage1_Save1\"")]
    public string checkpointID;

    [Tooltip("플레이어가 부활할 정확한 위치 (없으면 본체 transform 사용)")]
    [SerializeField] private Transform spawnPoint;

    [Header("Behavior")]
    [Tooltip("체크포인트 활성화 시 체력/마나를 풀로 회복할지 여부")]
    [SerializeField] private bool healOnActivate = true;

    private bool isPlayerInRange = false;

    /// <summary>실제로 플레이어가 부활할 위치 (spawnPoint가 없으면 본체)</summary>
    public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

    private void Start()
    {
        if (string.IsNullOrEmpty(checkpointID))
        {
            Debug.LogWarning($"[Checkpoint] {name}에 checkpointID가 비어있습니다. 식별자를 설정해주세요.");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Define.GameTags.Player))
        {
            isPlayerInRange = true;
            // TODO: "↑ 키로 저장" 같은 UI 팝업 표시
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Define.GameTags.Player))
        {
            isPlayerInRange = false;
            // TODO: UI 팝업 숨기기
        }
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        // InputManager null 가드 (씬 로드 순서에 따라 매니저가 늦게 준비될 수 있음)
        if (InputManager.Instance == null || InputManager.Instance.Controls == null) return;

        // 플레이어가 범위 안에 있고, Interact 키(↑)를 눌렀을 때 활성화
        if (InputManager.Instance.Controls.Player.Interact.WasPressedThisFrame())
        {
            ActivateCheckpoint();
        }
    }

    /// <summary>
    /// 체크포인트를 활성화하여 부활 지점 갱신, 회복, 저장을 수행합니다.
    /// </summary>
    private void ActivateCheckpoint()
    {
        // 1. 부활 지점 갱신 (사망 시 돌아올 위치)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateRespawnPoint(SpawnPosition);
        }

        // 2. 체력/마나 풀 회복 (다크소울 스타일)
        if (healOnActivate && GameManager.Instance?.player != null)
        {
            PlayerController player = GameManager.Instance.player;
            player.Health?.RestoreFullHealth();
            player.Mana?.RestoreFullMana();
        }

        // 3. 현재 게임 상태를 JSON 파일에 저장 (영구 보존)
        SaveProgress();

        // 4. TODO: 시각/청각 피드백 (깃발 펄럭임, 빛 이펙트, "저장 완료" UI 등)
        Debug.Log($"[Checkpoint] '{checkpointID}'에서 진행 상황이 저장되었습니다.");
    }

    /// <summary>
    /// 현재 플레이어 상태를 패키징하여 DataManager에 저장합니다.
    /// </summary>
    private void SaveProgress()
    {
        if (DataManager.Instance == null || GameManager.Instance == null) return;

        // 1. 플레이어 현재 스탯 export
        DataManager.GameData package = GameManager.Instance.ExportPlayerSession();

        // 2. 위치 복원 정보 설정 (체크포인트 기반)
        package.lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        package.lastCheckpointID = checkpointID;
        package.lastPortalID = "";  // 체크포인트 저장은 포탈 정보 무효화

        // 3. DataManager가 영구 진행 정보 병합 + 디스크 저장 자동 처리
        DataManager.Instance.SaveTransitionData(package);
    }

    // 에디터에서 spawnPoint 시각화
    private void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.3f);
            Gizmos.DrawLine(transform.position, spawnPoint.position);
        }
    }

    /// <summary>
    /// 씬 안의 모든 Checkpoint 중에서 ID가 일치하는 것을 찾아 반환합니다.
    /// (Portal.FindPortalByID와 동일한 패턴)
    /// </summary>
    public static Checkpoint FindCheckpointByID(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        Checkpoint[] all = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
        foreach (Checkpoint cp in all)
        {
            if (cp.checkpointID == id) return cp;
        }
        return null;
    }
}
