using UnityEngine;

[CreateAssetMenu(fileName = "newPlayerData", menuName = "Data/Player Data")]
public class PlayerData : EntityData
{
    [Header("Player Movement")]
    public float jumpForce = 12f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Player Mana System")]
    [Tooltip("최대 마나량")]
    public float maxMana = 100f;
    [Tooltip("대시 1회당 소모되는 마나량")]
    public float dashManaCost = 30f;
    [Tooltip("1초당 자동으로 회복되는 마나량")]
    public float manaRegenRate = 15f;

    [Header("Player Combat")]
    public float attackDamage = 15f;
    public float iFrameDuration = 0.5f; // 무적 시간
}