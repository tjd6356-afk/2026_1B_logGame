using UnityEngine;

public class FieldReturnManager : MonoBehaviour
{
    void Start()
    {
        // 전투 후 돌아온 상태가 아니면 실행 안 함
        if (string.IsNullOrEmpty(BattleTransferData.enemyName)) return;

        HandlePlayerReturn(); // 1. 플레이어 위치 복구

        // 2. NPC 전투였는지, 일반 필드 적 전투였는지 판단하여 처리
        if (BattleTransferData.isNPCBattle)
        {
            HandleNPCReturn();
        }
        else
        {
            HandleEnemyDestruction();
        }

        // 3. 사용한 데이터 상자 초기화
        ClearTransferData();
    }

    void HandlePlayerReturn()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && BattleTransferData.playerFieldPosition != Vector3.zero)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = BattleTransferData.playerFieldPosition;
            }
            player.transform.position = BattleTransferData.playerFieldPosition;
            Debug.Log("✅ 플레이어를 대화/전투 발생 위치로 복구했습니다.");
        }
    }

    void HandleNPCReturn()
    {
        // 이겼을 때만 NPC의 후속 대화(이벤트)를 발동시킵니다.
        if (BattleTransferData.isBattleWon)
        {
            // 씬에 배치된 모든 BattleNPC 컴포넌트를 검색
            BattleNPC[] fieldNPCs = FindObjectsByType<BattleNPC>(FindObjectsSortMode.None);
            int targetNPCID = BattleTransferData.encounteredNPCInstanceID;

            foreach (BattleNPC npc in fieldNPCs)
            {
                // 나와 싸웠던 바로 그 NPC를 ID값으로 매칭 성공!
                if (npc.gameObject.GetInstanceID() == targetNPCID)
                {
                    // NPC 스크립트에게 승리 대화를 틀으라고 명령 전달
                    npc.OnBattleWonReturn();
                    return;
                }
            }
        }
    }

    void HandleEnemyDestruction()
    {
        if (BattleTransferData.isBattleWon)
        {
            EnemyEncounter[] fieldEnemies = FindObjectsByType<EnemyEncounter>(FindObjectsSortMode.None);
            int targetID = BattleTransferData.encounteredEnemyInstanceID;

            foreach (EnemyEncounter enemy in fieldEnemies)
            {
                if (enemy.gameObject.GetInstanceID() == targetID)
                {
                    Destroy(enemy.gameObject);
                    return;
                }
            }
        }
    }

    void ClearTransferData()
    {
        BattleTransferData.enemyName = "";
        BattleTransferData.playerFieldPosition = Vector3.zero;
        BattleTransferData.encounteredEnemyInstanceID = 0;
        BattleTransferData.isNPCBattle = false;
        BattleTransferData.encounteredNPCInstanceID = 0;
        BattleTransferData.isBattleWon = false;
    }
}