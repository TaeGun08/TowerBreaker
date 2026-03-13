using System;
using UnityEngine;

[Serializable]
public class PlayerStats
{
    [Header("Base Stats")]
    public int maxHp = 100;
    public int defense = 5;
    public int baseDamage = 10;

    [Header("Probability Stats")]
    [Range(0, 1)] public float critChance = 0.1f; // 치명타 확률 (10%)
    public float critDamageMultiplier = 2.0f; // 치명타 공격력 배율 (200%)

    [Range(0, 1)] public float doubleHitChance = 0.05f; // 두 번 타격 확률 (5%)
    [Range(0, 1)] public float secondHitDamageRatio = 0.5f; // 두 번째 공격력 비율 (50%)

    // 현재 스탯 복사본 생성 (장비 합산 등을 위해)
    public PlayerStats Clone() => (PlayerStats)this.MemberwiseClone();
}
