using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// ────────────────────────────────────────────────────────
// 📦 데이터 저장 구조체 (사망 횟수 저장 변수 추가)
// ────────────────────────────────────────────────────────
[Serializable]
public class PlayerData
{
    public List<string> collectedItems = new List<string>();
    public int stage = 1;           
    public string equippedItem = "";
    public string sceneName = "";
    public float posX;
    public float posY;
    public int currentHealth;
    
    // ★ [추가] JSON 파일에 영구 저장될 사망 횟수
    public int deathCount = 0; 
}

// ────────────────────────────────────────────────────────
// 🎮 게임 데이터 매니저 (사망 복구 시스템 탑재)
// ────────────────────────────────────────────────────────
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager instance;
    public PlayerData playerData = new PlayerData();

    [Header("⚙️ 시스템 설정")]
    public bool isSaveMode = true;

    private bool pendingLoad = false;
    private Vector3 targetPosition;
    private int targetHealth;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // 💾 슬롯 버튼 통합 함수
    public void OnClickFileSlot(int slotNumber)
    {
        string filePath = Application.persistentDataPath + $"/player_data_slot_{slotNumber}.json";

        if (!File.Exists(filePath))
        {
            Debug.Log($"[{slotNumber}번 슬롯] 첫 저장을 시작합니다.");
            ExecuteSave(slotNumber, filePath);
            return;
        }

        if (isSaveMode)
        {
            File.Delete(filePath);
            ExecuteSave(slotNumber, filePath);
        }
        else
        {
            ExecuteLoad(slotNumber, filePath);
        }
    }

    // 실제 저장 처리
    private void ExecuteSave(int slotNumber, string filePath)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("🚨 [저장 실패] 씬에서 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
            return;
        }

        playerData.sceneName = SceneManager.GetActiveScene().name;

        if (playerData.sceneName.StartsWith("Level_"))
        {
            int.TryParse(playerData.sceneName.Replace("Level_", ""), out playerData.stage);
        }

        playerData.posX = player.transform.position.x;
        playerData.posY = player.transform.position.y;

        // ★ [동기화] 현재 게임 속 사망 카운트를 JSON 데이터에 기록
        playerData.deathCount = BattleTransferData.deathCount;

        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (stats != null) playerData.currentHealth = stats.currentHealth;

        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"💾 슬롯 {slotNumber} 저장 완료! (누적 사망: {playerData.deathCount}회)");
    }

    // 실제 로드 처리 (타이틀 화면 등에서 일반 로드할 때)
    private void ExecuteLoad(int slotNumber, string filePath)
    {
        string json = File.ReadAllText(filePath);
        playerData = JsonUtility.FromJson<PlayerData>(json);

        if (string.IsNullOrEmpty(playerData.sceneName))
        {
            playerData.sceneName = "Level_" + playerData.stage;
        }

        // ★ [동기화] 불러온 파일의 사망 카운트를 전역 시스템에 적용
        BattleTransferData.deathCount = playerData.deathCount;

        Debug.Log($"📂 슬롯 {slotNumber} 로드 성공! 사망 횟수({BattleTransferData.deathCount}회)가 적용되었습니다.");

        targetPosition = new Vector3(playerData.posX, playerData.posY, 0);
        targetHealth = playerData.currentHealth;
        pendingLoad = true;

        SceneManager.LoadScene(playerData.sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingLoad)
        {
            pendingLoad = false;
            StartCoroutine(RestorePlayerStateRoutine());
        }
    }

    private IEnumerator RestorePlayerStateRoutine()
    {
        yield return null; 

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.position = targetPosition;
            player.transform.position = targetPosition;

            CharacterStats stats = player.GetComponent<CharacterStats>();
            if (stats != null)
            {
                stats.currentHealth = targetHealth;
                stats.UpdateStats();
                FindFirstObjectByType<PlayerStatsUI>()?.UpdateStatsUI();
            }
            Debug.Log("✅ 플레이어 위치 및 체력 복구 완료!");
        }
    }

    // ────────────────────────────────────────────────────────
    // 💀 [신규 핵심 함수] 게임 오버 씬에서 '재시작'할 때 호출하는 함수
    // ────────────────────────────────────────────────────────
    public void RestartFromGameOver(int slotNumber = 1)
    {
        string filePath = Application.persistentDataPath + $"/player_data_slot_{slotNumber}.json";

        // 1. 기존 세이브 데이터가 있다면 아이템 정보 등을 복구하기 위해 읽어옴
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            playerData = JsonUtility.FromJson<PlayerData>(json);
        }
        else
        {
            // 세이브 파일이 아예 없던 상태라면 새 데이터 선언
            playerData = new PlayerData();
        }

        // 2. 세이브 데이터에 기록되어 있던 기본 사망 횟수에 +1 누적 (최대 3회 제한)
        BattleTransferData.deathCount = playerData.deathCount + 1;
        if (BattleTransferData.deathCount > 3) BattleTransferData.deathCount = 3;

        // 증가한 사망 카운트를 세이브 데이터 구조체에도 반영
        playerData.deathCount = BattleTransferData.deathCount;

        // 3. 목적지 맵을 무조건 'Level_1'로 고정 변경 및 부활 체력(-1은 풀피) 세팅
        playerData.sceneName = "Level_1";
        playerData.stage = 1;
        BattleTransferData.playerCurrentHealth = -1; 

        // Level_1의 지정된 기본 스폰 좌표에서 시작하도록 위치 복구 플래그는 꺼둠
        pendingLoad = false; 

        // 4. 사망 횟수가 늘어난 따끈따끈한 상태를 세이브 파일에 즉시 덮어쓰기 저장
        string updatedJson = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, updatedJson);

        Debug.Log($"♻️ 게임 오버 복구 완료: 누적 사망 [{BattleTransferData.deathCount}/3] 회를 안고 Level_1에서 재시작합니다.");
        
        // 5. Level_1 씬 로드
        SceneManager.LoadScene("Level_1");
    }

    public void SaveData(PlayerData data)
    {
        playerData = data;
        string filePath = Application.persistentDataPath + "/player_data_slot_1.json";
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, json);
    }

    public void PlayerDead()
    {
        // 플레이어 캐릭터 사망 시 호출되어 GameOver 씬으로 이동시킵니다.
        SceneManager.LoadScene("GameOver");
    }

    public void SetSaveMode(bool saveMode)
    {
        isSaveMode = saveMode;
    }
}