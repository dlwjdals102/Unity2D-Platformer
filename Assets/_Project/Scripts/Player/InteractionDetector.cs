using UnityEngine;

/// <summary>
/// 플레이어에 부착하여, 범위 내 IInteractable 오브젝트를 감지하고
/// Interact 키 입력 시 상호작용을 실행하는 컴포넌트입니다.
///
/// 이 컴포넌트 덕분에 상호작용 감지/입력 처리 책임이 플레이어 한 곳에 집중되고,
/// Portal, Checkpoint 등 개별 오브젝트는 자신의 고유 로직(Interact)에만 집중할 수 있습니다.
///
/// [사용법]
/// 1. 플레이어 오브젝트에 이 스크립트를 추가합니다.
/// 2. 상호작용 가능한 오브젝트에는 IInteractable 인터페이스 + Collider2D(IsTrigger)만 있으면 됩니다.
/// </summary>
public class InteractionDetector : MonoBehaviour
{
    private IInteractable currentTarget;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트가 IInteractable을 구현하고 있다면 타겟으로 등록
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            currentTarget = interactable;
            // TODO: "↑ 키로 상호작용" UI 팝업 표시
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 범위를 벗어난 오브젝트가 현재 타겟과 동일하면 해제
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == currentTarget)
        {
            currentTarget = null;
            // TODO: UI 팝업 숨기기
        }
    }

    private void Update()
    {
        if (currentTarget == null) return;
        if (InputManager.Instance == null || InputManager.Instance.Controls == null) return;

        if (InputManager.Instance.Controls.Player.Interact.WasPressedThisFrame())
        {
            currentTarget.Interact();
        }
    }
}
