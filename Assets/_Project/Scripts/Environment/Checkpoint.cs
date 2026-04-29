using UnityEngine;

/// <summary>
/// 플레이어가 상호작용하면 부활 지점 갱신, 체력/마나 회복, 진행 저장을 수행합니다.
///
/// [변경 사항]
/// IInteractable 인터페이스를 구현하여, 상호작용 감지/입력 처리 로직을
/// InteractionDetector에 위임합니다. 체크포인트는 "저장"이라는 고유 책임에만 집중합니다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour, IInteractable
{
    [Header("Checkpoint ID")]
    [Tooltip("이 체크포인트의 고유 식별자 (씬 안에서 유일해야 합니다). 예: \"Castle_Stage1_Save1\"")]
    public string checkpointID;

    [Tooltip("플레이어가 부활할 정확한 위치 (없으면 본체 transform 사용)")]
    [SerializeField] private Transform spawnPoint;

    [Header("Behavior")]
    [Tooltip("체크포인트 활성화 시 체력/마나를 풀로 회복할지 여부")]
    [SerializeField] private bool healOnActivate = true;

    /// <summary>실제로 플레이어가 부활할 위치 (spawnPoint가 없으면 본체)</summary>
    public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;

    private void Start()
    {
        if (string.IsNullOrEmpty(checkpointID))
        {
            Debug.LogWarning($"[Checkpoint] {name}에 checkpointID가 비어있습니다. 식별자를 설정해주세요.");
        }
    }

    // ==========================================
    // IInteractable 구현
    // ==========================================
    public void Interact()
    {
        // 1. 부활 지점 갱신 (사망 시 돌아올 위치)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateRespawnPoint(SpawnPosition);
        }

        // 2. 체력/마나 풀 회복
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

    // ==========================================
    // 내부 로직
    // ==========================================
    private void SaveProgress()
    {
        if (DataManager.Instance == null || GameManager.Instance == null) return;

        DataManager.GameData package = GameManager.Instance.ExportPlayerSession();

        package.lastSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        package.lastCheckpointID = checkpointID;
        package.lastPortalID = "";  // 체크포인트 저장은 포탈 정보 무효화

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
