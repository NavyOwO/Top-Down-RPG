using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler {
    Transform originalParent;
    CanvasGroup canvasGroup;

    [SerializeField] public float minDropDistance = 2f;
    [SerializeField] public float maxDropDistance = 3f;
    [SerializeField] public float menuDelay = 0.5f;
    [SerializeField] public float menuHiddenTime = 1f;

    private InventoryController inventoryController;
    void Start() {
        canvasGroup = GetComponent<CanvasGroup>();
        inventoryController = InventoryController.Instance;
    }

    public void OnBeginDrag(PointerEventData eventData) {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData) {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if (dropSlot == null) {
            GameObject item = eventData.pointerEnter;
            if (item != null) {
                dropSlot = item.GetComponentInParent<Slot>();
            }
        }

        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null) {
            if (dropSlot.currentItem != null) {
                Item draggedItem = GetComponent<Item>();
                Item targetItem = dropSlot.currentItem.GetComponent<Item>();

                if (draggedItem.ID == targetItem.ID) {
                    targetItem.AddToStack(draggedItem.quantity);
                    originalSlot.currentItem = null;
                    Destroy(gameObject);
                }
                else {
                    // swap items if not working
                }

                // cache the item being swapped back before modifying any slot state
                GameObject swappedItem = dropSlot.currentItem;
                swappedItem.transform.SetParent(originalSlot.transform);
                swappedItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                originalSlot.currentItem = swappedItem;
            }
            else {
                // clear the original slot when dropping into an empty slot
                originalSlot.currentItem = null;
            }

            transform.SetParent(dropSlot.transform);
            dropSlot.currentItem = gameObject;
        }
        else {
            if (!IsWithinInventory(eventData.position)) {
                DropItem(originalSlot);
            }
            else {
                transform.SetParent(originalParent);
            }
        }

        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    bool IsWithinInventory(Vector2 mousePosition) {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }
    void DropItem(Slot originalSlot) {
        Item item = GetComponent<Item>();
        int quantity = item.quantity;

        if (quantity > 1) {
            item.RemoveFromStack();

            transform.SetParent(originalParent);
            GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            quantity = 1;
        } else {
            originalSlot.currentItem = null;
        }  

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null) {
            Debug.Log("Missing 'Player' tag ");
            return;
        }

        HideForDuration(menuDelay, menuHiddenTime); //fix this

        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        GameObject dropItem = Instantiate(gameObject, dropPosition, Quaternion.identity);
        Item droppedItem = dropItem.GetComponent<Item>();
        droppedItem.quantity = 1;
        dropItem.GetComponent<BounceEffect>().StartBounce();

        if (quantity <= 1 && originalSlot.currentItem == null) {
            Destroy(gameObject);
        }
    }

    private IEnumerator HideForDuration(float delayBefore, float hideDuration) {
        GameObject ui = GameObject.Find("UI");
        CanvasGroup canvasGroup = ui.GetComponent<CanvasGroup>();

        yield return new WaitForSeconds(delayBefore);
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(hideDuration);
        canvasGroup.alpha = 0.2f;
    }
    private void HideItemForDuration(float delayBefore, float hideDuration) {
        StartCoroutine(HideForDuration(delayBefore, hideDuration));
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (eventData.button == PointerEventData.InputButton.Right) {
            SplitStack();
        }
    }
    private void SplitStack() {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1) return;

        int splitAmount = item.quantity / 2;
        if (splitAmount <= 0) return;

        item.RemoveFromStack(splitAmount);

        GameObject newItem = item.CloneItem(splitAmount);

        if (inventoryController == null || newItem == null) return;

        foreach (Transform slotTransform in inventoryController.inventoryPanel.transform) {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null) {
                slot.currentItem = newItem;
                newItem.transform.SetParent(slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                return;
            }
        }

        item.AddToStack(splitAmount);
        Destroy(newItem);
    }
}
