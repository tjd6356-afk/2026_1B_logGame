using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class TitleMenuManager : MonoBehaviour
{
    [Header("📋 UI 패널 연결")]
    [SerializeField] private GameObject saveLoadPanel;       // 파일 1, 2, 3 슬롯이 있는 패널
    [SerializeField] private GameObject exitConfirmationPanel; // 게임 종료 확인 팝업 패널

    private void Start()
    {
        // 게임 시작 시 모든 팝업 패널은 닫아둡니다.
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
        if (exitConfirmationPanel != null) exitConfirmationPanel.SetActive(false);
    }

    // ────────────────────────────────────────────────────────
    // 🎮 1. NEW GAME 버튼 (처음부터 시작)
    // ────────────────────────────────────────────────────────
    public void OnClickNewGame()
    {
        Debug.Log("🆕 새로운 게임을 시작합니다. 기존 세이브 데이터를 전부 삭제합니다.");

        // 1번부터 5번 슬롯까지 저장된 JSON 데이터 파일이 있다면 전부 삭제
        for (int i = 1; i <= 5; i++)
        {
            string filePath = Application.persistentDataPath + $"/player_data_slot_{i}.json";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"🗑️ 슬롯 {i} 세이브 파일 삭제 완료.");
            }
        }

        // 싱글톤 매니저가 살아있다면 플레이어 데이터 구조체도 완전 초기화
        if (GameDataManager.instance != null)
        {
            GameDataManager.instance.playerData = new PlayerData();
        }

        // 첫 번째 스테이지인 Level_1으로 진입
        SceneManager.LoadScene("Level_1");
    }

    // ────────────────────────────────────────────────────────
    // 📂 2. LOAD GAME 버튼 (이어서 하기)
    // ────────────────────────────────────────────────────────
    public void OnClickLoadGame()
    {
        if (saveLoadPanel != null)
        {
            // 기존에 만들어 둔 세이브/로드 슬롯 UI 창을 화면에 띄웁니다.
            saveLoadPanel.SetActive(true);

            // ★ 핵심: 타이틀에서 켜는 창은 '불러오기' 목적이므로 모드를 false(로드 모드)로 강제 세팅합니다.
            if (GameDataManager.instance != null)
            {
                GameDataManager.instance.SetSaveMode(false);
                Debug.Log("📂 불러오기(LOAD) 모드로 슬롯 창을 활성화했습니다.");
            }
        }
        else
        {
            Debug.LogError("🚨 세이브/로드 패널(SaveLoadPanel)이 스크립트에 연결되지 않았습니다!");
        }
    }

    // ────────────────────────────────────────────────────────
    // 🚪 3. GAME EXIT 버튼 및 팝업 제어 (게임 종료)
    // ────────────────────────────────────────────────────────
    
    // [종료] 버튼을 눌렀을 때 팝업창을 띄우는 함수
    public void OnClickExitButton()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(true);
            Debug.Log("❓ 게임 종료 확인 패널 오픈");
        }
    }

    // 팝업창 내부 -> [예 (종료하겠다)] 버튼 누를 때
    public void OnClickExitConfirm()
    {
        Debug.Log("🚪 게임을 안전하게 종료합니다.");
        
        #if UNITY_EDITOR
        // 유니티 에디터 환경에서 작동할 때 플레이 모드 끄기
        UnityEditor.EditorApplication.isPlaying = false; 
        #else
        // 실제 빌드된 PC/모바일 게임 환경에서 종료하기
        Application.Quit(); 
        #endif
    }

    // 팝업창 내부 -> [아니오 (종료 안하겠다)] 버튼 누를 때
    public void OnClickExitCancel()
    {
        if (exitConfirmationPanel != null)
        {
            exitConfirmationPanel.SetActive(false);
            Debug.Log("❌ 게임 종료를 취소하고 패널을 닫습니다.");
        }
    }
}