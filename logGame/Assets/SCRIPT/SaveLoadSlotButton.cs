using UnityEngine;

public class SaveLoadSlotButton : MonoBehaviour
{
    [Header("⚙️ 슬롯 번호 설정")]
    [Tooltip("이 버튼이 담당할 슬롯 번호를 적어주세요 (1, 2, 3 등)")]
    public int slotNumber = 1;

    // 💾 유저가 이 파일 슬롯 버튼을 눌렀을 때 실행될 통합 함수
    public void ClickSlot()
    {
        if (GameDataManager.instance != null)
        {
            // 실시간으로 살아있는 싱글톤 인스턴스를 찾아 안전하게 명령을 전달합니다.
            GameDataManager.instance.OnClickFileSlot(slotNumber);
        }
        else
        {
            Debug.LogError("🚨 [에러] 게임 데이터 매니저(GameDataManager)를 찾을 수 없습니다!");
        }
    }
}