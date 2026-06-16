using System.Collections;
using UnityEngine;

public class TimedBarrierTrigger : MonoBehaviour
{
    [Header("⚙️ 대상 오브젝트 설정")]
    [SerializeField] private GameObject barrier; // 방해물 (boundary_2)을 연결합니다.

    [Header("⏳ 시간 설정 (초 단위)")]
    [SerializeField] private float delayTime = 3.0f; // 나왔을 때부터 벽이 생기기까지의 시간

    private bool isPlayerInside = false;
    private Coroutine barrierCoroutine;

    private void Start()
    {
        // 게임 시작 시 방해물(벽)이 이미 켜져 있다면, 처음에는 통과할 수 있게 꺼둡니다.
        if (barrier != null)
        {
            barrier.SetActive(false);
        }
    }

    // 1. 플레이어가 트리거 범위 안으로 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;
            Debug.Log("👣 플레이어가 트리거를 밟았습니다.");

            // 만약 작동 중이던 타이머가 있다면 초기화 (재진입 시 리셋 기능)
            if (barrierCoroutine != null)
            {
                StopCoroutine(barrierCoroutine);
                barrierCoroutine = null;
            }
        }
    }

    // 2. 플레이어가 트리거 범위 밖으로 나갔을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;
            Debug.Log($"🏃 플레이어가 트리거에서 나갔습니다. {delayTime}초 후 벽이 생성됩니다.");

            // 플레이어가 나간 순간부터 시간 카운트다운을 시작합니다.
            barrierCoroutine = StartCoroutine(ActivateBarrierRoutine());
        }
    }

    // ⏳ 지정된 시간만큼 대기 후 벽을 활성화하는 코루틴
    private IEnumerator ActivateBarrierRoutine()
    {
        // 지정한 시간(delayTime)만큼 초단위로 대기
        yield return new WaitForSeconds(delayTime);

        if (barrier != null)
        {
            barrier.SetActive(true);
            Debug.Log("🚧 boundary_2 (방해물)가 생성되어 통행이 차단되었습니다!");
        }
    }
}