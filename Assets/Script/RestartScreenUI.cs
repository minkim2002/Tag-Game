using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartScreenUI : MonoBehaviour
{
    public void RestartGame()
    {
        SceneManager.LoadSceneAsync(0);
    }

    public void QuitGame()
    {
        // Quit the game (this will only work in a build, not in the editor)
        Application.Quit();
    }
}
