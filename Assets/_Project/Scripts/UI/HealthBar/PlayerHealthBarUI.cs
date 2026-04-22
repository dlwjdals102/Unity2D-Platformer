using UnityEngine;

public class PlayerHealthBarUI : HealthBarUI
{
    private void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null) SetTarget(player);
    }
}
