using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필수!

public class EnemyBattleTrigger : MonoBehaviour
{
    [Header("진입할 배틀 씬 이름")]
    public string battleSceneName = "Battle";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 충돌한 오브젝트가 플레이어(Player 태그)인지 확인
        if (collision.CompareTag("Player"))
        {
            Debug.Log("적과 충돌! 배틀 씬으로 이동합니다.");

            // 필요하다면 이동하기 전 GameDataManager.instance.SaveData(...)를 호출해 현재 상태를 저장할 수 있습니다.

            // 배틀 씬 로드
            SceneManager.LoadScene(battleSceneName);
        }
    }
}