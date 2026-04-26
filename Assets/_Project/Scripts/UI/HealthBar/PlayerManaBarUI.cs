using UnityEngine;

public class PlayerManaBarUI : ManaBarUI
{
    /* private void Start()
     {
         PlayerController player = FindFirstObjectByType<PlayerController>();
         if (player != null && player.Mana != null) SetTarget(player.Mana);
     }*/

    protected override void OnEnable()
    {
        base.OnEnable();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerReady += HandlePlayerReady;

            // 이미 준비된 플레이어가 있다면 즉시 연결
            if (GameManager.Instance.player != null && GameManager.Instance.player.Mana != null)
            {
                SetTarget(GameManager.Instance.player.Mana);
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
        if (player != null && player.Mana != null) SetTarget(player.Mana);
    }
}