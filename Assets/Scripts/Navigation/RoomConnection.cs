using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;


public class RoomConnection : MonoBehaviour
{
    [Header("This door's identity")]
    [Tooltip("Unique ID for this door inside this map. Must match targetDoorID on the door that leads here.")]
    public int doorID;

    [Header("Where this door leads")]
    public int targetMapID;
    public int targetDoorID;
    public int targetMapLibraryID; 

    public TMP_Text promptText;
    public string promptMessage = "Press [E] to enter";

    private bool _playerInRange = false;
    private bool _traveling     = false;


    private void OnEnable()
    {
        if (MapNavigation.Instance != null)
            MapNavigation.Instance.RegisterDoor(this);
        else
            StartCoroutine(RegisterNextFrame());
    }

    private void OnDisable()
    {
        if (MapNavigation.Instance != null)
            MapNavigation.Instance.UnregisterDoor(this);
    }

    private System.Collections.IEnumerator RegisterNextFrame()
    {
        yield return null;
        if (MapNavigation.Instance != null)
            MapNavigation.Instance.RegisterDoor(this);
        else
            Debug.LogWarning($"RoomConnection (doorID {doorID}): MapNavigation not found.");
    }


    private void Update()
    {
        if (!_playerInRange || _traveling) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            _traveling = true;
            SetPrompt(false);
            MapNavigation.Instance.GoToMap(targetMapLibraryID, targetDoorID);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        SetPrompt(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        _traveling     = false;
        SetPrompt(false);
    }


    private void SetPrompt(bool visible)
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(visible);
        if (visible) promptText.text = promptMessage;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(1f, 2f, 0.1f));
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.3f,
            $"Door {doorID} → Map {targetMapLibraryID} : Door {targetDoorID}");
#endif
    }
}
