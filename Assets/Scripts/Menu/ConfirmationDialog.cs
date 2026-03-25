using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmationDialog : MonoBehaviour
{
    public TMP_Text messageText;
    public Button confirmButton;
    public Button cancelButton;
    public GameObject dialogPanel;

    private System.Action onConfirm;
    private System.Action onCancel;

    void Start()
    {
        dialogPanel.SetActive(false);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);
    }

    public void ShowDialog(string message, System.Action confirmAction, System.Action cancelAction = null)
    {
        messageText.text = message;
        onConfirm = confirmAction;
        onCancel = cancelAction;
        dialogPanel.SetActive(true);
    }

    public void HideDialog()
    {
        dialogPanel.SetActive(false);
        onConfirm = null;
        onCancel = null;
    }

    void OnConfirm()
    {
        onConfirm?.Invoke();
        HideDialog();
    }

    void OnCancel()
    {
        onCancel?.Invoke();
        HideDialog();
    }
}