using UnityEngine;

public class InstructionZone : MonoBehaviour
{
    [TextArea]
    public string instructionText;

    public string instructionID;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (SingletonGameManager.Instance.HasInstructionShown(instructionID)) return;
        Debug.Log("Displaying instruction: " + instructionText);
        UIManager.Instance.Show(instructionText);

        SingletonGameManager.Instance.SaveInstructionShown(instructionID);
    }
}