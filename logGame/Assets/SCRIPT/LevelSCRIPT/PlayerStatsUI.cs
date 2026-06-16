using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatsUI : MonoBehaviour
{
    [Header("📊 텍스트 UI 연결 (TextMeshPro)")]
    public TextMeshProUGUI attackText;     // 공격력 표시 텍스트
    public TextMeshProUGUI healthText;     // 체력 표시 텍스트
    public TextMeshProUGUI defenseText;    // 방어력 표시 텍스트

    [Header("❤️ HP 상태별 이미지 설정")]
    public Image hpStateImage;             // PlayerStats_Content 우측의 큰 Image 컴포넌트
    public Sprite[] hpSprites;             // HP 감소에 따라 바뀔 이미지 배열 (크기 자유)

    private CharacterStats playerStats;

    private void Awake()
    {
        // 씬에서 Player 태그를 가진 오브젝트의 스탯 컴포넌트를 가져옵니다.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerStats = player.GetComponent<CharacterStats>();
        }
    }

    // 인벤토리에서 'PlayerStats' 탭을 눌러 이 패널이 켜질 때마다 자동으로 실행됩니다.
    private void OnEnable()
    {
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        if (playerStats == null)
        {
            // 혹시 Awake 시점에 못 찾았다면 재검색
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerStats = player.GetComponent<CharacterStats>();
            else return;
        }

        // 스탯 데이터를 최신화 (아이템 장착 상태 반영)
        playerStats.UpdateStats();

        // 1. 추가 스탯 계산 (최종 스탯 - 기본 스탯)
        int bonusAttack = playerStats.Attack - playerStats.baseAttack;
        int bonusMaxHealth = playerStats.MaxHealth - playerStats.baseMaxHealth;
        int bonusDefense = playerStats.Defense - playerStats.baseDefense;

        // 2. UI 텍스트 UI에 반영 (요청하신 형태로 출력)
        if (attackText != null) 
            attackText.text = $"공격력 : {playerStats.baseAttack} (+{bonusAttack})";

        if (healthText != null) 
            healthText.text = $"체력 : {playerStats.currentHealth} / {playerStats.baseMaxHealth} (+{bonusMaxHealth})";

        if (defenseText != null) 
            defenseText.text = $"방어력 : {playerStats.baseDefense} (+{bonusDefense})";

        // 3. HP 감소 상태에 따른 이미지 순차 변경 로직
        if (hpStateImage != null && hpSprites != null && hpSprites.Length > 0)
        {
            // 현재 HP 비율 계산 (1.0 = 풀피, 0.0 = 사망)
            float hpRatio = (float)playerStats.currentHealth / playerStats.MaxHealth;
            
            // 비율을 뒤집어서 (0.0 = 풀피, 1.0 = 사망) 배열 인덱스로 전환
            float invertedRatio = 1f - hpRatio;
            int spriteIndex = Mathf.FloorToInt(invertedRatio * hpSprites.Length);
            
            // 인덱스가 배열 범위를 벗어나지 않도록 안전하게 고정(Clamp)
            spriteIndex = Mathf.Clamp(spriteIndex, 0, hpSprites.Length - 1);

            // 해당 순서의 이미지로 교체
            hpStateImage.sprite = hpSprites[spriteIndex];
        }
    }
}