using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Sprite[] spriteUp;
    public Sprite[] spriteDown;
    public Sprite[] spriteLeft;
    public Sprite[] spriteRight;
    public float frameTime = 0.15f;
    public Tilemap groundTilemap;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 input;
    private Vector2 velocity;
    private Sprite[] currentSprites;
    private int frameIndex = 0;
    private float timer = 0f;
    private bool isFrozen = false;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        currentSprites = spriteDown;
        sr.sprite = currentSprites[0];
    }

    public void OnMove(InputValue value)
    {
        if (isFrozen) return;

        if (Time.timeScale == 0f)
        {
            input = Vector2.zero;
            velocity = Vector2.zero;
            return;
        }

        input = value.Get<Vector2>();
        velocity = input.normalized * moveSpeed;

        if (input.sqrMagnitude > 0.01f)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
           
            {
                
            }
            if (input.x > 0)
                ChangeSprites(spriteRight);
            else
                ChangeSprites(spriteLeft);
        }
        else
        {
            if (input.y > 0)
                ChangeSprites(spriteUp);
            else
                ChangeSprites(spriteDown);
        }
        }
        
    }

    private void Update()
    {   
        if (isFrozen) return;

        if (input.sqrMagnitude <= 0.01f)
        {
            frameIndex = 0;
            sr.sprite = currentSprites[frameIndex];
            return;
        }
        timer += Time.deltaTime;

        if (timer >= frameTime)
        {
            timer = 0f;
            frameIndex++;

            if (frameIndex >= currentSprites.Length)
                frameIndex = 0;
            sr.sprite = currentSprites[frameIndex];
        }
    }

    private void FixedUpdate()
    {   
        if (isFrozen) return;

        if (groundTilemap == null)
        {
            rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            return;
        }

        Vector2 currentPos = rb.position;
        Vector2 movement = velocity * Time.fixedDeltaTime;
        Vector2 nextPos = currentPos + movement;

        Vector3Int cellX = groundTilemap.WorldToCell(new Vector3(nextPos.x, currentPos.y, 0));
        if (!groundTilemap.HasTile(cellX))
        {
            nextPos.x = currentPos.x; // ground Ÿ���� ������ X�� �̵� ����
        }

        // 2. Y�� �̵� ���� ���� üũ
        Vector3Int cellY = groundTilemap.WorldToCell(new Vector3(currentPos.x, nextPos.y, 0));
        if (!groundTilemap.HasTile(cellY))
        {
            nextPos.y = currentPos.y; // ground Ÿ���� ������ Y�� �̵� ����
        }

        // ���� ���� ������ ��ġ�� �̵�
        rb.MovePosition(nextPos);
    }

    private void ChangeSprites(Sprite[] newSprites)
    {
        if (currentSprites == newSprites)
            return;
        currentSprites = newSprites;
        frameIndex = 0;
        timer = 0f;
        sr.sprite = currentSprites[frameIndex];
    }


        public void SetFreeze(bool freeze)
    {
        isFrozen = freeze;
        if (freeze)
        {
            input = Vector2.zero;
            velocity = Vector2.zero;
            frameIndex = 0;
            if (sr != null && currentSprites != null && currentSprites.Length > 0)
            {
                sr.sprite = currentSprites[0]; // 정지 상태(기본 프레임) 이미지로 고정
            }
        }
    }

}
