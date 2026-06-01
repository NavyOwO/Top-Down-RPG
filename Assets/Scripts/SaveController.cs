using System.IO;
using Unity.Cinemachine;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindAnyObjectByType<InventoryController>();

        LoadGame();
    }

    public void SaveGame()
    {
        SaveData savedata = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(savedata));
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData savedata = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject.FindGameObjectWithTag("Player").transform.position = savedata.playerPosition;

            FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D = GameObject.Find(savedata.mapBoundary).GetComponent<PolygonCollider2D>();

            inventoryController.SetInventoryItems(savedata.inventorySaveData);
        }
        else
        {
            SaveGame();
        }
    }
}
