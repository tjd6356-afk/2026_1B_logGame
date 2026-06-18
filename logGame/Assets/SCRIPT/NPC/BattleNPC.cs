using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleNPC : MonoBehaviour
{
    [Header("전투용 적 프리팹 설정")]
    [SerializeField] private string battleMonsterPrefabName; // BattleManager의 배열에 등록된 프리팹 이름과 정확히 일치해야 합니다.

    [Header("NPC 전투 스탯")]
    [SerializeField] private int attack = 12;
    [SerializeField] private int maxHealth = 60;
    [SerializeField] private int defense = 4;

    // 대화 시작 및 전투 발동 함수 (기존 다이얼로그 시스템이나 버튼에서 이 함수를 호출하게 만듭니다)
    public void StartNPCBattle()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 1. 현재 플레이어의 위치 기억
            BattleTransferData.playerFieldPosition = player.transform.position;

            CharacterStats playerStats = player.GetComponent<CharacterStats>();
            if (playerStats != null)
            {
                BattleTransferData.playerCurrentHealth = playerStats.currentHealth;
            }
        }

        // 2. static 데이터 상자에 NPC의 전투 스탯 배달
        BattleTransferData.enemyName = battleMonsterPrefabName;
        BattleTransferData.enemyAttack = attack;
        BattleTransferData.enemyMaxHealth = maxHealth;
        BattleTransferData.enemyDefense = defense;
        BattleTransferData.enemyCurrentHealth = maxHealth;

        // 3. NPC 전투 플래그 세팅
        BattleTransferData.isNPCBattle = true;
        BattleTransferData.encounteredNPCInstanceID = gameObject.GetInstanceID();
        BattleTransferData.isBattleWon = false;

        Debug.Log($"⚔️ NPC [{gameObject.name}] 전투 준비 완료 -> 씬 전환");
        SceneManager.LoadScene("Battle");
    }

    // ★ [핵심] 전투에서 이기고 돌아왔을 때 실행될 함수
    public void OnBattleWonReturn()
    {
        Debug.Log($"🏆 [{gameObject.name}] 전투 승리 후 필드 복귀 성공! 다음 다이얼로그를 출력합니다.");

        // TODO: 개발자님이 사용 중이신 DialogueManager를 호출하여 다음 대화(승리 후 대화)를 시작하는 코드를 넣으세요.
        // 예시 구조:
        // DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        // if (dialogueManager != null) {
        //     dialogueManager.StartDialogue("승리_후_대화_데이터");
        // }

        // 만약 이긴 뒤 이 NPC를 맵에서 영구적으로 없애고 싶다면 아래 주석을 해제하세요.
        // Destroy(gameObject);
    }
}