using UnityEngine;

public class DialogueNPC : MonoBehaviour
{
    public DialogueDataSO myDialogue;
    private DialogueManager dialogueManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueManager = FindAnyObjectByType<DialogueManager>();

        if (dialogueManager != null )
        {
            Debug.LogError("다이얼 로그 매니저가 없습니다.");
        }
    }
    
    private void OnMouseDown()
    {
        if (dialogueManager == null) return;
        if (dialogueManager.IsDialogueActive()) return;
        if (myDialogue  == null) return;

        dialogueManager.StartDialogue(myDialogue);
    }

}
