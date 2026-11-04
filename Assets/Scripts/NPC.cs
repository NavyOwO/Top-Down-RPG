using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable {
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    private void Start() {
        dialogueUI = DialogueController.Instance;
    }
    public bool CanInteract() {
        return !isDialogueActive;
    }

    public void Interact() {
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
        dialogueIndex = 0;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);

        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        DisplayCurrentLine();
    }
    void NextLine() {
        if (isTyping) {
            //Debug.Log("isTyping true");
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            return;
        }
        //Debug.Log("isTyping false");
        StopAllCoroutines();
        //Debug.Log("Coroutines Stopped");

        if (++dialogueIndex < dialogueData.dialogueLines.Length) {
            DisplayCurrentLine();
            //Debug.Log("Displaying Line");
        }

        dialogueUI.ClearChoices();
        Debug.Log("Cleared Choices");

        if (dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex]) {
            EndDialogue();
            Debug.Log($"Ended dialogue now (at end index [index = {dialogueIndex}])");
            return; 
        }

        foreach (DialogueChoice dialogueChoice in dialogueData.choices) {
            if (dialogueChoice.dialogueIndex == dialogueIndex) {
                DisplayChoices(dialogueChoice);
                Debug.Log("Displayed Choices");
                return;
            }
        }

        if (++dialogueIndex < dialogueData.dialogueLines.Length) {
            DisplayCurrentLine();
        } else {
            EndDialogue();
            Debug.Log("Ending dialogue (++index > length)");
        }
    }

    IEnumerator TypeLine() {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex]) {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);
            SoundEffectManager.PlayVoice(dialogueData.voiceSound, dialogueData.voicecPitch);
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex]) {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }
    void DisplayChoices(DialogueChoice choice) {
        for (int i = 0; i < choice.choices.Length; i++) {
            int nextIndex = choice.nextDialogueIndexes[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex));
        }
    }
    void ChooseOption(int nextIndex) {
        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        DisplayCurrentLine(); //displays line after choice before enddialogue can get to it
    }
    void DisplayCurrentLine() {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }
    public void EndDialogue() {
        StopAllCoroutines();     
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);
        Debug.Log("Ended dialogue");
    }
}
