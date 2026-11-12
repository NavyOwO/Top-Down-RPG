using System.Collections;
using TMPro;
using Unity.Multiplayer.Center.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class NPC : MonoBehaviour, IInteractable {
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive, awaitingChoice, isHeld;

    public void HoldDialogue() {
        isHeld = true;
        StopAllCoroutines();
        isTyping = false;
        isDialogueActive = false;
    }
    public void ResumeDialogue() {
        isHeld = false;
        //isDialogueActive = true;
        if (isDialogueActive) {
            dialogueUI.ClearChoices();
            DisplayCurrentLine();
        }
    }

    [System.Serializable]
    public class ChoiceEvent {
        public string id;
        public UnityEvent onChosen;
    };
    public ChoiceEvent[] choiceEvents;

    private void Start() {
        dialogueUI = DialogueController.Instance;
    }
    public bool CanInteract() {
        return !isDialogueActive && !isHeld;
    }

    public void Interact() {
        //if (isDialogueActive && (DialogueController.Instance == null || !DialogueController.Instance.IsDialogueUIActive())) EndDialogue();
        if (dialogueData == null || PauseController.IsGamePaused && !isDialogueActive) 
            return; 

        if (isDialogueActive) {
            NextLine();
        } else {
            StartDialogue();
        }
    }

    void StartDialogue() {
        isDialogueActive = true;
        awaitingChoice = false;
        isHeld = false;
        dialogueIndex = 0;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);

        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        DisplayCurrentLine();
    }
    void NextLine() {
        if (awaitingChoice || isHeld) return;

        if (isTyping) {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            TryShowChoices();
            return;
        }

        dialogueUI.ClearChoices();
        if (IsFlagged(dialogueData.endDialogueLines, dialogueIndex)) {
            EndDialogue();
            return; 
        }

        if (++dialogueIndex < dialogueData.dialogueLines.Length) {
            DisplayCurrentLine();
        } else {
            EndDialogue();
        }
    }
    bool TryShowChoices() {
        foreach (DialogueChoice c in dialogueData.choices) {
            if (c.dialogueIndex == dialogueIndex) {
                awaitingChoice = true;
                DisplayChoices(c);
                return true;
            }
        }
        return false;
    }

    static bool IsFlagged(bool[] flags, int i) =>
        flags != null && i < flags.Length && flags[i];

    IEnumerator TypeLine() {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        var sb = new System.Text.StringBuilder();
        foreach (char letter in dialogueData.dialogueLines[dialogueIndex]) {
            sb.Append(letter);
            dialogueUI.SetDialogueText(sb.ToString());
            SoundEffectManager.PlayVoice(dialogueData.voiceSound, dialogueData.voicecPitch);
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        isTyping = false;

        if (TryShowChoices()) yield break;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex]) {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice) {
        for (int i = 0; i < choice.choices.Length; i++) {
            int nextIndex = choice.nextDialogueIndexes[i];
            string eventId = (choice.eventIds != null && i < choice.eventIds.Length)
            ? choice.eventIds[i] : null;
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex, eventId));
        }
    }

    void ChooseOption(int nextIndex, string eventId) {
        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        awaitingChoice = false;

        TriggerChoiceEvent(eventId);
        if (!isDialogueActive || isHeld) return;
        DisplayCurrentLine();
    }
    void TriggerChoiceEvent(string eventId) {
        if (string.IsNullOrEmpty(eventId)) return;

        foreach (var e in choiceEvents) {
            if (e.id == eventId) {
                e.onChosen?.Invoke();
                return;
            }
        }
    }

    void DisplayCurrentLine() {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue() {
        StopAllCoroutines();
        awaitingChoice = false;
        isDialogueActive = false;
        isHeld = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);
        Debug.Log("Ended dialogue");
    }
}
