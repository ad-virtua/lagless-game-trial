using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.AudioSettings;

public class Player : MonoBehaviour
{
    [SerializeField] private Sprite[] idel, run, jump, shot, runShot;
    [SerializeField] private float moveSpeed, jumpForce, idelAnimSpeed, runAnimSpeed, jumpAnimSpeed, shotAnimSpeed, runShotAnimSpeed;
    [SerializeField] private GameObject shotPoint;
    [SerializeField] private GameObject shotPrefab;
    [SerializeField] private float shotInterval;
    [SerializeField] private int hp;
    [SerializeField] private Joystick joystick;

    private SpriteRenderer spriteRenderer;
    private Vector3 playerScale;
    private Rigidbody2D rb;
    private bool isGrounded;
    private float shotIntervalCount;
    public float knockbackForce = 0.01f; // 吹き飛ばす力

    private bool isShotMobile;
    private float shotMobileInterval;

    enum AnimType
    {
        Idel,
        Run,
        Jump,
        Shot,
        RunShot,
        Damage
    }
    AnimType animType;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (StageMoveSystem.instance.isScreenMove)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
        else
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (WebGLMobileChecker.IsMobile()) MoveMobile();
            else Move();
#else
            if (Application.isMobilePlatform)
            {
                MoveMobile();
            }
            else Move();
#endif
            if (shotMobileInterval > 0) shotMobileInterval -= Time.deltaTime;
            if (shotMobileInterval < 0)
            {
                shotMobileInterval = 0;
                isShotMobile = false;
            }

            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Move()
    {
        // Aキーを押している間
        if (Input.GetKey(KeyCode.A))
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) RunShotAnim();
            if (!isShotMobile) RunAnim();
            transform.localScale = new Vector3(-playerScale.x, playerScale.y, playerScale.z);
            transform.Translate(-moveSpeed * Time.deltaTime, 0, 0);
        }
        // Dキーを押している間
        else if (Input.GetKey(KeyCode.D))
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) RunShotAnim();
            if (!isShotMobile) RunAnim();
            transform.localScale = playerScale;
            transform.Translate(moveSpeed * Time.deltaTime, 0, 0);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.LeftShift)) ShotAnim();
            if (!isShotMobile) IdelAnim();
        }

        // ジャンプ（スペースキー）
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            if (!Input.GetKeyDown(KeyCode.LeftShift)) JumpAnim();
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    private void MoveMobile()
    {
        if (joystick == null) return;

        // 入力方向を取得
        Vector2 dir = joystick.InputDirection;

        if (dir.x == 0)
        {
            if (!isShotMobile) IdelAnim();
            return;
        }
        else if (dir.x > 0)
        {
            transform.Translate(moveSpeed * Time.deltaTime, 0, 0);
            transform.localScale = playerScale;
        }
        else if (dir.x < 0)
        {
            transform.Translate(-moveSpeed * Time.deltaTime, 0, 0);
            transform.localScale = new Vector3(-playerScale.x, playerScale.y, playerScale.z);
        }
        if (!isShotMobile) RunAnim();
    }

    public void ShotMobile()
    {
        // 入力方向を取得
        Vector2 dir = joystick.InputDirection;

        if (dir.x == 0) ShotAnim();
        else RunShotAnim();
    }

    public void JumpMobile()
    {
        if (isGrounded)
        {
            if (!isShotMobile) JumpAnim();
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
        }
    }

    // 地面との接触判定
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 法線が上向きに近ければ床と判定
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    break;
                }
            }
        }

        if (collision.transform.tag == "Enemy" && gameObject.layer == 6)
        {
            ApplyKnockback(collision.transform.position);
            StartCoroutine(StealthTime(1f));
            StartCoroutine(Generic.DamageFlash(GetComponent<SpriteRenderer>(), 0.05f, 20));
        }
    }

    IEnumerator StealthTime(float interval)
    {
        gameObject.layer = 8;
        yield return new WaitForSeconds(interval);
        gameObject.layer = 6;
    }

    // 当たり判定があったときに呼ぶ
    public void ApplyKnockback(Vector3 hitPosition)
    {
        // 自分と衝突した位置のX座標を比較して方向を決定
        float direction = transform.position.x < hitPosition.x ? -1f : 1f;

        // 左右方向の力だけ加える
        Vector3 force = new Vector3(direction * knockbackForce, 0, 0);

        rb.AddForce(force * 0.6f, ForceMode2D.Impulse);
    }

    private void IdelAnim()
    {
        if (animType != AnimType.Idel && isGrounded)
        {
            animType = AnimType.Idel;
            StartCoroutine(AnimSpeed(idel, idelAnimSpeed, AnimType.Idel));
        }
    }

    private void RunAnim()
    {
        if (animType != AnimType.Run && isGrounded)
        {
            animType = AnimType.Run;
            StartCoroutine(AnimSpeed(run, runAnimSpeed, AnimType.Run));
        }
    }

    private void JumpAnim()
    {
        if (animType != AnimType.Jump)
        {
            animType = AnimType.Jump;
            StartCoroutine(AnimSpeed(jump, jumpAnimSpeed, AnimType.Jump, true));
        }
    }

    private void ShotAnim()
    {
        if (animType != AnimType.Shot)
        {
            animType = AnimType.Shot;
            StartCoroutine(AnimSpeed(shot, shotAnimSpeed, AnimType.Shot));
        }
        CreateShot();
    }

    private void RunShotAnim()
    {
        if (animType != AnimType.RunShot)
        {
            animType = AnimType.RunShot;
            StartCoroutine(AnimSpeed(runShot, runShotAnimSpeed, AnimType.RunShot));
        }
        CreateShot();
    }

    IEnumerator AnimSpeed(Sprite[] targetAnim, float targetSpeed, AnimType targetAnimType, bool isNotLoop = false)
    {
        while (animType == targetAnimType)
        {
            for (int i = 0; i < targetAnim.Length; i++)
            {
                if (animType != targetAnimType) yield break;
                spriteRenderer.sprite = targetAnim[i];

                yield return new WaitForSeconds(targetSpeed);
            }

            yield return new WaitWhile(() => StageMoveSystem.instance.isScreenMove);

            if (isNotLoop) yield break;
        }
    }

    void CreateShot()
    {
        if (!isShotMobile) isShotMobile = true;
        shotMobileInterval = 0.5f;

        var shot = Instantiate(shotPrefab);
        shot.transform.parent = shotPoint.transform;
        shot.transform.localPosition = Vector3.zero;
        shot.GetComponent<Shot>().playerX = transform.localScale.x;
        shot.transform.localScale = new Vector3(1, 1, 1);
        shot.transform.parent = null;
    }

    void CreateShotInterval()
    {
        if (shotIntervalCount == 0)
        {
            var shot = Instantiate(shotPrefab);
            shot.transform.parent = shotPoint.transform;
            shot.transform.localPosition = Vector3.zero;
            shot.GetComponent<Shot>().playerX = transform.localScale.x;
            shot.transform.localScale = new Vector3(1, 1, 1);
            shot.transform.parent = null;
        }
        shotIntervalCount += Time.deltaTime;

        if (shotIntervalCount >= shotInterval)
        {
            shotIntervalCount = 0;
        }
    }
}
