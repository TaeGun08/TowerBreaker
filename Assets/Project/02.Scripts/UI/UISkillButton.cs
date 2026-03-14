using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillButton : MonoBehaviour
{
    [SerializeField] private int skillSlotIndex; // 0 또는 1
    [SerializeField] private Image iconImage; // 아이콘 복구
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

        if (instance != null && iconImage != null)
        {
            iconImage.sprite = instance.Data.icon;
            iconImage.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (_assignedInstance == null) return;

        // 쿨다운 연출
        if (cooldownImage != null)
        {
            float remaining = Mathf.Max(0, (_assignedInstance.LastUsedTime + _assignedInstance.Data.cooldown) - Time.time);
            cooldownImage.fillAmount = remaining / _assignedInstance.Data.cooldown;
        }

        button.interactable = _assignedInstance.IsReady && !PlayerUnit.Instance.IsTransitioning;
    }

    private void OnClickButton()
    {
        if (_assignedInstance != null && _assignedInstance.IsReady)
        {
            PlayerUnit.Instance.UseSkill(skillSlotIndex);
        }
    }
}
