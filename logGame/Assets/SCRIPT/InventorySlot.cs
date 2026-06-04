using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image iconImage;       // 슬롯 안에 있는 자식 Image 컴포넌트
    public Button slotButton;     // 슬롯 자체의 Button 컴포넌트

    private ItemData currentItem;
    private InventoryManager manager;

    // 매니저가 슬롯을 초기화할 때 호출
    public void Init(InventoryManager inventoryManager)
    {
        manager = inventoryManager;
        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(OnSlotClicked);
        }
        ClearSlot();
    }

    // 슬롯에 아이템 채우기
    public void SetItem(ItemData newItem)
    {
        currentItem = newItem;
        iconImage.sprite = newItem.icon;
        iconImage.gameObject.SetActive(true);
    }

    // 슬롯 비우기
    public void ClearSlot()
    {
        currentItem = null;
        iconImage.sprite = null;
        iconImage.gameObject.SetActive(false);
    }

    // 슬롯 클릭 시 매니저에게 정보 전달 (기능 3번 연동)
    private void OnSlotClicked()
    {
        if (currentItem != null && manager != null)
        {
            manager.SelectItem(currentItem);
        }
    }
}