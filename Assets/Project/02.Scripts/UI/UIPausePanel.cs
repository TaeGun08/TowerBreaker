using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIPausePanel : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (quitButton != null) quitButton.onClick.AddListener(QuitToMenu);
    }

    private void OnEnable()
    {
        // 패널이 열리면 게임 정지
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Time.timeScale = 1.0f;
        gameObject.SetActive(false);
    }

    public void QuitToMenu()
    {
        // 일시정지 상태 해제 후 씬 전환
        Time.timeScale = 1.0f;
        
        Debug.Log("Exiting to Main Menu...");
        SceneManager.LoadScene("OutGame");
    }
}
