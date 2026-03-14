using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillButton : MonoBehaviour
{
    [SerializeField] private int skillSlotIndex; 
    [SerializeField] private Image iconImage; 
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Button button;

    private SkillInstance _assignedInstance;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClickButton);
    }

    public void SetSkill(SkillInstance instance)
    {
        _assignedInstance = instance;
        
        
        gameObject.SetActive(instance != null);

        if (instance != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = instance.Data.icon;
                iconImage.gameObject.SetActive(true);
            }
            
            if (button != null)
            {
                button.interactable = true; 
            }
        }
    }

    private void Update()
    {
        if (_assignedInstance == null || button == null) return;

        
        bool isReady = _assignedInstance.IsReady;
        if (cooldownImage != null)
        {
            float remaining = Mathf.Max(0, (_assignedInstance.LastUsedTime + _assignedInstance.Data.cooldown) - Time.time);
            cooldownImage.fillAmount = remaining / _assignedInstance.Data.cooldown;
            cooldownImage.gameObject.SetActive(!isReady); 
        }

        
        button.interactable = isReady && (PlayerUnit.Instance != null && !PlayerUnit.Instance.IsTransitioning);
    }

    private void OnClickButton()
    {
        if (_assignedInstance != null && _assignedInstance.IsReady)
        {
            PlayerUnit.Instance.UseSkill(skillSlotIndex);
        }
    }
}

