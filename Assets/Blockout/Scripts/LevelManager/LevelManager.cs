using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : SingletonBehaviour<LevelManager>
{
    [SerializeField] Transform defaultSpawnPoint;
    [SerializeField] PlayerMovement playerPrefab;

    void Start()
    {
        SpawnPlayer(playerPrefab);
    }

    public void SpawnPlayer(PlayerMovement player)
    {
        Vector3 spawnPos;

        if (SingletonGameManager.Instance.resumeMode == ResumeMode.Checkpoint)
        {
            spawnPos = SingletonGameManager.Instance.GetCheckpoint();
        }
        else
        {
            spawnPos = defaultSpawnPoint.position;
        }
        player.Respawn(spawnPos);
    }

    public void PlayerDied(PlayerMovement player)
    {
        RespawnAtCheckpoint(player);
    }

    public void RespawnAtCheckpoint(PlayerMovement player)
    {
        Vector3 spawnPos = SingletonGameManager.Instance.GetCheckpoint();

        player.Respawn(spawnPos);
    }
}
