using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 모든 Button 클릭에 자동으로 사운드를 연결합니다.
/// EventSystem이 있는 어떤 오브젝트에도 붙여 사용할 수 있고,
/// 또는 각 Canvas의 루트에 붙여도 됩니다.
///
/// [작동 방식]
/// Awake에서 자식의 모든 Button을 찾아 onClick 이벤트에 사운드 재생을 자동 등록합니다.
/// Unity 6의 새 Input System / Old Input System 모두와 호환됩니다.
/// (이 방식은 입력 감지에 의존하지 않고 Button 자체의 onClick 이벤트를 사용)
///
/// [사용법]
/// 1. AudioManager에 "UI_Click" 사운드를 등록합니다.
/// 2. 각 Canvas (메인 메뉴 Canvas, 일시정지 Canvas 등)의 루트에 이 컴포넌트를 추가합니다.
/// 3. 끝.
///
/// [장점]
/// - Input System 종류와 무관 (Button.onClick 이벤트 활용)
/// - 동적으로 추가되는 버튼도 자동 처리 (RegisterAllButtons() 재호출 시)
/// - 각 버튼에 일일이 OnClick을 추가할 필요 없음
/// </summary>
public class UISoundPlayer : MonoBehaviour
{
    [Header("Sound Names (AudioManager에 등록된 이름)")]
    [Tooltip("버튼 클릭 시 재생할 사운드 이름")]
    [SerializeField] private string clickSoundName = "UI_Click";

    [Tooltip("버튼 호버 시 재생할 사운드 이름 (비워두면 호버 사운드 없음)")]
    [SerializeField] private string hoverSoundName = "";

    [Header("Settings")]
    [Tooltip("자식 오브젝트의 Button까지 모두 찾을지 여부")]
    [SerializeField] private bool includeInactiveButtons = false;

    private void Awake()
    {
        RegisterAllButtons();
    }

    /// <summary>
    /// 자식 오브젝트의 모든 Button을 찾아 클릭 사운드를 자동 등록합니다.
    /// 동적으로 버튼이 추가/제거되었다면 외부에서 다시 호출하여 갱신할 수 있습니다.
    /// </summary>
    public void RegisterAllButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(includeInactiveButtons);

        foreach (Button btn in buttons)
        {
            // 중복 등록 방지: 기존에 등록된 동일 핸들러를 먼저 제거
            btn.onClick.RemoveListener(PlayClickSound);
            btn.onClick.AddListener(PlayClickSound);

            // 호버 사운드 등록 (선택)
            if (!string.IsNullOrEmpty(hoverSoundName))
            {
                AddHoverHandler(btn);
            }
        }
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(clickSoundName))
        {
            AudioManager.Instance.Play(clickSoundName);
        }
    }

    private void PlayHoverSound()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(hoverSoundName))
        {
            AudioManager.Instance.Play(hoverSoundName);
        }
    }

    /// <summary>
    /// 호버 사운드를 위해 EventTrigger 컴포넌트를 동적으로 추가합니다.
    /// </summary>
    private void AddHoverHandler(Button btn)
    {
        EventTrigger trigger = btn.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = btn.gameObject.AddComponent<EventTrigger>();
        }

        // PointerEnter 이벤트 등록
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entry.callback.AddListener((data) => PlayHoverSound());
        trigger.triggers.Add(entry);
    }
}