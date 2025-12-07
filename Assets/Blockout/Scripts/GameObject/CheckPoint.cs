using UnityEngine;

public class CheckPoint : MonoBehaviour
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
            SingletonGameManager.Instance.SaveCheckpoint(transform.position);
            Debug.Log("Checkpoint reached at position: " + transform.position);
        }
    }
}
