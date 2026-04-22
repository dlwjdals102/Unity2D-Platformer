using UnityEngine;

public class PlayerManaBarUI : ManaBarUI
{
    private void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null && player.Mana != null) SetTarget(player.Mana);
    }
}