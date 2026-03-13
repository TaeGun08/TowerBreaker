using System.Collections;
using TMPro;
using UnityEngine;

public class InGameUIManager : SingletonBase<InGameUIManager>
{
    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Currency Info")]
    [SerializeField] private TextMeshProUGUI goldText; 
    [SerializeField] private TextMeshProUGUI chestText; 

    [Header("Stage Info")]
    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Stat Info")]
    [SerializeField] private TextMeshProUGUI statsText; 
    [SerializeField] private TextMeshProUGUI upgradeNoticeText; 

    [Header("Damage Font Settings")]
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0, 1.2f, 0); 
    
    private RectTransform _rectTransform;
    private Canvas _parentCanvas;

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        if (_parentCanvas == null) _parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (PlayerUnit.Instance != null)
        {
            UpdateHP(PlayerUnit.Instance.CurrentHP, PlayerUnit.Instance.Stats.maxHp);
            UpdateStatsDisplay();
        }

        if (StageManager.Instance != null)
        {
            UpdateStageText(StageManager.Instance.StageCount);
        }

        // 인게임 UI는 항상 0부터 시작 (세션 데이터 기반)
        if (CurrencyManager.Instance != null)
        {
            UpdateGoldUI(CurrencyManager.Instance.SessionGold);
            UpdateChestUI(CurrencyManager.Instance.SessionChests);
        }
        else
        {
            UpdateGoldUI(0);
            UpdateChestUI(0);
        }

        if (upgradeNoticeText != null) upgradeNoticeText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (PlayerUnit.Instance != null)
            PlayerUnit.Instance.OnHealthChanged += HandleHealthChanged;

        if (StageManager.Instance != null)
            StageManager.Instance.OnStageProgress += HandleStageProgress;

        // 세션 전용 이벤트 구독
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnSessionGoldChanged += UpdateGoldUI;
            CurrencyManager.Instance.OnSessionChestChanged += UpdateChestUI;
        }
    }

    private void OnDisable()
    {
        if (PlayerUnit.Instance != null && !SingletonBase<PlayerUnit>.IsQuitting)
            PlayerUnit.Instance.OnHealthChanged -= HandleHealthChanged;

        if (StageManager.Instance != null && !SingletonBase<StageManager>.IsQuitting)
            StageManager.Instance.OnStageProgress -= HandleStageProgress;

        if (CurrencyManager.Instance != null && !SingletonBase<CurrencyManager>.IsQuitting)
        {
            CurrencyManager.Instance.OnSessionGoldChanged -= UpdateGoldUI;
            CurrencyManager.Instance.OnSessionChestChanged -= UpdateChestUI;
        }
    }

    private void HandleHealthChanged(int cur, int max)
    {
        UpdateHP(cur, max);
        UpdateStatsDisplay();
    }

    public void UpdateStatsDisplay()
    {
        if (statsText == null || PlayerUnit.Instance == null) return;
        var s = PlayerUnit.Instance.Stats;
        statsText.text = $"<b>ATK</b>  {s.baseDamage}\n" +
                         $"<b>DEF</b>  {s.defense}\n" +
                         $"<b>CRIT</b> {Mathf.RoundToInt(s.critChance * 100)}%\n" +
                         $"<b>DBL</b>  {Mathf.RoundToInt(s.doubleHitChance * 100)}%";
    }

    public void UpdateGoldUI(int gold)
    {
        if (goldText != null) goldText.text = gold.ToString();
    }

    public void UpdateChestUI(int chests)
    {
        if (chestText != null) chestText.text = chests.ToString();
    }

    public void ShowUpgradeNotice(string message)
    {
        if (upgradeNoticeText == null) return;
        
        upgradeNoticeText.gameObject.SetActive(false);
        upgradeNoticeText.text = $"<color=green>UPGRADE!</color>\n<size=120%>{message}</size>";
        upgradeNoticeText.gameObject.SetActive(true);
        
        StopCoroutine("HideNoticeAfterDelay");
        StartCoroutine("HideNoticeAfterDelay");
        
        UpdateStatsDisplay();
    }

    private IEnumerator HideNoticeAfterDelay()
    {
        yield return new WaitForSeconds(3.0f);
        if (upgradeNoticeText != null) upgradeNoticeText.gameObject.SetActive(false);
    }

    private void UpdateHP(int currentHp, int maxHp)
    {
        if (hpText != null)
        {
            hpText.text = $"HP {Mathf.Max(0, currentHp)} / {maxHp}";
        }
    }

    private void HandleStageProgress()
    {
        if (StageManager.Instance != null)
        {
            UpdateStageText(StageManager.Instance.StageCount);
        }
    }

    private void UpdateStageText(int stage)
    {
        if (stageText != null)
        {
            stageText.text = $"FLOOR {stage + 1}";
        }
    }

    public void SpawnDamageText(Vector3 worldPos, int damage, bool isCrit)
    {
        if (damageTextPrefab == null) return;

        EnsureReferences();
        if (_rectTransform == null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 targetWorldPos = worldPos + damageTextOffset;
        Vector2 screenPos = mainCam.WorldToScreenPoint(targetWorldPos);

        Camera uiCam = null;
        if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCam = mainCam;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPos, uiCam, out Vector2 localPos))
        {
            DamageText instance = Instantiate(damageTextPrefab, transform);
            RectTransform rect = instance.transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = localPos;
            }
            instance.Setup(damage, isCrit);
        }
    }
}
