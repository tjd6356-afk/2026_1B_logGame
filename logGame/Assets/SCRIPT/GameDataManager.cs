using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// ────────────────────────────────────────────────────────
// 📦 데이터 저장 구조체 (문자열 기반 맵 저장 추가)
// ────────────────────────────────────────────────────────
[Serializable]
public class PlayerData
{
    public List<string> collectedItems = new List<string>();
    public int stage = 1;           // 구버전 호환용 (지우지 마세요)
    public string equippedItem = "";

    // ★ 맵 이름 자체를 저장하는 변수 추가 (버그 원천 차단)
    public string sceneName = "";
    public float posX;
    public float posY;
    public int currentHealth;
}

// ────────────────────────────────────────────────────────
// 🎮 게임 데이터 매니저 (싱글톤 최종판)
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

        // ★ 현재 활성화된 씬의 이름을 있는 그대로 완벽하게 저장합니다.
        playerData.sceneName = SceneManager.GetActiveScene().name;

        // 구버전 호환용 stage 숫자 추출도 안전하게 유지
        if (playerData.sceneName.StartsWith("Level_"))
        {
            int.TryParse(playerData.sceneName.Replace("Level_", ""), out playerData.stage);
        }

        playerData.posX = player.transform.position.x;
        playerData.posY = player.transform.position.y;

        CharacterStats stats = player.GetComponent<CharacterStats>();
        if (stats != null) playerData.currentHealth = stats.currentHealth;

        // JSON 저장
        string json = JsonUtility.ToJson(playerData, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"💾 슬롯 {slotNumber} 저장 완료! 맵:[{playerData.sceneName}], 위치:({playerData.posX}, {playerData.posY})");
    }

    // 실제 로드 처리
    private void ExecuteLoad(int slotNumber, string filePath)
    {
        string json = File.ReadAllText(filePath);
        playerData = JsonUtility.FromJson<PlayerData>(json);

        // 예외 처리: 옛날 세이브 파일이라 sceneName이 비어있다면 구버전 숫자로 보완
        if (string.IsNullOrEmpty(playerData.sceneName))
        {
            playerData.sceneName = "Level_" + playerData.stage;
        }

        Debug.Log($"📂 슬롯 {slotNumber} 로드 성공! [{playerData.sceneName}] 맵으로 이동합니다.");

        targetPosition = new Vector3(playerData.posX, playerData.posY, 0);
        targetHealth = playerData.currentHealth;
        pendingLoad = true;

        // 이제 숫자가 아닌 진짜 저장된 맵 이름으로 안전하게 로드합니다.
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
        yield return null; // 씬 안정화 대기

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
        else
        {
            Debug.LogWarning("⚠️ [주의] 로드 후 플레이어 오브젝트를 찾지 못해 위치 복구를 건너뜁니다.");
        }
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
        playerData.sceneName = "SampleScene"; // 사망 시 기본 맵으로 초기화
        playerData.stage = 1;
        SceneManager.LoadScene("GameOver");
    }

    public void SetSaveMode(bool saveMode)
    {
        isSaveMode = saveMode;
    }
}