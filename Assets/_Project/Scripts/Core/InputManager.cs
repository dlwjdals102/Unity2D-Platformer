using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    // 방금 자동 생성된 C# 클래스
    public PlayerInputActions Controls { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // 입력 객체 생성
            Controls = new PlayerInputActions();

            // @Core 전체를 파괴 방지 (이전 설계 유지)
            //DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 시스템이 켜고 꺼질 때 입력도 같이 켜고 꺼줍니다.
    private void OnEnable()
    {
        Controls?.Enable();
    }

    private void OnDisable()
    {
        Controls?.Disable();
    }
}
