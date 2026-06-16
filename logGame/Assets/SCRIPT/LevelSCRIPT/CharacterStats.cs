using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [Header(" 기본 스탯 설정 (인스펙터에서 수정 가능)")]
    public int baseAttack = 10;
    public int baseMaxHealth = 100;
    public int baseDefense = 5;

    // 다른 스크립트(전투, UI 등)에서 가져가서 쓸 최종 실시간 스탯 (기본 스탯 + 장착 보너스)
    public int Attack { get; private set; }
    public int MaxHealth { get; private set; }
    public int Defense { get; private set; }

    [HideInInspector]
    public int currentHealth; // 실시간 현재 체력 (데미지 입을 때 깎이는 수치)

    private bool isPlayer = false;

    private void Awake()
    {
        // 오브젝트의 태그가 "Player"인 경우 플레이어로 인식합니다.
        isPlayer = CompareTag("Player");
    }

    private void Start()
    {
        UpdateStats();
        currentHealth = MaxHealth; // 게임 시작 시 현재 체력을 최대 체력으로 초기화
    }

    // ★ 스탯을 최신 상태로 계산하는 함수 (아이템 장착/해제 시 인벤토리에서 호출해 줄 겁니다)
    public void UpdateStats()
    {
        if (isPlayer)
        {
            int bonusAttack = 0;
            int bonusHealth = 0;
            int bonusDefense = 0;

            // GameDataManager에서 현재 장착된 아이템 이름을 가져옴
            string equippedName = GameDataManager.instance.playerData.equippedItem;

            if (!string.IsNullOrEmpty(equippedName))
            {
                // 씬에 있는 InventoryManager를 찾아 해당 아이템의 스탯 데이터를 가져옴
                InventoryManager inv = FindFirstObjectByType<InventoryManager>();
                if (inv != null)
                {
                    ItemData equippedItemData = inv.itemDatabase.Find(x => x.itemName == equippedName);
                    if (equippedItemData != null)
                    {
                        bonusAttack = equippedItemData.attackBonus;
                        bonusHealth = equippedItemData.healthBonus;
                        bonusDefense = equippedItemData.defenseBonus;
                    }
                }
            }

            // 플레이어 최종 스탯 = 기본 스탯 + 아이템 보너스
            Attack = baseAttack + bonusAttack;
            MaxHealth = baseMaxHealth + bonusHealth;
            Defense = baseDefense + bonusDefense;

            Debug.Log($"[플레이어 스탯 갱신] 공격력: {Attack}(+{bonusAttack}), 체력: {MaxHealth}(+{bonusHealth}), 방어력: {Defense}(+{bonusDefense})");
        }
        else
        {
            // 몬스터나 NPC는 아이템 장착 보너스 없이 기본 스탯만 사용
            Attack = baseAttack;
            MaxHealth = baseMaxHealth;
            Defense = baseDefense;
        }
    }

    // 데미지 받는 함수 예시 (나중에 전투 시스템 만들 때 참고용)
    public void TakeDamage(int damage)
    {
        int finalDamage = damage - Defense;
        if (finalDamage < 1) finalDamage = 1; // 최소 데미지 보장

        currentHealth -= finalDamage;
        Debug.Log($"{gameObject.name}이(가) {finalDamage}의 데미지를 받음. 남은 체력: {currentHealth}/{MaxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} 사망!");
        if (isPlayer)
        {
            GameDataManager.instance.PlayerDead();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}