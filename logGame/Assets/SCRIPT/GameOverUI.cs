using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    // 게임 오버 화면의 '다시 시작(Retry)' 버튼에 이 함수를 연결하세요.
    public void OnClickRetry()
    {
        if (GameDataManager.instance != null)
        {
            // 기본 1번 세이브 슬롯을 기반으로 사망 처리를 연동해 Level_1로 보냅니다.
            GameDataManager.instance.RestartFromGameOver(1);
        }
    }
}