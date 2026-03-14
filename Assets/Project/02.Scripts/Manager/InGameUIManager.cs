using System.Collections;
using TMPro;
using UnityEngine;

public class InGameUIManager : SingletonBase<InGameUIManager>
{
    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Currency Info")]
    [SerializeField] private TextMeshProUGUI corpseText; 
    [SerializeField] private TextMeshProUGUI chestText; 

    [Header("Stage Info")]
    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Stat Info")]
    [SerializeField] private TextMeshProUGUI statsText; 
    [SerializeField] private TextMeshProUGUI upgradeNoticeText; 

    [Header("Skill Gacha")]
    [SerializeField] private GameObject skillGachaPanel;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;

    [Header("Skill Buttons")]
    [SerializeField] private UISkillButton[] skillButtons; // 인스펙터에서 2개 할당

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
        // 1. 플레이어 정보 초기화 (인스턴스 존재 여부 철저히 체크)
        var player = PlayerUnit.Instance;
        if (player != null && player.Stats != null)
        {
            UpdateHP(player.CurrentHP, player.Stats.maxHp);
            UpdateStatsDisplay();
            UpdateSkillButtons();
        }
        else
        {
            // 플레이어가 없는 상황 (아웃게임 등)에서는 기본값 표시 혹은 무시
            if (hpText != null) hpText.text = "";
        }

        // 2. 스테이지 정보 초기화
        if (StageManager.Instance != null)
        {
            UpdateStageText(StageManager.Instance.StageCount);
        }

        // 3. 재화 정보 즉시 동기화 (가장 중요)
        var cm = CurrencyManager.Instance;
        if (cm != null)
        {
            UpdateCorpseUI(cm.SessionCorpse);
            UpdateChestUI(cm.SessionChests);
            Debug.Log($"<color=cyan>UI Initialized: Corpse={cm.SessionCorpse}, Chest={cm.SessionChests}</color>");
        }
        else
        {
            UpdateCorpseUI(0);
            UpdateChestUI(0);
        }

        if (upgradeNoticeText != null) upgradeNoticeText.gameObject.SetActive(false);
        if (skillGachaPanel != null) skillGachaPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (PlayerUnit.Instance != null)
        {
            PlayerUnit.Instance.OnHealthChanged += HandleHealthChanged;
            PlayerUnit.Instance.OnSkillsUpdated += UpdateSkillButtons; // 스킬 갱신 이벤트 구독
        }

        if (StageManager.Instance != null)
            StageManager.Instance.OnStageProgress += HandleStageProgress;

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnSessionCorpseChanged += UpdateCorpseUI;
            CurrencyManager.Instance.OnSessionChestChanged += UpdateChestUI;
        }
    }

    private void OnDisable()
    {
        // 1. 플레이어 유닛 이벤트 해제
        if (!SingletonBase<PlayerUnit>.IsQuitting)
        {
            var player = PlayerUnit.Instance;
            if (player != null)
            {
                player.OnHealthChanged -= HandleHealthChanged;
                player.OnSkillsUpdated -= UpdateSkillButtons;
            }
        }

        // 2. 스테이지 매니저 이벤트 해제
        if (!SingletonBase<StageManager>.IsQuitting)
        {
            var sm = StageManager.Instance;
            if (sm != null) sm.OnStageProgress -= HandleStageProgress;
        }

        // 3. 재화 매니저 이벤트 해제
        if (!SingletonBase<CurrencyManager>.IsQuitting)
        {
            var cm = CurrencyManager.Instance;
            if (cm != null)
            {
                cm.OnSessionCorpseChanged -= UpdateCorpseUI;
                cm.OnSessionChestChanged -= UpdateChestUI;
            }
        }
    }

    public void UpdateSkillButtons()
    {
        if (skillButtons == null || PlayerUnit.Instance == null) return;

        var currentSkills = PlayerUnit.Instance.CurrentSkills;
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (skillButtons[i] == null) continue;
            
            if (i < currentSkills.Count)
            {
                skillButtons[i].SetSkill(currentSkills[i]);
            }
            else
            {
                skillButtons[i].SetSkill(null);
            }
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

    public void UpdateCorpseUI(int corpse)
    {
        if (corpseText != null) corpseText.text = corpse.ToString();
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

    public void ShowSkillGachaPanel()
    {
        if (skillGachaPanel != null)
        {
            skillGachaPanel.SetActive(true);
        }
    }

    public void ShowPausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
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
