using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject ammoPanel;

    [Header("First Selected Buttons")]
    public GameObject firstPauseButton;
    public GameObject firstSettingsButton;

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenu";
    public float inputResumeDelay = 0.2f;

    public static bool GameIsPaused { get; private set; }
    private static float previousTimeScale = 1f;
    private bool isResuming = false;

    private void Awake()
    {
        GameIsPaused = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isResuming)
        {
            if (settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        if (GameIsPaused)
        {
            StartCoroutine(ResumeGameCoroutine());
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        previousTimeScale = Time.timeScale;

        pauseMenuPanel.SetActive(true);
        if (ammoPanel) ammoPanel.SetActive(false);

        Time.timeScale = 0f;
        GameIsPaused = true;
        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(firstPauseButton);

        // Сбрасываем ввод старой системы
        ResetLegacyInput();
    }

    private IEnumerator ResumeGameCoroutine()
    {
        isResuming = true;

        pauseMenuPanel.SetActive(false);
        if (ammoPanel) ammoPanel.SetActive(true);

        // Ждем один кадр перед восстановлением
        yield return null;

        Time.timeScale = previousTimeScale;
        GameIsPaused = false;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Дополнительная задержка для предотвращения случайных выстрелов
        yield return new WaitForSecondsRealtime(inputResumeDelay);
        isResuming = false;
    }

    private void ResetLegacyInput()
    {
        // Сбрасываем все состояния ввода старой системы
        Input.ResetInputAxes();
    }

    public void OpenSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstSettingsButton);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(firstPauseButton);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        AudioListener.pause = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}