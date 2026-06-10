using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [Header("UI 설정")]
    public GameObject inventoryPanel; 

    [Header("탭 내용물 패널 (Content)")]
    public GameObject[] contentPanels; 

    [Header("★ 인벤토리 시스템 추가 설정")]
    public Transform slotParent;          // 슬롯들의 부모 오브젝트 (Grid Layout Group이 있는 곳)
    public List<ItemData> itemDatabase;   // 프로젝트에 존재하는 모든 아이템 리스트 (데이터베이스)
    
    [Header("★ 아이템 정보창 UI")]
    public TextMeshProUGUI itemNameText;             // 아이템 이름 텍스트
    public TextMeshProUGUI itemDescText;             // 아이템 설명 텍스트
    public Image infoItemIcon;                      //정보창에 보일 큰 아이템 이미지 칸
    public Button equipButton;            // 장착 버튼
    public Button unequipButton;          // 장착 해제 버튼

    public static bool isInventoryOpen { get; private set; } = false;
    
    private InventorySlot[] slots;        // 부모 밑에 있는 슬롯 배열
    private ItemData selectedItem;        // 현재 플레이어가 클릭해 선택한 아이템

    private void Awake()
    {
        // 부모(slotParent) 아래에 있는 모든 InventorySlot을 자동으로 찾아와 초기화합니다.
        slots = slotParent.GetComponentsInChildren<InventorySlot>(true);
        foreach (var slot in slots)
        {
            slot.Init(this);
        }
    }

    private void Start()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        Time.timeScale = 1f;
        isInventoryOpen = false;

        // 장착/해제 버튼에 함수 연결
        if (equipButton != null) equipButton.onClick.AddListener(EquipSelectedItem);
        if (unequipButton != null) unequipButton.onClick.AddListener(UnequipSelectedItem);

        ClearInfoPanel();
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

            if (isInventoryOpen)
            {
                if (contentPanels.Length > 0)
                {
                    SwitchTab(contentPanels[0]);
                }
                
                // ★ 인벤토리가 열릴 때 화면을 갱신하고 정보창을 초기화합니다.
                RefreshInventoryUI();
                ClearInfoPanel();
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

    public void SwitchTab(GameObject panelToOpen)
    {
        foreach (GameObject panel in contentPanels)
        {
            if (panel != null) panel.SetActive(false);
        }

        if (panelToOpen != null) panelToOpen.SetActive(true);
    }

    // ────────────────────────────────────────────────────────
    // ★ [기능 1] 인벤토리 아이템 동기화 및 슬롯 표시
    // ────────────────────────────────────────────────────────
    public void RefreshInventoryUI()
    {
        // 1. 모든 슬롯을 먼저 깨끗하게 비웁니다.
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }

        // 2. 세이브 데이터에서 먹은 아이템 이름 리스트를 가져옵니다.
        List<string> collectedNames = GameDataManager.instance.playerData.collectedItems;
        int slotIndex = 0;

        // 3. 먹은 아이템 이름들을 데이터베이스(SO)와 매칭하여 슬롯에 채웁니다.
        foreach (string itemName in collectedNames)
        {
            if (slotIndex >= slots.Length) break; // 슬롯 개수를 초과하면 중단

            // 데이터베이스에서 일치하는 식별명을 가진 ItemData를 찾음
            ItemData data = itemDatabase.Find(x => x.itemName == itemName);
            if (data != null)
            {
                slots[slotIndex].SetItem(data);
                slotIndex++;
            }
        }
    }

    // ────────────────────────────────────────────────────────
    // ★ [기능 3] 아이템 선택 시 정보 확인 기능
    // ────────────────────────────────────────────────────────
    public void SelectItem(ItemData item)
    {
        selectedItem = item;
        itemNameText.text = item.displayName;
        itemDescText.text = item.description;

        if (infoItemIcon != null)
        {
            infoItemIcon.sprite = item.icon;
            infoItemIcon.gameObject.SetActive(true);
        }

        UpdateActionButtons();
    }

    // 정보창 초기 상태 (아무것도 선택 안 됨)
    private void ClearInfoPanel()
    {
        selectedItem = null;
        if (itemNameText != null) itemNameText.text = "아이템을 선택하세요.";
        if (itemDescText != null) itemDescText.text = "";
        if (infoItemIcon != null) infoItemIcon.gameObject.SetActive(false);
        if (equipButton != null) equipButton.gameObject.SetActive(false);
        if (unequipButton != null) unequipButton.gameObject.SetActive(false);
    }

    // 현재 장착 여부에 따라 장착/해제 버튼을 스위칭 노출
    private void UpdateActionButtons()
    {
        if (selectedItem == null) return;

        string equippedName = GameDataManager.instance.playerData.equippedItem;

        // 현재 선택한 아이템이 이미 장착된 아이템이라면? -> '장착 해제' 버튼만 보여줌
        if (equippedName == selectedItem.itemName)
        {
            equipButton.gameObject.SetActive(false);
            unequipButton.gameObject.SetActive(true);
        }
        else // 장착 안 된 아이템이라면? -> '장착' 버튼만 보여줌
        {
            equipButton.gameObject.SetActive(true);
            unequipButton.gameObject.SetActive(false);
        }
    }

    // ────────────────────────────────────────────────────────
    // ★ [기능 2] 장착 및 장착 해제 기능
    // ────────────────────────────────────────────────────────
    public void EquipSelectedItem()
    {
        if (selectedItem == null) return;

        GameDataManager.instance.playerData.equippedItem = selectedItem.itemName;
        GameDataManager.instance.SaveData(GameDataManager.instance.playerData);
        
        Debug.Log($"[인벤토리] {selectedItem.displayName} 장착 완료!");
        
        // ★ 추가: 장착 후 플레이어 스탯 실시간 업데이트 호출
        GameObject.FindGameObjectWithTag("Player")?.GetComponent<CharacterStats>()?.UpdateStats();

        UpdateActionButtons();
    }

    public void UnequipSelectedItem()
    {
        if (selectedItem == null) return;

        GameDataManager.instance.playerData.equippedItem = "";
        GameDataManager.instance.SaveData(GameDataManager.instance.playerData);

        Debug.Log($"[인벤토리] 장착 해제 완료!");
        
        // ★ 추가: 해제 후 플레이어 스탯 실시간 업데이트 호출
        GameObject.FindGameObjectWithTag("Player")?.GetComponent<CharacterStats>()?.UpdateStats();

        UpdateActionButtons();
    }
}