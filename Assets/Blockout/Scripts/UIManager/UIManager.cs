using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMPro.TextMeshProUGUI instructionLabel;

    protected override void OnInit()
    {
        // Dipanggil sebelum Start(), aman untuk setup awal
        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(string message)
    {
        instructionLabel.text = message;
        panel.SetActive(true);

        // auto hide
        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), 3f);
    }

    public void Hide()
    {
        panel?.SetActive(false);
    }

    public void DebugLol()
    {
        Debug.Log("LOL");
    }
}