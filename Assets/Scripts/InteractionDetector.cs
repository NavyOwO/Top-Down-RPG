using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour {
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;

    void Start() {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context) {
        if (!context.performed) return;
        if (BattleManager.IsBattleActive || interactableInRange == null) return;

        if (!interactableInRange.CanInteract()) {
            interactionIcon.SetActive(false);
            return;
        }

        interactableInRange.Interact();
        interactionIcon.SetActive(false);
    }

    public void Register(IInteractable interactable) {
        interactableInRange = interactable;
        interactionIcon.SetActive(false);
    }

    void Refresh(Collider2D collision) {
        if (BattleManager.IsBattleActive || PauseController.IsGamePaused) return;
        if (!collision.TryGetComponent(out IInteractable interactable)) return;
        if (interactableInRange != null && interactableInRange != interactable) return;
        if (!interactable.CanInteract()) return;

        interactableInRange = interactable;
        interactionIcon.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision) => Refresh(collision);
    private void OnTriggerStay2D(Collider2D collision) => Refresh(collision);

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange) {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}