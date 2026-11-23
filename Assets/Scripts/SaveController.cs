using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class SaveController : MonoBehaviour {
    private string saveLocation;
    private InventoryController inventoryController;
    private Chest[] chests;
    private NPC[] npcs;
    void Start() {
        InitializeComponents();
        LoadGame();
    }
    private void InitializeComponents() {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindAnyObjectByType<InventoryController>();
        chests = FindObjectsByType<Chest>();
        npcs = FindObjectsByType<NPC>(FindObjectsInactive.Include);
    }

    public void SaveGame() {
        //Debug.Log("SaveGame called, NPC count: " + (npcs?.Length ?? -1));
        SaveData savedata = new SaveData {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            chestSaveData = GetChestState(),
            npcSaveData = GetNPCStates()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(savedata));
    }
    private List<ChestSaveData> GetChestState() {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();

        foreach (Chest chest in chests) {
            ChestSaveData chestSaveData = new ChestSaveData {
                chestID = chest.ChestID,
                isOpened = chest.IsOpened
            };
            chestStates.Add(chestSaveData);
        }

        return chestStates;
    }
    private List<NPCSaveData> GetNPCStates() {
        var states = new List<NPCSaveData>();
        foreach (NPC npc in npcs) {
            states.Add(new NPCSaveData {
                NPCID = npc.NPCID,
                isActive = npc.gameObject.activeSelf
            });
        }
        return states;
    }

    private void LoadNPCStates(List<NPCSaveData> states) {
        if (states == null) return;

        foreach (NPC npc in npcs) {
            NPCSaveData data = states.FirstOrDefault(s => s.NPCID == npc.NPCID);
            if (data != null) npc.gameObject.SetActive(data.isActive);
        }
    }
    public void LoadGame() {
        if (File.Exists(saveLocation)) {
            SaveData savedata = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject.FindGameObjectWithTag("Player").transform.position = savedata.playerPosition;

            FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D = GameObject.Find(savedata.mapBoundary).GetComponent<PolygonCollider2D>();

            MapController_man.Instance?.HighlightArea(savedata.mapBoundary);

            inventoryController.SetInventoryItems(savedata.inventorySaveData);

            LoadChestStates(savedata.chestSaveData);

            LoadNPCStates(savedata.npcSaveData);
            //Debug.Log("LoadGame called, NPC count: " + savedata.npcSaveData.Count);
        }
        else {
            SaveGame();

            inventoryController.SetInventoryItems(new List<InventorySaveData>());
        }
    }
    private void LoadChestStates(List<ChestSaveData> chestStates) {
        foreach (Chest chest in chests) {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);

            if (chestSaveData != null) {
                chest.SetOpened(chestSaveData.isOpened);
            }
        }
    }
}
