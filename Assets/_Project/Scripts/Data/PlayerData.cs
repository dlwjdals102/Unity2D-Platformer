using UnityEngine;

[CreateAssetMenu(fileName = "newPlayerData", menuName = "Data/Player Data")]
public class PlayerData : EntityData
{
    [Header("Player Movement")]
    public float jumpForce = 12f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Player Combat")]
    public float attackDamage = 15f;
    public float iFrameDuration = 0.5f; // 무적 시간
}