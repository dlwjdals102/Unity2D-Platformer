using UnityEngine;

/// <summary>
/// 씬 간 이동을 담당하는 포탈입니다.
/// 
/// [변경 사항]
/// IInteractable 인터페이스를 구현하여, 상호작용 감지/입력 처리 로직을
/// InteractionDetector에 위임합니다. 포탈은 "씬 이동"이라는 고유 책임에만 집중합니다.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Portal : MonoBehaviour, IInteractable
{
    [Header("Portal ID")]
    public string portalID;         // 이 포탈의 고유 번호 (ex: "Castle_Exit")
    public string targetPortalID;   // 도착지 씬에서 내가 나타나야 할 포탈의 ID
    public string targetSceneName;

    [Header("Spawn Point")]
    public Transform spawnPoint;    // 플레이어가 포탈을 타고 나타날 정확한 위치

    // ==========================================
    // IInteractable 구현
    // ==========================================
    public void Interact()
    {
        if (string.IsNullOrEmpty(targetSceneName)) return;

        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.TransitionToScene(targetSceneName, targetPortalID);
        }
    }

    // ==========================================
    // 정적 헬퍼 (씬 로드 시 도착 포탈 검색용)
    // ==========================================
    public static Portal FindPortalByID(string id)
    {
        Portal[] allPortals = Object.FindObjectsByType<Portal>(FindObjectsSortMode.None);

        foreach (Portal portal in allPortals)
        {
            if (portal.portalID == id)
            {
                return portal;
            }
        }

        Debug.LogWarning($"[Portal] ID가 '{id}'인 포탈을 찾을 수 없습니다.");
        return null;
    }
}
