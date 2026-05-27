using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; //  New Input System을 사용하기 위해 필수!

public class PlayerInteraction : MonoBehaviour
{
    [Header("상호작용 설정")]
    public string targetTag = "boundary"; // 감지할 장애물의 태그
    public float removalTime = 2.0f;       // 장애물 제거에 걸리는 시간 (초)

    private GameObject currentObstacle = null; // 현재 범위 내에 있는 장애물
    private Coroutine removeCoroutine = null;   // 진행 중인 제거 코루틴 저장용
    private bool isRemoving = false;            // 현재 제거 중인지 확인하는 플래그

    void Update()
    {
        if (Time.timeScale == 0f) return; // 게임이 일시정지 중이면 키 입력을 무시함
        
        //  New Input System 방식으로 E 키가 이번 프레임에 눌렸는지 체크
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            // 상호작용 가능한 장애물이 있고, 이미 제거 중이 아닐 때만 시작
            if (currentObstacle != null && !isRemoving)
            {
                removeCoroutine = StartCoroutine(RemoveObstacleRoutine(currentObstacle));
            }
        }
    }

    //  일정 시간 동안 대기 후 장애물을 삭제하는 코루틴
    private IEnumerator RemoveObstacleRoutine(GameObject obstacle)
    {
        isRemoving = true;
        Debug.Log($"[상호작용] {obstacle.name} 제거 시작... ({removalTime}초 소요)");

        // 설정한 시간(seconds)만큼 대기합니다.
        yield return new WaitForSeconds(removalTime);

        // 대기 시간이 끝난 후 장애물이 아직 존재한다면 파괴!
        if (obstacle != null)
        {
            Debug.Log($"[상호작용] {obstacle.name} 제거 완료!");
            Destroy(obstacle);
        }

        // 상태 초기화
        isRemoving = false;
        currentObstacle = null;
        removeCoroutine = null;
    }

    //  플레이어가 장애물 범위(Trigger)에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag))
        {
            currentObstacle = collision.gameObject;
            Debug.Log($"[상호작용 가능] {currentObstacle.name}에 접근함. (E 키를 누르세요)");
        }
    }

    //  플레이어가 장애물 범위(Trigger)를 벗어났을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(targetTag) && collision.gameObject == currentObstacle)
        {
            // 만약 제거하는 도중에 도망쳤다면 작업을 취소합니다.
            if (isRemoving && removeCoroutine != null)
            {
                StopCoroutine(removeCoroutine);
                isRemoving = false;
                removeCoroutine = null;
                Debug.Log("[상호작용 취소] 장애물과 멀어져서 제거가 취소되었습니다.");
            }

            currentObstacle = null;
        }
    }
}