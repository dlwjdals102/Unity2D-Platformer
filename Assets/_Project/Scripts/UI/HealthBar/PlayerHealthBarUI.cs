using UnityEngine;

public class PlayerHealthBarUI : HealthBarUI
{
    /*private void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null) SetTarget(player);
    }*/

    protected override void OnEnable()
    {
        base.OnEnable();

        // GameManager가 플레이어 준비 완료 신호를 보내면 자동 연결
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReady += HandlePlayerReady;

            // 이미 준비된 플레이어가 있다면 즉시 연결 (씬 도중 활성화된 경우 대비)
            if (GameManager.Instance.player != null)
            {
                SetTarget(GameManager.Instance.player);
            }
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReady -= HandlePlayerReady;
        }
    }

    private void HandlePlayerReady(PlayerController player)
    {
        if (player != null) SetTarget(player);
    }
}
