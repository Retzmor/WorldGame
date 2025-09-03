using UnityEngine;

public class NPCDialogue : MonoBehaviour, IInteractuable
{
    [SerializeField] string[] dialogues;
    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(dialogues);
    }
}
