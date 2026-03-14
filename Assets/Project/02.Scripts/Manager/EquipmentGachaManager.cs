using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentGachaManager : SingletonBase<EquipmentGachaManager>
{
    private const int GACHA_COST_CHEST = 1; // 뽑기 1회당 체스트 1개

    public EquipmentData DrawEquipment()
    {
        if (CurrencyManager.Instance == null || CurrencyManager.Instance.TotalChests < GACHA_COST_CHEST)
        {
            Debug.LogWarning("Not enough Chests!");
            return null;
        }

        if (EquipmentManager.Instance == null || EquipmentManager.Instance.MasterDB == null)
        {
            Debug.LogError("Equipment Database is missing!");
            return null;
        }

        // 1. 체스트 소모
        CurrencyManager.Instance.AddChest(-GACHA_COST_CHEST);

        // 2. 확률에 따른 티어 결정
        float rand = UnityEngine.Random.value;
        EquipmentTier targetTier;

        if (rand < 0.03f) targetTier = EquipmentTier.Legendary;
        else if (rand < 0.15f) targetTier = EquipmentTier.Epic;
        else if (rand < 0.40f) targetTier = EquipmentTier.Rare;
        else targetTier = EquipmentTier.Common;

        // 3. 해당 티어의 장비 목록 추출 (EquipmentManager의 MasterDB 참조)
        var possibleItems = EquipmentManager.Instance.MasterDB.Where(e => e.tier == targetTier).ToList();

        if (possibleItems.Count == 0)
        {
            Debug.LogError($"No equipment found for tier: {targetTier}");
            return null;
        }

        // 4. 랜덤 선택 및 획득 처리
        int index = UnityEngine.Random.Range(0, possibleItems.Count);
        EquipmentData result = possibleItems[index];

        EquipmentManager.Instance.AddEquipment(result.id);

        return result;
    }
}
