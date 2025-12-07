using UnityEngine;
using UnityEngine.SceneManagement;
 
public class BlockoutMainMenuManager : MonoBehaviour
{
    public void Resume()
    {
        SingletonGameManager.Instance.BackToGameplay();
    }
    public void Play()
    {
        SingletonGameManager.Instance.ResetGame();
    }
    public void Exit()
    {
        Application.Quit();
    }
}