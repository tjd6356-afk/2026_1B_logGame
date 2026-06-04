using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;      // GameDataManager의 collectedItems에 들어갈 코드명 (예: Item_1)
    public string displayName;   // UI 상에 보여줄 진짜 이름 (예: 강철 검)
    [TextArea]
    public string description;   // 아이템 상세 설명
    public Sprite icon;          // 인벤토리 슬롯에 표시될 이미지

    [Header("📊 아이템 장착 스탯 보너스")]
    public int attackBonus;      // 증가할 공격력
    public int healthBonus;      // 증가할 체력
    public int defenseBonus;     // 증가할 방어력

}