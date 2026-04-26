using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementComponent : MonoBehaviour
{
    public int FacingDirection { get; private set; } = 1;
    public bool FacingRight { get; private set; } = true;

    // ==========================================
    // 환경 감지 센서 (기존 Entity에 있던 것들)
    // ==========================================
    [Header("Collision Info")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [SerializeField] private Transform wallCheck;
    [SerializeField] private float wallCheckDistance = 0.5f;

    public bool CanAutoFlip { get; set; } = true;
    public LayerMask GroundLayer => groundLayer;
    public float WallCheckDistacne => wallCheckDistance;

    // 수정된 코드 (지연 초기화 패턴)
    private Rigidbody2D _rb;
    public Rigidbody2D RB
    {
        get
        {
            // 누군가 RB를 달라고 했을 때, 아직 못 찾았다면 지금 당장 찾아서 돌려준다!
            if (_rb == null)
            {
                _rb = GetComponent<Rigidbody2D>();
            }
            return _rb;
        }
    }

    // ==========================================
    // 속도 제어
    // ==========================================
    public void SetVelocity(float xVelocity, float yVelocity)
    {
        RB.linearVelocity = new Vector2(xVelocity, yVelocity);

        if (CanAutoFlip)
            FlipController(xVelocity); // 속도에 맞춰 자동으로 방향 뒤집기
    }

    public void ZeroVelocity()
    {
        RB.linearVelocity = Vector2.zero;
    }

    // ==========================================
    // 센서 체크
    // ==========================================
    public virtual bool IsGrounded() => Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
    public virtual bool IsWallDetected() => Physics2D.Raycast(wallCheck.position, transform.right, wallCheckDistance, groundLayer);

    // ==========================================
    // 방향 전환 (Flip)
    // ==========================================
    public void Flip()
    {
        FacingDirection *= -1;
        FacingRight = !FacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public void FlipController(float moveX)
    {
        if (moveX > 0 && !FacingRight) Flip();
        else if (moveX < 0 && FacingRight) Flip();
    }

    // ==========================================
    // 기즈모 (센서 시각화)
    // ==========================================
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }

        if (wallCheck != null)
        {
            Gizmos.color = IsWallDetected() ? Color.green : Color.red;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + transform.right * wallCheckDistance);
        }
    }
}
