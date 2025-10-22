using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerMobile : MonoBehaviour
{
    [Header("Joystick Reference")]
    public Joystick joystick;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("Jump Settings")]
    [Range(0.1f, 1f)]
    public float jumpThreshold = 0.8f; // この値以上上に倒すとジャンプ（調整可能）

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool jumpPressed = false;

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (joystick == null) return;

        // 入力方向を取得
        Vector2 dir = joystick.InputDirection;

        // 水平方向に移動
        rb.velocity = new Vector2(dir.x * moveSpeed, rb.velocity.y);
    }

    void FixedUpdate()
    {
        // 接地判定
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // ジャンプ処理
        if (jumpPressed)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpPressed = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        // 接地確認用ギズモ表示
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
