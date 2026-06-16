using UnityEngine;

// 이 스크립트는 손전등 빛 컴포넌트가 있는 오브젝트에 부착됩니다.
public class FlashLightRotationController : MonoBehaviour
{
    [Header("플레이어 센서 스크립트 연결")]
    [SerializeField] private PlayerDirectionSensor inputSensor; // Player 오브젝트에 부착된 PlayerInputDirectionSensor 스크립트를 연결합니다.

    private void Update()
    {
        // 인스펙터에서 센서 스크립트가 연결되지 않았다면 작동하지 않습니다.
        if (inputSensor == null) return;

        // 플레이어가 입력한 스냅 방향을 센서로부터 즉시 가져옵니다.
        Vector2 lookDirection = inputSensor.GetLookDirection();

        // 방향에 따른 회전 각도를 스냅으로 계산합니다. (Z축 회전)
        float zAngle = 0f;

        if (lookDirection == Vector2.up) zAngle = 0f;        // 위 (기본 각도)
        else if (lookDirection == Vector2.down) zAngle = 180f;  // 아래
        else if (lookDirection == Vector2.left) zAngle = 90f;   // 왼쪽
        else if (lookDirection == Vector2.right) zAngle = -90f; // 오른쪽

        // 손전등 오브젝트의 회전 값을 업데이트합니다. (X, Y는 고정, Z축만 회전)
        transform.rotation = Quaternion.Euler(0, 0, zAngle);
    }
}