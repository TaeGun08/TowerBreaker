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
        
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        Time.timeScale = 1.0f;
        gameObject.SetActive(false);
    }

    public void QuitToMenu()
    {
        
        Time.timeScale = 1.0f;
        
        SceneManager.LoadScene("OutGame");
    }
}
