using System;
using Unity.Cinemachine;
using UnityEngine;

public class DialogueTeleport : MonoBehaviour
{
    [SerializeField] NPC npc;
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] Transform playerTarget;
    [SerializeField] Transform npcTarget;

    [Header("Sprites")]
    [SerializeField] Sprite playerBattleSprite;
    [SerializeField] Sprite npcBattleSprite;
    Sprite npcOriginalSprite;

    public async void Run() {
        npc.HoldDialogue();

        npcOriginalSprite = npc.GetComponentInChildren<SpriteRenderer>().sprite;
        DialogueController.Instance.ShowDialogueUI(false);

        await ScreenFader.Instance.FadeOut();

        var confiner = FindAnyObjectByType<CinemachineConfiner2D>();
        if (confiner != null && mapBoundary != null)
            confiner.BoundingShape2D = mapBoundary;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            player.transform.position = playerTarget.position;
            SetSprite(player.GetComponentInChildren<SpriteRenderer>(), playerBattleSprite);
        }

        npc.transform.position = npcTarget.position;
        SetSprite(npc.GetComponentInChildren<SpriteRenderer>(), npcBattleSprite);

        Physics2D.SyncTransforms();

        if (mapBoundary != null)
            MapController_man.Instance?.HighlightArea(mapBoundary.name);

        await ScreenFader.Instance.FadeIn();

        DialogueController.Instance.ShowDialogueUI(true);
        npc.ResumeDialogue();

        var detector = player != null ? player.GetComponentInChildren<InteractionDetector>() : null;
        if (detector != null)
            detector.Register(npc);

        Debug.Log($"NPC Target {npcTarget.position} | Actual {npc.transform.position}");
    }

    private void SetSprite(SpriteRenderer sr, Sprite sprite) {
        if (sr == null || sprite == null) return;
        sr.sprite = sprite;
        sr.flipX = false;
    }
    public void ResetSprite() {
        SetSprite(npc.GetComponentInChildren<SpriteRenderer>(), npcOriginalSprite);
    }
}
