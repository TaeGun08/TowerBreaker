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
    [Range(0, 1)] public float critChance = 0.1f; 
    public float critDamageMultiplier = 2.0f; 

    [Range(0, 1)] public float doubleHitChance = 0.05f; 
    [Range(0, 1)] public float secondHitDamageRatio = 0.5f; 

    
    public PlayerStats Clone() => (PlayerStats)this.MemberwiseClone();

    public string LevelUpRandomStat()
    {
        int rand = UnityEngine.Random.Range(0, 5);
        string result = "";

        switch (rand)
        {
            case 0:
                int hpUp = UnityEngine.Random.Range(8, 13);
                int oldHp = maxHp;
                maxHp += hpUp;
                result = $"HP +{hpUp} ({oldHp} -> {maxHp})";
                break;
            case 1:
                int dmgUp = UnityEngine.Random.Range(2, 4);
                int oldDmg = baseDamage;
                baseDamage += dmgUp;
                result = $"ATK +{dmgUp} ({oldDmg} -> {baseDamage})";
                break;
            case 2:
                int defUp = 1;
                int oldDef = defense;
                defense += defUp;
                result = $"DEF +{defUp} ({oldDef} -> {defense})";
                break;
            case 3:
                float oldCrit = critChance;
                critChance = Mathf.Min(0.5f, critChance + 0.01f);
                result = $"CRIT +1% ({Mathf.RoundToInt(oldCrit * 100)}% -> {Mathf.RoundToInt(critChance * 100)}%)";
                break;
            case 4:
                float oldDouble = doubleHitChance;
                doubleHitChance = Mathf.Min(0.4f, doubleHitChance + 0.01f);
                result = $"DBL +1% ({Mathf.RoundToInt(oldDouble * 100)}% -> {Mathf.RoundToInt(doubleHitChance * 100)}%)";
                break;
        }
        return result;
    }
}

