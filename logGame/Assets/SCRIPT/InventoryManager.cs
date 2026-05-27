using UnityEngine;
using UnityEngine.InputSystem; // New Input System 사용

public class InventoryManager : MonoBehaviour
{
    [Header("UI 설정")]
    public GameObject inventoryPanel; // 인벤토리 패널 UI를 여기에 연결하세요.

    // 다른 스크립트에서 인벤토리 상태를 읽을 수 있도록 프로퍼티 생성
    public static bool isInventoryOpen { get; private set; } = false;

    private void Start()
    {
        // 시작할 때 인벤토리가 켜져 있다면 끄고, 게임 속도를 정상화합니다.
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        Time.timeScale = 1f;
        isInventoryOpen = false;
    }

    private void Update()
    {
        // New Input System: 이번 프레임에 Tab 키가 눌렸는지 확인
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        // 인벤토리 패널 활성화/비활성화
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }

        // 인벤토리 상태에 따른 게임 일시정지 / 재개
        if (isInventoryOpen)
        {
            Time.timeScale = 0f; // 게임 멈춤
            Debug.Log("[인벤토리] 열림 - 게임 일시정지");
        }
        else
        {
            Time.timeScale = 1f; // 게임 재개
            Debug.Log("[인벤토리] 닫힘 - 게임 재개");
        }
    }
}