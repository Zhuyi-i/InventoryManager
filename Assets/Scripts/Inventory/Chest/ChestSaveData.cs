using System;
using System.Collections.Generic;

[Serializable]
public class SavedChestSlot
{
    public string itemID;
    public int quantity;
}

[Serializable]
public class SavedChestState
{
    public string chestUID;          
    public bool   isOpened;
    public List<SavedChestSlot> remainingItems = new List<SavedChestSlot>();
}
