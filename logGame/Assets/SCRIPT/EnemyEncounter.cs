using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CharacterStats stats = GetComponent<CharacterStats>();
            if (stats != null)
            {
                // 생성 시 붙는 (Clone) 등의 문자열을 제거하고 순수 프리팹 이름만 추출
                string cleanName = gameObject.name.Replace("(Clone)", "").Trim();

                // 1. 적 스탯 저장 (이전 코드 유지)
                BattleTransferData.enemyName = cleanName;
                BattleTransferData.enemyAttack = stats.Attack;
                BattleTransferData.enemyMaxHealth = stats.MaxHealth;
                BattleTransferData.enemyDefense = stats.Defense;
                BattleTransferData.enemyCurrentHealth = stats.currentHealth;

                // 2. ★ 복귀용 데이터 저장
                // 트리거에 들어온 플레이어 오브젝트의 현재 위치 저장
                BattleTransferData.playerFieldPosition = collision.transform.position;
                BattleTransferData.enemyFieldPosition = transform.position;
                
                BattleTransferData.isNPCBattle = false;
                BattleTransferData.isBattleWon = false;

                Debug.Log($"⚔️ [{cleanName}] 스탯 및 위치 복사 완료 -> 전투 시작!");
                SceneManager.LoadScene("Battle");
            }
        }
    }
}