using UnityEngine;
using UnityEngine.InputSystem; 

public class InventoryManager : MonoBehaviour
{
    [Header("UI 설정")]
    public GameObject inventoryPanel; 

    // ▼▼▼ 새로 추가된 부분: 탭으로 사용할 내용물 패널들을 담을 배열 ▼▼▼
    [Header("탭 내용물 패널 (Content)")]
    public GameObject[] contentPanels; 

    public static bool isInventoryOpen { get; private set; } = false;

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        Time.timeScale = 1f;
        isInventoryOpen = false;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);

            // ▼▼▼ 추가: 인벤토리를 열 때 무조건 첫 번째 탭(인벤토리 내용)을 보여주도록 설정 ▼▼▼
            if (isInventoryOpen && contentPanels.Length > 0)
            {
                SwitchTab(contentPanels[0]);
            }
        }

        if (isInventoryOpen)
        {
            Time.timeScale = 0f; 
            Debug.Log("[인벤토리] 열림 - 게임 일시정지");
        }
        else
        {
            Time.timeScale = 1f; 
            Debug.Log("[인벤토리] 닫힘 - 게임 재개");
        }
    }

    // ▼▼▼ 새롭게 추가된 기능: 탭 전환 함수 ▼▼▼
    // 유니티 UI 버튼의 OnClick 이벤트에서 이 함수를 호출할 것입니다.
    public void SwitchTab(GameObject panelToOpen)
    {
        // 1. 모든 패널을 일단 끕니다.
        foreach (GameObject panel in contentPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        // 2. 버튼이 눌러서 전달받은 특정 패널만 켭니다.
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
    }
}