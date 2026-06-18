using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header("📊 캐릭터 기본 스탯")]
    public int baseAttack = 10;
    public int baseMaxHealth = 100;
    public int baseDefense = 5;

    [Header("🛡️ 현재 장착 중인 아이템 데이터 (ScriptableObject)")]
    public ItemData equippedItemData; 

    // ★ ItemData에서 가져온 보너스 스탯을 임시 저장할 내부 변수들
    private int itemAttackBonus = 0;
    private int itemHealthBonus = 0;
    private int itemDefenseBonus = 0;

    [HideInInspector] public int currentHealth;

    // ────────────────────────────────────────────────────────
    // ⚔️ [최종 스탯 프로퍼티] 아이템 보너스 + 사망 배율 동시 계산
    // ────────────────────────────────────────────────────────
    public int Attack
    {
        get
        {
            // 1. 기본 공격력 + 장착한 ItemData의 attackBonus를 먼저 더합니다.
            int totalAttackWithItem = baseAttack + itemAttackBonus;

            // 2. 거기에 죽은 횟수만큼 2배율 버프를 곱해줍니다. (최대 3회 제한)
            int clampedDeath = Mathf.Clamp(BattleTransferData.deathCount, 0, 3);
            int deathMultiplier = Mathf.RoundToInt(Mathf.Pow(2, clampedDeath)); // 1배, 2배, 4배, 8배

            return totalAttackWithItem * deathMultiplier;
        }
    }

    // 최대 체력과 방어력도 ItemData의 보너스 변수명을 그대로 반영합니다.
    public int MaxHealth => baseMaxHealth + itemHealthBonus;
    public int Defense => baseDefense + itemDefenseBonus;

    void Start()
    {
        // 게임 시작 시 아이템 보너스 스탯 먼저 계산
        UpdateStats();

        // 씬 전환 시 체력 연동 복구
        if (BattleTransferData.playerCurrentHealth != -1)
            currentHealth = BattleTransferData.playerCurrentHealth;
        else
            currentHealth = MaxHealth;
    }

    // 🔄 UI 창이 켜지거나 아이템을 꼈다 뺄 때 호출되어 스탯을 최신화하는 함수
    public void UpdateStats()
    {
        // 장착된 아이템(ScriptableObject)이 존재한다면
        if (equippedItemData != null)
        {
            // ★ 올려주신 ItemData의 실제 변수명을 그대로 꽂아줍니다!
            itemAttackBonus = equippedItemData.attackBonus;
            itemHealthBonus = equippedItemData.healthBonus;
            itemDefenseBonus = equippedItemData.defenseBonus;
        }
        else
        {
            // 장착한 아이템이 없다면 보너스는 전부 0
            itemAttackBonus = 0;
            itemHealthBonus = 0;
            itemDefenseBonus = 0;
        }
    }

    // 외부에 의해 체력이 변할 때 상한선을 지켜주는 함수 (필요 시 활용)
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > MaxHealth) currentHealth = MaxHealth;
    }
}