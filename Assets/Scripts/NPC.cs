using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class NPC : MonoBehaviour, IInteractable {
    public NPCDialogue dialogueData;

    [Tooltip("After the dialogue finishes, remove NPC from map")]
    public bool removeAfterDialogue;

    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive, awaitingChoice, isHeld;

    [System.Serializable]
    public class ChoiceEvent {
        public string id;
        [Tooltip("Close the dialogue (and unpause) before running onChosen. Tick this for choices that start a battle.")]
        public bool endDialogueFirst;
        public UnityEvent onChosen;
    };
    public ChoiceEvent[] choiceEvents;

    private void Start() {
        dialogueUI = DialogueController.Instance;
    }
    public void HoldDialogue() {
        isHeld = true;
        StopAllCoroutines();
        isTyping = false;
    }

    public void ResumeDialogue() {
        isHeld = false;
        if (isDialogueActive) {
            dialogueUI.ClearChoices();
            DisplayCurrentLine();
        }
    }

    public bool CanInteract() {
        return !isHeld;
    }

    public void Interact() {
        if (dialogueData == null || PauseController.IsGamePaused && !isDialogueActive)
            return;

        if (isDialogueActive) {
            NextLine();
        }
        else {
            StartDialogue(0);
        }
    }
    public void StartDialogueAt(int lineIndex) {
        if (dialogueData == null) return;
        battlePending = false;
        StartDialogue(lineIndex);
    }
    void StartDialogue(int startIndex) {
        isDialogueActive = true;
        awaitingChoice = false;
        isHeld = false;
        dialogueIndex = startIndex;

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
        }
        else {
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
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed); //realtime is unaffected by pause
        }
        isTyping = false;

        if (TryShowChoices()) yield break;

        var auto = dialogueData.autoProgressLines;
        if (auto != null && auto.Length > dialogueIndex && auto[dialogueIndex]) {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
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
        if (!isDialogueActive || isHeld) return; // event ended or held the dialogue
        DisplayCurrentLine();
    }
    void TriggerChoiceEvent(string eventId) {
        if (string.IsNullOrEmpty(eventId) || choiceEvents == null) return;

        foreach (var e in choiceEvents) {
            if (e.id == eventId) {
                if (e.endDialogueFirst) {
                    battlePending = true;
                    FinishDialogue(allowRemoval: false);
                }
                e.onChosen?.Invoke();
                return;
            }
        }
    }

    public void EndDialogue() => FinishDialogue(allowRemoval: true);

    void DisplayCurrentLine() {
        StopAllCoroutines();
        if (dialogueIndex < 0 || dialogueIndex >= dialogueData.dialogueLines.Length) {
            EndDialogue();
            return;
        }
        StartCoroutine(TypeLine());
    }
    private bool battlePending;
    public void FinishDialogue(bool allowRemoval) {
        StopAllCoroutines();
        awaitingChoice = false;
        isTyping = false;
        isDialogueActive = false;
        isHeld = false;
        dialogueUI.ClearChoices();
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);

        if (allowRemoval && !battlePending && removeAfterDialogue && !BattleManager.IsBattleActive) {
            _ = RemoveWithFade();
            return;
        }

        if (!BattleManager.IsBattleActive) PauseController.SetPause(false);
    }
    async Task RemoveWithFade() {
        var fader = ScreenFader.Instance;
        if (fader != null) await fader.FadeOut();

        gameObject.SetActive(false);

        if (fader != null) await fader.FadeIn();

        if (!BattleManager.IsBattleActive) PauseController.SetPause(false);
    }
}