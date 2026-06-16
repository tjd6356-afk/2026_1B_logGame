using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem; // New Input System 연동을 위해 필수!

public class DialogueManager : MonoBehaviour
{
    [Header("UI 연결")]
    public GameObject DialoguePanel;
    public Image characterImage;
    public TextMeshProUGUI charcternameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;

    [Header("기본 설정")]
    public Sprite defaultCharacterImage;

    [Header("타이핑 효과 설정")]
    public float typingSpeed = 0.05f;
    public bool skipTypingOnClick = true;

    private DialogueDataSO currentDialogue;
    private int currentLineIndex = 0;
    private bool isDialogueActive = false;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        DialoguePanel.SetActive(false);
        nextButton.onClick.AddListener(HandleNextInput);
    }

    void Update()
    {
        // ★ New Input System 스타일로 업데이트
        // 대화가 활성화된 상태에서 Space 키 또는 E 키를 누르면 다음 대사 처리 진행
        if (isDialogueActive && Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
            {
                HandleNextInput();
            }
        }
    }

    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        dialogueText.text = "";

        for (int i = 0; i < textToType.Length; i++)
        {
            dialogueText.text += textToType[i];
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void CompleteTyping()
    {
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        isTyping = false;

        if (currentDialogue != null && currentLineIndex < currentDialogue.dialogueLines.Count)
        {
            dialogueText.text = currentDialogue.dialogueLines[currentLineIndex];
        }
    }

    void ShowCurrentLine()
    {
        if(currentDialogue != null && currentLineIndex < currentDialogue.dialogueLines.Count)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            string currentText = currentDialogue.dialogueLines[currentLineIndex];
            typingCoroutine = StartCoroutine(TypeText(currentText));
        }
    }

    public void ShowNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.dialogueLines.Count)
        {
            EndDialogue();
        }
        else
        {
            ShowCurrentLine();
        }
    }

    void EndDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isDialogueActive = false;
        isTyping = false;
        DialoguePanel.SetActive(false);
        currentLineIndex = 0;

            PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetFreeze(false);
        }
    }

    public void HandleNextInput()
    {
        if (isTyping && skipTypingOnClick)
        {
            CompleteTyping();
        }
        else if (!isTyping)
        {
            ShowNextLine();
        }
    }

    public void SkipDialogue()
    {
        EndDialogue();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void StartDialogue(DialogueDataSO dialogue)
    {
        if (dialogue == null || dialogue.dialogueLines.Count == 0) return;

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueActive = true;

        DialoguePanel.SetActive(true);
        charcternameText.text = dialogue.characterName;
        
        if (characterImage != null)
        {
            if (dialogue.characterImage != null)
            {
                characterImage.sprite = dialogue.characterImage;
            }
            else
            {
                characterImage.sprite = defaultCharacterImage;
            }
        }

        ShowCurrentLine();

            PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            player.SetFreeze(true);
        }
    }
}