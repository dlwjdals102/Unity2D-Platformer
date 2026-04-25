using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Portal : MonoBehaviour
{
    [Header("Portal ID")]
    public string portalID; // 이 포탈의 고유 번호 (ex: "Castle_Exit")
    public string targetPortalID; // 도착지 씬에서 내가 나타나야 할 포탈의 ID
    public string targetSceneName;

    [Header("Spawn Point")]
    public Transform spawnPoint;// 플레이어가 포탈을 타고 나타날 정확한 위치

    private bool isPlayerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // (선택) 여기서 "위쪽 화살표를 누르세요" 같은 UI 팝업을 띄우면 아주 좋습니다!
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // UI 팝업 숨기기
        }
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(targetSceneName)) return;

        // 플레이어가 포탈 범위 안에 있고, 상호작용 키(예: 위쪽 방향키)를 눌렀을 때
        if (isPlayerInRange && InputManager.Instance.Controls.Player.Interact.WasPressedThisFrame())
        {
            // 중복 클릭 방지를 위해 범위를 벗어난 것으로 처리
            isPlayerInRange = false;

            // 매니저에게 씬 이동을 요청!
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.TransitionToScene(targetSceneName, targetPortalID);
            }
            else
            {
                Debug.LogError("SceneTransitionManager가 없습니다!");
            }
        }
    }

    public static Portal FindPortalByID(string id)
    {
        // 현재 씬에 로드된 모든 Portal 오브젝트를 가져옵니다.
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
