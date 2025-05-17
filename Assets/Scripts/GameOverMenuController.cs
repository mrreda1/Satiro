using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuController : MonoBehaviour {
    public void OnNewGameClick() {
        SceneManager.LoadScene("GameScene");
    }

    public void onMainMenuClick() {
        SceneManager.LoadScene("Start Menu");
    }

    public void OnExitClick() {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
        Application.Quit();
    }
}
