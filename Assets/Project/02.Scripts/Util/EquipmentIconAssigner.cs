#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class EquipmentIconAssigner : EditorWindow
{
    [MenuItem("TowerBreaker/Assign Equipment Icons")]
    public static void AssignIcons()
    {
        
        string[] guids = AssetDatabase.FindAssets("t:EquipmentData");
        List<EquipmentData> allEquips = new List<EquipmentData>();
        foreach (string guid in guids)
        {
            allEquips.Add(AssetDatabase.LoadAssetAtPath<EquipmentData>(AssetDatabase.GUIDToAssetPath(guid)));
        }

        
        Dictionary<string, string> iconMap = new Dictionary<string, string>
        {
            { "weapon_legendary_01", "Assets/Project/00.Assets/Cainos 1/Pixel Art Icon Pack - RPG/Texture/Weapon & Tool/Golden Sword.png" },
            { "armor_epic_01", "Assets/Project/00.Assets/Cainos 1/Pixel Art Icon Pack - RPG/Texture/Equipment/Iron Armor.png" },
            { "helmet_rare_01", "Assets/Project/00.Assets/SPUM/Resources/Addons/Ver300/0_Unit/0_Sprite/8_Weapons/7_Shield/New_Shield_01.png" },
            { "shoes_rare_01", "Assets/Project/00.Assets/Cainos 1/Pixel Art Icon Pack - RPG/Texture/Equipment/Leather Armor.png" },
            { "accessory_common_01", "Assets/Project/00.Assets/SPUM/Resources/Addons/Ver300/0_Unit/0_Sprite/8_Weapons/0_Sword/New_Weapon_11.png" }
        };

        int count = 0;
        foreach (var eq in allEquips)
        {
            if (iconMap.TryGetValue(eq.id, out string path))
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null)
                {
                    eq.icon = sprite;
                    EditorUtility.SetDirty(eq);
                    count++;
                }
            }
        }

        AssetDatabase.SaveAssets();
    }
}
#endif
