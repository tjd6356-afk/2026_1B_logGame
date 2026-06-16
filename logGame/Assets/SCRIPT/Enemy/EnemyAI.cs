using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrol, Chase }

    [Header("─ 상태 설정")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("─ 이동 속도")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.0f;
    public float directionChangeInterval = 2.0f; // 랜덤 방향 전환 주기 (초)

    [Header("─ 시야 센서 (Raycast)")]
    public float viewDistance = 4.0f;       // 레이가 뻗어나갈 거리
    [Range(0, 360)] public float viewAngle = 90f; // 시야 각도 (부채꼴 범위)
    public int rayCount = 7;                // 부채꼴 내부에 쏠 레이의 개수

    [Header("─ 레이어 마스크 설정")]
    public LayerMask playerLayer;           // 감지할 플레이어 레이어
    public LayerMask obstacleLayer;         // 시야를 가로막을 벽/장애물 레이어

    private Rigidbody2D rb;
    private Transform playerTransform;
    private Vector2 movementDirection;
    private Vector2 lastFacingDirection = Vector2.down; // 기본 바라보는 방향
    private float patrolTimer;
    private bool isPlayerDetected = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 2D 탑다운 게임이므로 물리 회전으로 인해 굴러다니지 않게 고정합니다.
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f; // 중력 차단
        }

        PickRandomDirection();
    }

    void Update()
    {
        // 매 프레임 여러 갈래의 레이를 쏘아 플레이어를 찾습니다.
        CheckVisionCone();

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolLogic();
                break;
            case EnemyState.Chase:
                ChaseLogic();
                break;
        }
    }

    void FixedUpdate()
    {
        if (rb == null) return;

        // 현재 상태에 맞는 속도를 적용하여 이동 처리
        float currentSpeed = (currentState == EnemyState.Chase) ? chaseSpeed : patrolSpeed;

        // ★ Unity 6 신규 표준인 linearVelocity를 사용합니다. (기존 velocity 대용)
        rb.linearVelocity = movementDirection * currentSpeed;
    }

    // ────────────────────────────────────────────────────────
    // 다중 레이캐스트 (시야 범위 체크)
    // ────────────────────────────────────────────────────────
    void CheckVisionCone()
    {
        isPlayerDetected = false;

        // 부채꼴 시야의 시작 각도 계산
        float startAngle = -viewAngle / 2f;
        float angleStep = viewAngle / (rayCount - 1);

        for (int i = 0; i < rayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            // 현재 바라보는 방향을 기준으로 각 레이의 방향 벡터 계산
            Vector2 rayDirection = RotateVector(lastFacingDirection, currentAngle);

            // 레이를 쏘아 플레이어나 벽 중 가장 먼저 부딪히는 것을 감지
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, viewDistance, playerLayer | obstacleLayer);

            if (hit.collider != null)
            {
                // 부딪힌 오브젝트의 레이어가 플레이어 레이어 세팅과 일치하는지 확인
                if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0)
                {
                    isPlayerDetected = true;
                    playerTransform = hit.transform;
                    break; // 한 개의 레이라도 플레이어를 잡았다면 루프 탈출
                }
            }
        }

        // 상태 스위칭 조건문
        if (isPlayerDetected && currentState == EnemyState.Patrol)
        {
            Debug.Log("플레이어 포착! 추적을 시작합니다.");
            currentState = EnemyState.Chase;
        }
        else if (!isPlayerDetected && currentState == EnemyState.Chase)
        {
            Debug.Log("플레이어를 놓쳤습니다. 다시 순찰합니다.");
            currentState = EnemyState.Patrol;
            PickRandomDirection(); // 놓친 자리에서 즉시 새로운 순찰 방향 설정
        }
    }

    // ────────────────────────────────────────────────────────
    //  AI 행동 로직 (순찰 & 추적)
    // ────────────────────────────────────────────────────────
    void PatrolLogic()
    {
        patrolTimer += Time.deltaTime;
        if (patrolTimer >= directionChangeInterval)
        {
            PickRandomDirection();
        }
    }

    void ChaseLogic()
    {
        if (playerTransform == null) return;

        // 플레이어가 있는 방향 벡터 계산 후 정규화(normalized)
        Vector2 directionToPlayer = ((Vector2)playerTransform.position - (Vector2)transform.position).normalized;

        movementDirection = directionToPlayer;
        lastFacingDirection = directionDirectionPriority(directionToPlayer); // 쫓아가는 방향으로 시야 고정
    }

    void PickRandomDirection()
    {
        patrolTimer = 0f;
        int randomChoice = Random.Range(0, 5); // 0 ~ 4 랜덤

        switch (randomChoice)
        {
            case 0: movementDirection = Vector2.up; break;
            case 1: movementDirection = Vector2.down; break;
            case 2: movementDirection = Vector2.left; break;
            case 3: movementDirection = Vector2.right; break;
            case 4: movementDirection = Vector2.zero; break; // 잠시 제자리 멈춤 효과
        }

        // 정지 상태가 아니라면 움직이는 방향을 바라보도록 설정
        if (movementDirection != Vector2.zero)
        {
            lastFacingDirection = movementDirection;
        }
    }

    // 대각선 이동 시 시야 정렬을 깔끔하게 상하좌우축으로 보정해주는 함수
    Vector2 directionDirectionPriority(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? Vector2.right : Vector2.left;
        }
        else
        {
            return dir.y > 0 ? Vector2.up : Vector2.down;
        }
    }

    // 2D 벡터 회전용 수학 헬퍼 함수
    Vector2 RotateVector(Vector2 vector, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
        return new Vector2(cos * vector.x - sin * vector.y, sin * vector.x + cos * vector.y);
    }

    // ────────────────────────────────────────────────────────
    //  에디터 뷰 시각화 (Gizmos)
    // ────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // 인스펙터 창에서 Enemy 선택 시 시야 레이를 노란색/빨간색으로 그려줍니다.
        Gizmos.color = (currentState == EnemyState.Chase) ? Color.red : Color.yellow;

        float startAngle = -viewAngle / 2f;
        float angleStep = viewAngle / (rayCount - 1);

        for (int i = 0; i < rayCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 rayDirection = RotateVector(lastFacingDirection, currentAngle);
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + rayDirection * viewDistance);
        }
    }
}