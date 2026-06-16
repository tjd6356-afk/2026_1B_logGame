using UnityEngine;
using UnityEngine.InputSystem; // New Input System 연동을 위해 필수!

public class DialogueNPC : MonoBehaviour
{
    public DialogueDataSO myDialogue;
    private DialogueManager dialogueManager;
    private bool isPlayerInRange = false; // 플레이어가 상호작용 범위 내에 있는지 확인하는 플래그

    void Start()
    {
        dialogueManager = FindAnyObjectByType<DialogueManager>();

        // ★ 에러 해결: 기존의 반대로 되어 있던 조건문(!= null)을 올바르게(== null) 고쳤습니다.
        if (dialogueManager == null)
        {
            Debug.LogError($" [{gameObject.name}] 씬에서 DialogueManager를 찾을 수 없습니다! Canvas 내부에 매니저 오브젝트가 있는지 확인하세요.");
        }
    }

    void Update()
    {
        // 플레이어가 범위 안에 있고, New Input System 방식으로 E 키가 이번 프레임에 눌렸을 때
        if (isPlayerInRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (dialogueManager == null) return;

            // 이미 대화창이 켜져 있는 상태가 아니고, 대사 데이터가 존재할 때만 대화 시작
            if (!dialogueManager.IsDialogueActive() && myDialogue != null)
            {
                dialogueManager.StartDialogue(myDialogue);
            }
        }
    }

    // ────────────────────────────────────────────────────────
    //  트리거 영역 진입 및 이탈 체크
    // ────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트의 태그가 "Player"인지 확인합니다.
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            Debug.Log($"[대화 가능] {gameObject.name}에게 접근했습니다. (E 키를 누르면 대화 시작)");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("[대화 불가능] NPC 영역을 벗어났습니다.");
            
            // 만약 대화 중에 플레이어가 도망치면 대화창을 강제로 닫고 싶다면 아래 주석을 해제하세요.
            // if (dialogueManager != null && dialogueManager.IsDialogueActive()) { dialogueManager.SkipDialogue(); }
        }
    }
}