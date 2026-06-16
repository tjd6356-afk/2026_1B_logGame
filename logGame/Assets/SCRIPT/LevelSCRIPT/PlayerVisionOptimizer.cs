using UnityEngine;
using System.Collections.Generic;

public class PlayerVisionOptimizer : MonoBehaviour
{
    [Header("─ 플레이어 시야 최적화 설정")]
    public Transform flashlightTransform; // 플레이어의 FlashLight 오브젝트
    public float viewDistance = 6.0f;       // 플레이어 손전등 빛이 닿는 최장 거리
    [Range(0, 360)] public float viewAngle = 90f; // 플레이어 손전등 시야 각도 (부채꼴)

    [Header("─ 레이어 마스크")]
    public LayerMask obstacleLayer;       // 시야를 가로막는 벽(wall, boundary 등) 레이어

    private List<EnemyAI> allEnemies = new List<EnemyAI>();
    private float updateInterval = 0.3f;  // 0.3초마다 씬의 전체 적 목록을 갱신 (성능 최적화)
    private float timer;

    void Start()
    {
        FindAllEnemiesInScene();
    }

    void Update()
    {
        // 성능을 위해 매 프레임 Find를 쓰지 않고, 주기적으로 적 목록을 관리합니다.
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            FindAllEnemiesInScene();
        }

        // 매 프레임 시야 체크를 통해 적들의 Ray 기능을 On/Off 합니다.
        OptimizeEnemies();
    }

    // 씬에 있는 모든 적들을 찾아 리스트에 수집하는 함수 (Unity 6 최신 표준 함수 사용)
    void FindAllEnemiesInScene()
    {
        allEnemies.Clear();
        EnemyAI[] enemies = FindObjectsByType<EnemyAI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        allEnemies.AddRange(enemies);
    }

    void OptimizeEnemies()
    {
        if (flashlightTransform == null) return;

        // 플레이어 위치 및 손전등이 실시간으로 비추고 있는 전방 방향 벡터 구하기
        Vector2 playerPos = transform.position;
        Vector2 forwardDir = flashlightTransform.up; // ※ 만약 손전등 회전 기준이 우측이면 right로 변경 가능

        foreach (EnemyAI enemy in allEnemies)
        {
            if (enemy == null) continue;

            Vector2 enemyPos = enemy.transform.position;
            Vector2 dirToEnemy = enemyPos - playerPos;
            float distanceToEnemy = dirToEnemy.magnitude;

            bool isInsidePlayerVision = false;

            // [1차 필터] 플레이어의 시야 '거리' 안에 들어왔는가?
            if (distanceToEnemy <= viewDistance)
            {
                dirToEnemy.Normalize();

                // [2차 필터] 플레이어가 바라보는 시야 '각도(부채꼴)' 안에 들어왔는가?
                float angleToEnemy = Vector2.Angle(forwardDir, dirToEnemy);

                if (angleToEnemy <= viewAngle / 2f)
                {
                    // [3차 필터] 플레이어와 적 사이에 '벽'이 가로막고 있진 않은가? (오클루전 레이캐스트)
                    // 플레이어 몸에서 해당 적을 향해 딱 1개의 확인용 레이만 쏩니다.
                    RaycastHit2D hit = Physics2D.Raycast(playerPos, dirToEnemy, distanceToEnemy, obstacleLayer);

                    if (hit.collider == null)
                    {
                        // 모든 조건을 통과하면 플레이어 시야에 적이 노출된 상태입니다!
                        isInsidePlayerVision = true;
                    }
                }
            }

            //  [핵심] 플레이어 시야 상태에 따른 EnemyAI 활성화 / 비활성화 스위칭
            if (isInsidePlayerVision)
            {
                // 플레이어 시야에 들어오면 EnemyAI를 켜서 자기가 직접 Ray를 쏘고 추적하게 만듬
                if (!enemy.enabled)
                {
                    enemy.enabled = true;
                }
            }
            else
            {
                // 플레이어 시야에서 벗어나면 EnemyAI 스크립트를 통째로 꺼서 내부 Ray 연산을 0%로 만듬
                if (enemy.enabled)
                {
                    enemy.enabled = false;

                    // 스크립트가 꺼질 때 물리 관성에 의해 미끄러지는 현상 방지 (Unity 6 공식 규격)
                    Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        enemyRb.linearVelocity = Vector2.zero;
                    }
                }
            }
        }
    }

    // 에디터 뷰에서 플레이어의 시야 범위를 하늘색 실선 부채꼴로 시각화해주는 기즈모 함수
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 pos = transform.position;
        Gizmos.DrawWireSphere(pos, viewDistance);

        if (flashlightTransform != null)
        {
            Vector2 forwardDir = flashlightTransform.up;
            Vector3 leftBoundary = RotateVector(forwardDir, viewAngle / 2f) * viewDistance;
            Vector3 rightBoundary = RotateVector(forwardDir, -viewAngle / 2f) * viewDistance;

            Gizmos.DrawLine(pos, pos + leftBoundary);
            Gizmos.DrawLine(pos, pos + rightBoundary);
        }
    }

    Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
        return new Vector2(cos * vector.x - sin * vector.y, sin * vector.x + cos * vector.y);
    }
}