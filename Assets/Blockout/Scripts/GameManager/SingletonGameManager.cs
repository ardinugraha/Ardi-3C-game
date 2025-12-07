using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public enum ResumeMode
{
    None,
    Checkpoint
}


public class SingletonGameManager : PersistentSingletonBehaviour<SingletonGameManager>
{
    public ResumeMode resumeMode = ResumeMode.None;
    private HashSet<string> shownInstructions = new();
    
    private Vector3 currentCheckpoint;
    [SerializeField]
    private bool isInMainMenu = true;

    
    public Vector3 GetCheckpoint() => currentCheckpoint;

    public void ResetGameState()
    {
        resumeMode = ResumeMode.None;
    }

    public void PlayerDied(PlayerMovement player)
    {
        Debug.Log("PlayerDied Event Received. Respawning...");
        player.Respawn(currentCheckpoint);
    }

    public void SaveCheckpoint(Vector3 checkpointPos)
    {
        resumeMode = ResumeMode.Checkpoint;
        currentCheckpoint = checkpointPos;
        Debug.Log("Checkpoint saved at position: " + currentCheckpoint);
    }

    public bool HasInstructionShown(string id)
    {
        return shownInstructions.Contains(id);
    }

    public void SaveInstructionShown(string id)
    {
        shownInstructions.Add(id);
    }
 
    public void BackToMainMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenuBlockout");
        isInMainMenu = true;
    }

    public void BackToGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("Blockout");
        isInMainMenu = false;
    }

    public void ResetGame()
    {
        resumeMode = ResumeMode.None;
        shownInstructions.Clear();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SceneManager.LoadScene("Blockout");
        isInMainMenu = false;
    }

}