using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]

public class NPCDialogue : ScriptableObject {
    [SerializeField] public string npcName;
    [SerializeField] public Sprite npcPortrait;
    [SerializeField] public string[] dialogueLines;
    [SerializeField] public bool[] autoProgressLines;
    [SerializeField] public float autoProgressDelay = 1.5f;
    [SerializeField] public float typingSpeed = 0.05f;
    [SerializeField] public AudioClip voiceSound;
    [SerializeField] public float voicecPitch = 1f;
}
