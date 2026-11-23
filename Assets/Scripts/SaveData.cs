using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData {
    public Vector3 playerPosition;
    public string mapBoundary; //name of boundary
    public List<InventorySaveData> inventorySaveData;
    public List<ChestSaveData> chestSaveData;
    public List<NPCSaveData> npcSaveData;
}

[System.Serializable]
public class ChestSaveData {
    public string chestID;
    public bool isOpened;
}
[System.Serializable]
public class NPCSaveData {
    public string NPCID;
    public bool isActive;
}