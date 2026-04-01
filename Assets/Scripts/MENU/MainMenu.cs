using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _exitButton;

    private void Awake()
    {
        // Инициализация кнопок
        if (_playButton == null)
            _playButton = GameObject.Find("PLAY").GetComponent<Button>();

        if (_exitButton == null)
            _exitButton = GameObject.Find("EXIT").GetComponent<Button>();

        _playButton.onClick.AddListener(PlayGame);
        _exitButton.onClick.AddListener(QuitGame);
    }

    private void PlayGame()
    {
        // Принудительно сбрасываем возможное состояние паузы
        Time.timeScale = 1f;
        Debug.Log("Play!");
        SceneManager.LoadScene("Map");
    }

    private void QuitGame()
    {
        Debug.Log("Quit!");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}