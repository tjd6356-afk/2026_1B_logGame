using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyEncounter : MonoBehaviour
{ // <- 스크린샷에서 이 클래스 시작 중괄호가 빠져 있어서 에러가 났었습니다!

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 필드에서 플레이어와 적 트리거가 충돌했을 때
        if (collision.CompareTag("Player"))
        {
            // 자신에게 붙어있는 CharacterStats 스탯 정보 가져오기
            CharacterStats stats = GetComponent<CharacterStats>();
            if (stats != null)
            {
                // 생성 시 붙는 (Clone) 등의 문자열을 제거하고 순수 프리팹 이름만 추출
                string cleanName = gameObject.name.Replace("(Clone)", "").Trim();

                // 정적 데이터 상자에 현재 적 스탯 실시간 저장
                BattleTransferData.enemyName = cleanName;
                BattleTransferData.enemyAttack = stats.Attack;
                BattleTransferData.enemyMaxHealth = stats.MaxHealth;
                BattleTransferData.enemyDefense = stats.Defense;
                BattleTransferData.enemyCurrentHealth = stats.currentHealth;

                Debug.Log($"⚔️ [{cleanName}] 스탯 복사 완료 -> 전투 씬으로 전환합니다.");
                SceneManager.LoadScene("Battle");
            }
        }
    }
}