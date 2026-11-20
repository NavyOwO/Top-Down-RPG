using NUnit.Framework;
using NUnit.Framework.Internal.Execution;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEditor;
using System.Collections;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    public static InventoryController Instance { get; private set; }

    void Awake() //changed from start to awake
    {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        itemDictionary = FindAnyObjectByType<ItemDictionary>();
    }

    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null) return false;

        //look for existing item
        foreach (Transform slotTransform in inventoryPanel.transform) {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null) {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem != null && slotItem.ID == itemToAdd.ID) {
                    slotItem.AddToStack();
                    return true;
                }
            }
        }

        // look for empty slot
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("Inventory full!");
        return false;
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData { 
                    itemID = item.ID, 
                    slotIndex = slotTransform.GetSiblingIndex(), 
                    quantity = item.quantity 
                });
            }
        }
        return invData;
    }
    public int CountItem(int itemID) {
        int total = 0;
        foreach (Transform slotTransform in inventoryPanel.transform) {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null) {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null && item.ID == itemID) total += item.quantity;
            }
        }
        return total;
    }

    public bool RemoveItem(int itemID, int amount = 1) {
        foreach (Transform slotTransform in inventoryPanel.transform) {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) continue;

            Item item = slot.currentItem.GetComponent<Item>();
            if (item == null || item.ID != itemID) continue;

            item.RemoveFromStack(amount);
            if (item.quantity <= 0) {
                Destroy(slot.currentItem);
                slot.currentItem = null;
            }
            return true;
        }
        return false;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject ItemPrefab = itemDictionary.GetItemPrefab(data.itemID);

                if (ItemPrefab != null)
                {
                    GameObject item = Instantiate(ItemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    Item itemComponent = item.GetComponent<Item>();
                    if (itemComponent != null & data.quantity > 1) {
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }
                    slot.currentItem = item;
                } //else {
                   // Debug.Log("ItemPrefab is null");
                //}
            }
        }
    }
}
