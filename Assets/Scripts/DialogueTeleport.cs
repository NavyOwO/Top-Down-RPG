using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class DialogueTeleport : MonoBehaviour {
    [SerializeField] NPC npc;
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] Transform playerTarget;
    [SerializeField] Transform npcTarget;

    [Header("Sprites")]
    [SerializeField] Sprite playerBattleSprite;
    [SerializeField] Sprite npcBattleSprite;

    [Header("After the battle (-1 = return | no dialogue)")]
    [SerializeField] int winDialogueIndex = -1;
    [SerializeField] int loseDialogueIndex = -1;

    Sprite npcOriginalSprite, playerOriginalSprite;
    Vector3 playerOriginalPos, npcOriginalPos;
    Collider2D originalBounds;
    bool hasOrigin, running;

    public async void Run() {
        if (running) return;
        running = true;

        npc.HoldDialogue();
        DialogueController.Instance.ShowDialogueUI(false);

        var player = GameObject.FindGameObjectWithTag("Player");
        CaptureOrigin(player);

        await MoveTo(player, playerTarget.position, npcTarget.position,
                     mapBoundary, playerBattleSprite, npcBattleSprite);

        DialogueController.Instance.ShowDialogueUI(true);
        npc.ResumeDialogue();
        RegisterWithDetector(player);

        running = false;
    }
    public void ReturnAfterWin() => Return(winDialogueIndex);
    public void ReturnAfterLoss() => Return(loseDialogueIndex);

    async void Return(int dialogueIndex) {
        if (running || !hasOrigin) return;
        running = true;
        PauseController.SetPause(true);

        var player = GameObject.FindGameObjectWithTag("Player");
        await MoveTo(player, playerOriginalPos, npcOriginalPos,
                     originalBounds, playerOriginalSprite, npcOriginalSprite);
        hasOrigin = false;

        if (dialogueIndex >= 0) {
            RegisterWithDetector(player);
            npc.StartDialogueAt(dialogueIndex);
        }
        else {
            PauseController.SetPause(false);
        }

        running = false;
    }

    // Shared helpers
    void CaptureOrigin(GameObject player) {
        if (hasOrigin) return;

        npcOriginalPos = npc.transform.position;
        var npcSr = npc.GetComponentInChildren<SpriteRenderer>();
        npcOriginalSprite = npcSr != null ? npcSr.sprite : null;

        if (player != null) {
            playerOriginalPos = player.transform.position;
            var pSr = player.GetComponentInChildren<SpriteRenderer>();
            playerOriginalSprite = pSr != null ? pSr.sprite : null;
        }

        var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
        originalBounds = confiner != null ? confiner.BoundingShape2D : null;

        hasOrigin = true;
    }

    async Task MoveTo(GameObject player, Vector3 playerPos, Vector3 npcPos,
                      Collider2D bounds, Sprite playerSprite, Sprite npcSprite) {
        try {
            await ScreenFader.Instance.FadeOut();

            var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
            if (confiner != null && bounds != null) {
                confiner.BoundingShape2D = bounds;
                confiner.InvalidateBoundingShapeCache();
            }

            if (player != null) {
                player.transform.position = playerPos;
                SetSprite(player.GetComponentInChildren<SpriteRenderer>(), playerSprite);
            }

            npc.transform.position = npcPos;
            SetSprite(npc.GetComponentInChildren<SpriteRenderer>(), npcSprite);

            Physics2D.SyncTransforms();

            if (bounds != null)
                MapController_man.Instance?.HighlightArea(bounds.name);

            await ScreenFader.Instance.FadeIn();
        }
        catch (System.Exception ex) {
            Debug.LogException(ex);
        }
    }

    void RegisterWithDetector(GameObject player) {
        var detector = player != null ? player.GetComponentInChildren<InteractionDetector>() : null;
        if (detector != null) detector.Register(npc);
    }

    private void SetSprite(SpriteRenderer sr, Sprite sprite) {
        if (sr == null || sprite == null) return;
        sr.sprite = sprite;
        sr.flipX = false;
    }
    public void ResetSprite() {
        SetSprite(npc.GetComponentInChildren<SpriteRenderer>(), npcOriginalSprite);
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            SetSprite(player.GetComponentInChildren<SpriteRenderer>(), playerOriginalSprite);
    }
}