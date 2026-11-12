using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour {
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;
    void Start() {
        interactionIcon.SetActive(false);
    }
    public void OnInteract(InputAction.CallbackContext context) {
        Debug.Log($"OnInteract phase={context.phase} inRange={interactableInRange}");
        if (context.performed || interactableInRange == null) return;

        if (!interactableInRange.CanInteract()) {
            interactionIcon.SetActive(false);
        } else {
            interactableInRange.Interact();
        }
    }
    public void Register(IInteractable interactable) {
        interactableInRange = interactable;
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange && !BattleManager.IsBattleActive) {
            if (!interactable.CanInteract()) return;

            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }
    private void OnTriggerStay2D(Collider2D collision) {
        if (BattleManager.IsBattleActive) return;

        if (interactableInRange == null &&
            collision.TryGetComponent(out IInteractable interactable) &&
            interactable.CanInteract()) {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }
    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange) {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}
