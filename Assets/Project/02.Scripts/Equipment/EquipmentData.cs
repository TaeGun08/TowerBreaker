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
    public string id;           
    public string equipmentName;
    public EquipmentType type;
    public EquipmentTier tier;
    public Sprite icon;

    [Header("Stat Bonuses")]
    public int atkBonus;
    public int defBonus;
    public int hpBonus;
    public float critBonus;      
    public float doubleHitBonus; 

    public Color GetTierColor()
    {
        switch (tier)
        {
            case EquipmentTier.Rare: return Color.cyan;
            case EquipmentTier.Epic: return Color.magenta;
            case EquipmentTier.Legendary: return new Color(1f, 0.5f, 0f); 
            default: return Color.white;
        }
    }
}

