using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private string firstGameSceneName = "FirstRoom"; // Scene name to load on Start

    private void Awake()
    {
        AssignButtonListeners();
    }

    private void AssignButtonListeners()
    {
        playButton.onClick.AddListener(HandlePlayButton);
        quitButton.onClick.AddListener(HandleQuitButton);
    }

    private void HandlePlayButton()
    {
        SceneManager.LoadScene(firstGameSceneName);
    }

    private void HandleQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // For testing in editor
#else
        Application.Quit(); // For build
#endif
    }
}
