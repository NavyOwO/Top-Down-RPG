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
    Sprite npcOriginalSprite, playerOriginalSprite;

    bool running;

    public async void Run() {
        if (running) return; // guard against double-trigger
        running = true;

        npc.HoldDialogue();
        DialogueController.Instance.ShowDialogueUI(false);

        var npcRenderer = npc.GetComponentInChildren<SpriteRenderer>();
        var player = GameObject.FindGameObjectWithTag("Player");
        var playerRenderer = player != null ? player.GetComponentInChildren<SpriteRenderer>() : null;

        if (npcOriginalSprite == null && npcRenderer != null) 
            npcOriginalSprite = npcRenderer.sprite;
        if (playerOriginalSprite == null && playerRenderer != null) 
            playerOriginalSprite = playerRenderer.sprite;

        try {
            await ScreenFader.Instance.FadeOut();

            var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
            if (confiner != null && mapBoundary != null) {
                confiner.BoundingShape2D = mapBoundary;
                confiner.InvalidateBoundingShapeCache();
            }

            if (player != null) {
                player.transform.position = playerTarget.position;
                SetSprite(playerRenderer, playerBattleSprite);
            }

            npc.transform.position = npcTarget.position;
            SetSprite(npcRenderer, npcBattleSprite);

            Physics2D.SyncTransforms();

            if (mapBoundary != null)
                MapController_man.Instance?.HighlightArea(mapBoundary.name);

            await ScreenFader.Instance.FadeIn();
        }
        catch (System.Exception ex) {
            Debug.LogException(ex);
        }

        DialogueController.Instance.ShowDialogueUI(true);
        npc.ResumeDialogue();

        var detector = player != null ? player.GetComponentInChildren<InteractionDetector>() : null;
        if (detector != null)
            detector.Register(npc);

        running = false;
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