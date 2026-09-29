using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor.Overlays;
using UnityEngine;

public class SaveController : MonoBehaviour {
    private string saveLocation;
    private InventoryController inventoryController;
    private Chest[] chests;
    void Start() {
        InitializeComponents();
        LoadGame();
    }
    private void InitializeComponents() {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindAnyObjectByType<InventoryController>();
        chests = FindObjectsByType<Chest>();
    }

    public void SaveGame() {
        SaveData savedata = new SaveData {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            chestSaveData = GetChestState()
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
    public void LoadGame() {
        if (File.Exists(saveLocation)) {
            SaveData savedata = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject.FindGameObjectWithTag("Player").transform.position = savedata.playerPosition;

            FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D = GameObject.Find(savedata.mapBoundary).GetComponent<PolygonCollider2D>();

            MapController_man.Instance?.HighlightArea(savedata.mapBoundary);

            inventoryController.SetInventoryItems(savedata.inventorySaveData);

            LoadChestStates(savedata.chestSaveData);
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
