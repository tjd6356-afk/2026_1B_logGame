using UnityEngine;

public class FieldReturnManager : MonoBehaviour
{
    void Start()
    {
        if (string.IsNullOrEmpty(BattleTransferData.enemyName)) return;

        HandlePlayerReturn();

        if (BattleTransferData.isNPCBattle)
        {
            HandleNPCReturn();
        }
        else
        {
            HandleEnemyDestruction(); // 일반 적 삭제 처리
        }

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

    void HandleEnemyDestruction()
    {
        if (BattleTransferData.isBattleWon)
        {
            // 1. 씬에 새로 배치된 모든 EnemyEncounter 적들을 검색합니다.
            EnemyEncounter[] fieldEnemies = FindObjectsByType<EnemyEncounter>(FindObjectsSortMode.None);
            
            EnemyEncounter targetEnemy = null;
            float minDistance = float.MaxValue;
            float matchThreshold = 2.0f; // 2미터 이내의 가장 가까운 적을 탐색 (순찰 오차 고려)

            foreach (EnemyEncounter enemy in fieldEnemies)
            {
                string cleanName = enemy.gameObject.name.Replace("(Clone)", "").Trim();
                
                // 이름이 같은 녀석들 중에서
                if (cleanName == BattleTransferData.enemyName || enemy.gameObject.name.Contains(BattleTransferData.enemyName))
                {
                    // 전투가 일어났던 좌표와 리스폰된 이 적의 거리를 계산합니다.
                    float distance = Vector3.Distance(enemy.transform.position, BattleTransferData.enemyFieldPosition);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        targetEnemy = enemy;
                    }
                }
            }

            // 2. 조건에 맞는 가장 가까운 적을 월드에서 삭제합니다.
            if (targetEnemy != null && minDistance <= matchThreshold)
            {
                string destroyedName = targetEnemy.gameObject.name;
                Destroy(targetEnemy.gameObject);
                Debug.Log($"🗑️ 승리 확인: 위치 매칭을 통해 필드의 [{destroyedName}]를 삭제했습니다. (오차 거리: {minDistance:F2}m)");
            }
            else
            {
                // [백업 예외처리] 만약 거리가 멀거나 매칭이 안 풀렸을 경우 이름이 같은 첫 번째 대상을 강제 삭제
                foreach (EnemyEncounter enemy in fieldEnemies)
                {
                    string cleanName = enemy.gameObject.name.Replace("(Clone)", "").Trim();
                    if (cleanName == BattleTransferData.enemyName)
                    {
                        Destroy(enemy.gameObject);
                        Debug.Log($"🗑️ 예외 백업: 이름이 일치하는 [{cleanName}] 오브젝트를 안전하게 제거했습니다.");
                        break;
                    }
                }
            }
        }
    }

    void HandleNPCReturn()
    {
        if (BattleTransferData.isBattleWon)
        {
            BattleNPC[] fieldNPCs = FindObjectsByType<BattleNPC>(FindObjectsSortMode.None);
            
            BattleNPC targetNPC = null;
            float minDistance = float.MaxValue;

            foreach (BattleNPC npc in fieldNPCs)
            {
                float distance = Vector3.Distance(npc.transform.position, BattleTransferData.enemyFieldPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    targetNPC = npc;
                }
            }

            if (targetNPC != null && minDistance < 2.0f)
            {
                targetNPC.OnBattleWonReturn();
            }
        }
    }

    void ClearTransferData()
    {
        BattleTransferData.enemyName = "";
        BattleTransferData.playerFieldPosition = Vector3.zero;
        BattleTransferData.enemyFieldPosition = Vector3.zero;
        BattleTransferData.encounteredEnemyInstanceID = 0;
        BattleTransferData.isNPCBattle = false;
        BattleTransferData.encounteredNPCInstanceID = 0;
        BattleTransferData.isBattleWon = false;
    }
}