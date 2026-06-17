using UnityEngine;
using UnityEngine.UI;          // UI Image 제어를 위해 필수
using UnityEngine.SceneManagement;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    private enum BattleState { Charging, PlayerTurn, EnemyTurn, End }
    private BattleState currentState;

    [Header("── 캐릭터 스탯 스크립트 연동 ──")]
    public CharacterStats playerStats; // 인스펙터에서 연결하거나 Start에서 자동 수집
    private CharacterStats enemyStats;  // 생성된 적에게서 동적으로 가져옴

    [Header("── 속도 설정 (ATB 충전 속도) ──")]
    public float playerSpeed = 0.5f;
    public float enemySpeed = 0.4f;

    private float playerGauge = 0f;
    private float enemyGauge = 0f;

    private int playerDefenseBuff = 0;
    private int enemyDefenseBuff = 0;

    [Header("── UI 게이지 및 HP 바 (Image Filled 타입) ──")]
    public Image playerGaugeImage; // PlayerTurnBased (Image) 매핑
    public Image enemyGaugeImage;  // EnemyTurnBased (Image) 매핑
    public Image playerHPImage;    // PlayerHP (Image) 매핑
    public Image enemyHPImage;     // 적 체력바 (Image) 매핑

    [Header("── UI 버튼 및 행동 패널 ──")]
    public GameObject actionPanel;
    public Button attackButton;
    public Button defenseButton;
    public Button runButton;

    [Header("── 동적 적 생성 시스템 ──")]
    public GameObject[] enemyPrefabs; // 필드의 적 이름과 매칭될 전투용 적 프리팹 목록들
    public Transform enemySpawnPoint; // 전투 씬에서 적이 스폰될 위치 (Empty Object)

    void Start()
    {
        // 1. 전투 씬 전용 적 프리팹 생성 및 스탯 동기화
        SpawnBattleEnemy();

        // 2. 플레이어 스탯 스크립트 자동 수집 및 초기화
        if (playerStats == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerStats = playerObj.GetComponent<CharacterStats>();
        }

        if (playerStats != null)
        {
            playerStats.UpdateStats();
            // 필요 시 전투 시작할 때 플레이어 체력을 풀피로 채우거나 유지할 수 있습니다.
            playerStats.currentHealth = playerStats.MaxHealth;
        }

        // 3. 변수 및 UI 초기화
        playerGauge = 0f;
        enemyGauge = 0f;
        playerDefenseBuff = 0;
        enemyDefenseBuff = 0;

        actionPanel.SetActive(false);
        UpdateHPUI();
        UpdateGaugeUI();

        // 4. 버튼 이벤트 연결
        if (attackButton != null) attackButton.onClick.AddListener(OnPlayerAttack);
        if (defenseButton != null) defenseButton.onClick.AddListener(OnPlayerDefense);
        if (runButton != null) runButton.onClick.AddListener(OnPlayerRun);

        currentState = BattleState.Charging;
        Debug.Log("⚔️ 실시간 스탯 기반 ATB 전투 시작!");
    }

    void Update()
    {
        if (currentState != BattleState.Charging) return;

        // 게이지 지속 충전
        playerGauge += playerSpeed * Time.deltaTime;
        enemyGauge += enemySpeed * Time.deltaTime;

        UpdateGaugeUI();

        if (playerGauge >= 1f)
        {
            playerGauge = 1f;
            StartPlayerTurn();
        }
        else if (enemyGauge >= 1f)
        {
            enemyGauge = 1f;
            StartEnemyTurn();
        }
    }

    // ────────────────────────────────────────────────────────
    // 🤖 동적 적 프리팹 매칭 생성 함수
    // ────────────────────────────────────────────────────────
    void SpawnBattleEnemy()
    {
        string targetName = BattleTransferData.enemyName;
        GameObject selectedPrefab = null;

        // 만약 직접 Battle 씬을 실행해서 데이터가 비어있다면, 첫 번째 프리팹을 테스트용으로 강제 세팅
        if (string.IsNullOrEmpty(targetName) && enemyPrefabs.Length > 0)
        {
            targetName = enemyPrefabs[0].name;
            BattleTransferData.enemyAttack = 15;
            BattleTransferData.enemyMaxHealth = 100;
            BattleTransferData.enemyDefense = 5;
            BattleTransferData.enemyCurrentHealth = 100;
        }

        // 인스펙터에 넣어둔 프리팹 배열에서 이름이 똑같은 녀석을 검색
        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab.name == targetName)
            {
                selectedPrefab = prefab;
                break;
            }
        }

        if (selectedPrefab != null)
        {
            // 스폰 위치가 지정되어 있으면 그곳에, 없으면 원점에 적 생성
            Vector3 spawnPos = enemySpawnPoint != null ? enemySpawnPoint.position : Vector3.zero;
            GameObject spawnedEnemy = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);

            // 생성된 적의 스탯 컴포넌트를 가져와 필드에서 복사해 온 기본 데이터 덮어쓰기
            enemyStats = spawnedEnemy.GetComponent<CharacterStats>();
            if (enemyStats != null)
            {
                enemyStats.baseAttack = BattleTransferData.enemyAttack;
                enemyStats.baseMaxHealth = BattleTransferData.enemyMaxHealth;
                enemyStats.baseDefense = BattleTransferData.enemyDefense;

                // CharacterStats 내부 함수를 새로고침하여 최종 변수 세팅
                enemyStats.UpdateStats();
                enemyStats.currentHealth = BattleTransferData.enemyCurrentHealth;

                Debug.Log($"🎯 전투 프리팹 [{targetName}] 소환 및 스탯 동기화 성공!");
            }
        }
        else
        {
            Debug.LogError($"❌ 에러: enemyPrefabs 목록에 '{targetName}'과 일치하는 프리팹 이름이 없습니다!");
        }
    }

    // ────────────────────────────────────────────────────────
    // ⚔️ 턴 제어 및 전투 수식 계산 (CharacterStats 연동)
    // ────────────────────────────────────────────────────────
    void StartPlayerTurn()
    {
        currentState = BattleState.PlayerTurn;
        actionPanel.SetActive(true);
    }

    void OnPlayerAttack()
    {
        if (playerStats == null || enemyStats == null) return;

        // 기획서 공식: 데미지 = 공격력 - 적의방어버프
        int damage = playerStats.Attack;
        int finalDamage = Mathf.Max(1, damage - enemyDefenseBuff);

        enemyStats.currentHealth -= finalDamage;
        Debug.Log($"💥 플레이어 공격! [{enemyStats.gameObject.name}]에게 {finalDamage} 피해. (적 버프 차감전: {enemyDefenseBuff})");

        enemyDefenseBuff = 0; // 방어 버프 소모

        UpdateHPUI();
        EndPlayerTurn();
    }

    void OnPlayerDefense()
    {
        if (playerStats == null) return;

        // 즉시 체력 회복 (최대 체력의 10%)
        int healAmount = Mathf.RoundToInt(playerStats.MaxHealth * 0.1f);
        playerStats.currentHealth = Mathf.Min(playerStats.MaxHealth, playerStats.currentHealth + healAmount);

        // 현재 방어력 기반 방어 버프 획득
        playerDefenseBuff = playerStats.Defense;
        Debug.Log($"🛡️ 플레이어 방어! 체력 +{healAmount} 회복 및 방어버프 +{playerDefenseBuff} 생성.");

        UpdateHPUI();
        EndPlayerTurn();
    }

    void OnPlayerRun()
    {
        if (Random.value <= 0.5f)
        {
            currentState = BattleState.End;
            Debug.Log("🏃 도망 성공! Level_1 씬으로 돌아갑니다.");
            SceneManager.LoadScene("Level_1");
        }
        else
        {
            Debug.Log("❌ 도망 실패!");
            EndPlayerTurn();
        }
    }

    void EndPlayerTurn()
    {
        playerGauge = 0f;
        UpdateGaugeUI();
        actionPanel.SetActive(false);

        if (CheckBattleEnd()) return;
        currentState = BattleState.Charging;
    }

    void StartEnemyTurn()
    {
        currentState = BattleState.EnemyTurn;
        StartCoroutine(EnemyAIActionCoroutine());
    }

    IEnumerator EnemyAIActionCoroutine()
    {
        yield return new WaitForSeconds(1.0f);

        if (playerStats != null && enemyStats != null)
        {
            if (Random.value <= 0.5f)
            {
                // 적의 공격
                int damage = enemyStats.Attack;
                int finalDamage = Mathf.Max(1, damage - playerDefenseBuff);

                playerStats.currentHealth -= finalDamage;
                Debug.Log($"💥 적의 공격! 플레이어에게 {finalDamage} 피해. (플레이어 버프 차감전: {playerDefenseBuff})");

                playerDefenseBuff = 0;
            }
            else
            {
                // 적의 방어
                int healAmount = Mathf.RoundToInt(enemyStats.MaxHealth * 0.1f);
                enemyStats.currentHealth = Mathf.Min(enemyStats.MaxHealth, enemyStats.currentHealth + healAmount);
                enemyDefenseBuff = enemyStats.Defense;
                Debug.Log($"🛡️ 적 방어! 체력 +{healAmount} 회복 및 방어버프 +{enemyDefenseBuff} 생성.");
            }
        }

        UpdateHPUI();
        EndEnemyTurn();
    }

    void EndEnemyTurn()
    {
        enemyGauge = 0f;
        UpdateGaugeUI();

        if (CheckBattleEnd()) return;
        currentState = BattleState.Charging;
    }

    // ────────────────────────────────────────────────────────
    // 📊 Image (Filled 타입) 전용 UI 업데이트 함수
    // ────────────────────────────────────────────────────────
    void UpdateGaugeUI()
    {
        // Slider의 .value 대신 Image의 .fillAmount (0.0 ~ 1.0)를 조절합니다
        if (playerGaugeImage != null) playerGaugeImage.fillAmount = playerGauge;
        if (enemyGaugeImage != null) enemyGaugeImage.fillAmount = enemyGauge;
    }

    void UpdateHPUI()
    {
        // (현재 체력 / 최대 체력) 비율을 계산해 fillAmount에 대입합니다
        if (playerStats != null && playerHPImage != null)
        {
            playerHPImage.fillAmount = (float)playerStats.currentHealth / playerStats.MaxHealth;
        }

        if (enemyStats != null && enemyHPImage != null)
        {
            enemyHPImage.fillAmount = (float)enemyStats.currentHealth / enemyStats.MaxHealth;
        }
    }

    bool CheckBattleEnd()
    {
        if (enemyStats != null && enemyStats.currentHealth <= 0)
        {
            currentState = BattleState.End;
            Debug.Log("🏆 전투 승리! 메인 월드로 돌아갑니다.");
            StartCoroutine(ExitBattleScene());
            return true;
        }
        else if (playerStats != null && playerStats.currentHealth <= 0)
        {
            currentState = BattleState.End;
            Debug.Log("💀 플레이어 사망... 게임 오버!");
            SceneManager.LoadScene("GameOver");
            return true;
        }
        return false;
    }

    IEnumerator ExitBattleScene()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("Level_1");
    }
}