using UnityEngine;

public enum EquipmentType
{
    Weapon,
    Armor,
    Helmet,
    Shoes,
    Accessory
}

public enum EquipmentTier
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "NewEquipment", menuName = "EquipmentData")]
public class EquipmentData : ScriptableObject
{
    [Header("Basic Info")]
    public string id;           // 고유 식별자
    public string equipmentName;
    public EquipmentType type;
    public EquipmentTier tier;
    public Sprite icon;

    [Header("Stat Bonuses")]
    public int atkBonus;
    public int defBonus;
    public int hpBonus;
    public float critBonus;      // 0.05 = 5%
    public float doubleHitBonus; // 0.01 = 1%

    public Color GetTierColor()
    {
        switch (tier)
        {
            case EquipmentTier.Rare: return Color.cyan;
            case EquipmentTier.Epic: return Color.magenta;
            case EquipmentTier.Legendary: return new Color(1f, 0.5f, 0f); // Orange
            default: return Color.white;
        }
    }
}
