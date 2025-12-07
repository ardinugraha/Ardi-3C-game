using UnityEngine;

public class DeadZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.Instance.PlayerDied(other.GetComponent<PlayerMovement>());
            Debug.Log("Player entered Dead Zone. Triggering death event.");
        }
    }
}   