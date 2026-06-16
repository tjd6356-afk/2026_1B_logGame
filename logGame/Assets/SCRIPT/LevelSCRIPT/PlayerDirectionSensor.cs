using UnityEngine;
using System.Reflection;

// 이 스크립트는 플레이어 오브젝트에 부착됩니다.
[RequireComponent(typeof(Rigidbody2D))] // Rigidbody2D 컴포넌트가 필수입니다.
public class PlayerDirectionSensor : MonoBehaviour
{
    private PlayerController playerController;
    private FieldInfo inputFieldInfo; // PlayerController 내부의 'input' 필드 정보를 저장
    private Vector2 lastInputDirection = Vector2.down; // 게임 시작 시 기본 방향 (아래)

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        // 중요: 사용자가 제공한 PlayerController.cs 내부의 'private Vector2 input;' 필드를 찾아옵니다.
        // 이 방식은 원본 코드를 한 줄도 수정하지 않고 데이터를 가져올 수 있게 해줍니다.
        inputFieldInfo = typeof(PlayerController).GetField("input", BindingFlags.NonPublic | BindingFlags.Instance);

        if (inputFieldInfo == null)
        {
            Debug.LogError("[PlayerInputDirectionSensor] PlayerController 내부에서 'input' 변수를 찾을 수 없습니다. 원본 코드의 변수 이름이 'input'인지 확인하세요.");
        }
    }

    private void Update() // FixedUpdate보다 Update가 키 입력 반응에 더 빠릅니다.
    {
        if (playerController == null || inputFieldInfo == null) return;

        // PlayerController의 현재 private input 값을 가져옵니다.
        Vector2 currentInput = (Vector2)inputFieldInfo.GetValue(playerController);

        // 입력이 아주 작지 않을 때만 (즉, 키를 눌렀을 때만) 방향을 업데이트합니다.
        if (currentInput.sqrMagnitude > 0.01f)
        {
            // 상하 방향 입력이 좌우 방향 입력보다 크다면 상하 방향으로, 아니면 좌우 방향으로 고정합니다.
            // 이는 스냅(즉시) 회전을 구현하는 핵심 로직입니다.
            if (Mathf.Abs(currentInput.y) > Mathf.Abs(currentInput.x))
            {
                lastInputDirection = (currentInput.y > 0) ? Vector2.up : Vector2.down;
            }
            else
            {
                lastInputDirection = (currentInput.x > 0) ? Vector2.right : Vector2.left;
            }
        }
    }

    // 손전등 회전 스크립트에서 플레이어가 '입력한' 최종 방향을 스냅으로 가져갑니다.
    public Vector2 GetLookDirection()
    {
        return lastInputDirection;
    }
}