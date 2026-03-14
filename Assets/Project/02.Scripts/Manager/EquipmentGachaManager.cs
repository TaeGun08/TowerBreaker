using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentGachaManager : SingletonBase<EquipmentGachaManager>
{
    private const int GACHA_COST_CHEST = 1; 

    public EquipmentData DrawEquipment()
    {
        if (CurrencyManager.Instance == null || CurrencyManager.Instance.TotalChests < GACHA_COST_CHEST)
        {
            return null;
        }

        if (EquipmentManager.Instance == null || EquipmentManager.Instance.MasterDB == null || EquipmentManager.Instance.MasterDB.Count == 0)
        {
            return null;
        }

        
        CurrencyManager.Instance.AddChest(-GACHA_COST_CHEST);

        
        float rand = UnityEngine.Random.value;
        EquipmentTier targetTier;

        if (rand < 0.03f) targetTier = EquipmentTier.Legendary;
        else if (rand < 0.15f) targetTier = EquipmentTier.Epic;
        else if (rand < 0.40f) targetTier = EquipmentTier.Rare;
        else targetTier = EquipmentTier.Common;

        
        var possibleItems = EquipmentManager.Instance.MasterDB.Where(e => e.tier == targetTier).ToList();

        if (possibleItems.Count == 0)
        {
            return null;
        }

        
        int index = UnityEngine.Random.Range(0, possibleItems.Count);
        EquipmentData result = possibleItems[index];

        EquipmentManager.Instance.AddEquipment(result.id);

        return result;
    }
}
