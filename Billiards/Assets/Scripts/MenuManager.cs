using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject rulesPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene"); 
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed"); 
    }
}
