using System.Collections.Generic;
using UnityEngine;

public class ChestManager : MonoBehaviour
{
    public static ChestManager Instance;

    private Dictionary<string, SavedChestState> _states
        = new Dictionary<string, SavedChestState>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    public void LoadFromSave(SaveDataFile save)
    {
        _states.Clear();

        if (save?.chestStates == null) return;

        foreach (var state in save.chestStates)
            _states[state.chestUID] = state;

        Debug.Log($"ChestManager: loaded {_states.Count} chest states from save.");
    }

    public void Clear()
    {
        _states.Clear();
        Debug.Log("ChestManager: cleared all chest states.");
    }

    public void WriteToSave(SaveDataFile save)
    {
        save.chestStates.Clear();
        foreach (var kvp in _states)
            save.chestStates.Add(kvp.Value);
    }

    public SavedChestState GetState(string uid)
    {
        _states.TryGetValue(uid, out var state);
        return state;
    }

    public void SetState(SavedChestState state)
    {
        _states[state.chestUID] = state;
    }

    public void MarkLooted(string uid)
    {
        var state = new SavedChestState
        {
            chestUID = uid,
            isOpened = true,
            remainingItems = new List<SavedChestSlot>()
        };
        _states[uid] = state;
    }
}