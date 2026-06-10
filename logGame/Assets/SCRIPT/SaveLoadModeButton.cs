using UnityEngine;
using TMPro;

public class SaveLoadModeButton : MonoBehaviour
{
    private TextMeshProUGUI buttonText;

    private void Awake()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        // 게임이 시작되거나 UI가 처음 켜질 때 텍스트를 즉시 동기화합니다.
        UpdateToggleText();
    }

    private void OnEnable()
    {
        UpdateToggleText();
    }

    // 🔄 UI 버튼에는 이 함수 딱 하나만 연결되어 있어야 합니다!
    public void ToggleSaveLoadMode()
    {
        if (GameDataManager.instance == null) return;

        // 현재 모드를 반대로 전환 (!true -> false / !false -> true)
        bool nextMode = !GameDataManager.instance.isSaveMode;
        GameDataManager.instance.SetSaveMode(nextMode);

        // 바뀐 모드에 맞춰 즉시 텍스트 새로고침
        UpdateToggleText();
    }

    private void UpdateToggleText()
    {
        if (GameDataManager.instance == null || buttonText == null) return;

        if (GameDataManager.instance.isSaveMode)
        {
            buttonText.text = "현재: [저장하기]";
        }
        else
        {
            buttonText.text = "현재: [불러오기]";
        }
    }
}