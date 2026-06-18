using UnityEngine;
using UnityEngine.SceneManagement; // �� ��ȯ�� ���� �ʼ�!

public class EnemyBattleTrigger : MonoBehaviour
{
    [Header("������ ��Ʋ �� �̸�")]
    public string battleSceneName = "Battle";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // �浹�� ������Ʈ�� �÷��̾�(Player �±�)���� Ȯ��
        if (collision.CompareTag("Player"))
        {
            Debug.Log("���� �浹! ��Ʋ ������ �̵��մϴ�.");

            // �ʿ��ϴٸ� �̵��ϱ� �� GameDataManager.instance.SaveData(...)�� ȣ���� ���� ���¸� ������ �� �ֽ��ϴ�.
            CharacterStats playerStats = collision.GetComponent<CharacterStats>();
                if (playerStats != null)
                {
                    BattleTransferData.playerCurrentHealth = playerStats.currentHealth;
                }
            // ��Ʋ �� �ε�
            SceneManager.LoadScene(battleSceneName);
        }
    }
}