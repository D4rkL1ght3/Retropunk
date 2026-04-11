using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Default,
        Gun,
        Melee
    }

    public PlayerState currentState = PlayerState.Default;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float jumpForce = 8f;
    private bool doubleJumpUsed = false;

    public float climbSpeed = 3f;
    private int ladderCount = 0;
    private bool isOnLadder => ladderCount > 0;
    private float ladderX;
    private bool isClimbing;
    private float verticalInput;
    private float defaultGravity;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    [SerializeField] Animator defaultAnimator;
    [SerializeField] Animator gunAnimator;
    private Animator currentAnimator
    {
        get
        {
            return currentState == PlayerState.Default
                ? defaultAnimator
                : gunAnimator;
        }
    }

    private float moveInput;
    private bool isGrounded;

    [Header("Sprite Renderers")]
    [SerializeField] SpriteRenderer defaultRenderer;
    [SerializeField] SpriteRenderer gunBodyRenderer;
    [SerializeField] private SpriteRenderer armRenderer;
    [SerializeField] private SpriteRenderer gunRenderer;

    [Header("Arm Pivoting")]
    [SerializeField] Transform armPivot;
    [SerializeField] Transform gunPivot;

    [SerializeField] private Vector2 posRight;
    [SerializeField] private Vector2 posUpRight;
    [SerializeField] private Vector2 posUp;
    [SerializeField] private Vector2 posDownRight;
    [SerializeField] private Vector2 posDown;

    [SerializeField] private Vector2 gunOffsetRight;
    [SerializeField] private Vector2 gunOffsetUpRight;
    [SerializeField] private Vector2 gunOffsetUp;
    [SerializeField] private Vector2 gunOffsetDownRight;
    [SerializeField] private Vector2 gunOffsetDown;

    [Header("Sprites")]
    [SerializeField] private Sprite armRight;
    [SerializeField] private Sprite armUpRight;
    [SerializeField] private Sprite armDownRight;
    [SerializeField] private Sprite armUp;
    [SerializeField] private Sprite armDown;

    [SerializeField] private Sprite gunBase;
    [SerializeField] private Sprite gunDiagonal;

    [Header("Shooting")]
    public Gun currentGun;
    [SerializeField] private Transform firePoint;
    [SerializeField] AudioClip shootSound;

    Vector2 currentAimDirection;
    float currentSnappedAngle;

    [Header("Reloading")]
    private Coroutine reloadCoroutine;
    private bool isReloading;

    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip reloadSound;

    [Header("Melee")]
    public int punchDamage = 10;
    public float punchRange = 1.2f;
    [SerializeField] private Transform meleePoint;
    [SerializeField] private LayerMask enemyLayer;

    public float punchCooldown = 0.5f;
    private float nextPunchTime = 0f;
    private bool isPunching = false;

    [Header("Stamina")]
    public float maxStamina = 5f;
    private float currentStamina;
    public float CurrentStamina => currentStamina;

    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 0.8f;

    private bool isRunning;

    private float staminaCooldownTimer;
    public float staminaCooldown = 1.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;

        if (currentGun != null)
            currentGun.Initialize();

        currentStamina = maxStamina;
        UpdateAmmoUI();
    }

    void Update()
    {
        // Get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Running
        bool runInput = Input.GetKey(KeyCode.LeftShift);

        if (runInput && currentStamina > 0 && moveInput != 0 && isGrounded)
            isRunning = true;
        else
            isRunning = false;

        currentAnimator.SetBool("IsRunning", isRunning);
        HandleStamina();

        // Flip sprite
        if (currentState == PlayerState.Default)
        {
            if (moveInput > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (moveInput < 0)
                transform.localScale = new Vector3(-1, 1, 1);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && !doubleJumpUsed)
        {
            if (!isGrounded && !isClimbing)
            {
                // Double Jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                defaultAnimator.SetTrigger("DoubleJump");
                doubleJumpUsed = true;
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Climbing
        verticalInput = Input.GetAxisRaw("Vertical");

        if (isOnLadder && Mathf.Abs(verticalInput) > 0f && currentState == PlayerState.Default)
            StartClimbing();

        if (currentState == PlayerState.Gun && isClimbing)
            StopClimbing();

        if (isOnLadder && isGrounded && verticalInput < 0f)
            StopClimbing();

        if (isClimbing && (Mathf.Abs(moveInput) > 0 || Input.GetButtonDown("Jump")))
            StopClimbing();

        // Toggle gun state
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ToggleGunState();
        }

        // Animator
        currentAnimator.SetFloat("Speed", Mathf.Abs(moveInput));
        defaultAnimator.SetFloat("ClimbSpeed", verticalInput);
        currentAnimator.SetBool("IsJumping", !isGrounded && !isClimbing);
        defaultAnimator.SetBool("IsClimbing", isClimbing);

        if (currentState == PlayerState.Default)
        {
            if (Input.GetMouseButtonDown(0) && Time.time >= nextPunchTime && !isPunching)
            {
                defaultAnimator.SetTrigger("Punch");
                defaultAnimator.SetBool("IsAttacking", true);
                nextPunchTime = Time.time + punchCooldown;
            }
        }

        if (currentState == PlayerState.Gun)
        {
            AimTowardMouse();
        }

        if (currentState == PlayerState.Gun && currentGun != null && !isReloading)
        {
            if (Input.GetMouseButton(0))
            {
                if (currentGun.CanShoot())
                {
                    currentGun.Shoot(firePoint, currentAimDirection);

                    if (shootSound != null)
                        audioSource.PlayOneShot(shootSound);

                    UpdateAmmoUI();
                }
                else if (currentGun.NeedsReload())
                {
                    if (!isReloading)
                    {
                        reloadCoroutine = StartCoroutine(Reload());
                    }
                }
            }

            // Manual reload
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(Reload());
            }
        }

        if (currentState != PlayerState.Gun)
        {
            ammoText.gameObject.SetActive(false);

            if (isReloading && reloadCoroutine != null)
            {
                StopCoroutine(reloadCoroutine);
                reloadCoroutine = null;
                isReloading = false;
                Debug.Log("Reload cancelled.");

                if (reloadSound != null)
                    audioSource.Stop();
            }
        }
        else
        {
            ammoText.gameObject.SetActive(true);
        }
    }

    void ToggleGunState()
    {
        if (currentState == PlayerState.Default)
        {
            currentState = PlayerState.Gun;
            EnterGunMode();
        }
        else
        {
            currentState = PlayerState.Default;
            EnterDefaultMode();
        }
    }

    void EnterGunMode()
    {
        defaultRenderer.gameObject.SetActive(false);
        gunBodyRenderer.gameObject.SetActive(true);
        armPivot.gameObject.SetActive(true);
    }
    void EnterDefaultMode()
    {
        defaultRenderer.gameObject.SetActive(true);
        gunBodyRenderer.gameObject.SetActive(false);
        armPivot.gameObject.SetActive(false);
    }

    // Stamina
    void HandleStamina()
    {
        if (isRunning)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            staminaCooldownTimer = staminaCooldown;
        }
        else
        {
            if (staminaCooldownTimer > 0)
                staminaCooldownTimer -= Time.deltaTime;
            else
                currentStamina += staminaRegenRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    }

    // Melee Attack
    void Punch()
    {
        isPunching = true;

        defaultAnimator.SetTrigger("Punch");
    }

    public void DealPunchDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            meleePoint.position,
            punchRange,
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponent<Health>();

            if (health != null)
            {
                health.TakeDamage(punchDamage);
            }
        }

        isPunching = false;
        defaultAnimator.SetBool("IsAttacking", false);
    }

    // Aim Rotation
    void AimTowardMouse()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 direction = (mouseWorld - transform.position).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        currentSnappedAngle = Mathf.Round(angle / 45f) * 45f;

        currentAimDirection = GetSnappedDirection(direction);

        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        UpdateArmSprite(currentSnappedAngle);
    }

    IEnumerator Reload()
    {
        if (currentGun == null)
            yield break;

        if (currentGun.currentAmmo == currentGun.maxAmmo)
            yield break;

        isReloading = true;
        Debug.Log("Reloading...");

        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(currentGun.reloadTime);

        currentGun.Reload();
        UpdateAmmoUI();

        isReloading = false;
        reloadCoroutine = null;
        Debug.Log("Reloaded!");
    }

    Vector2 GetSnappedDirection(Vector2 rawDirection)
    {
        float angle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;

        // Snap to nearest 45 degrees
        float snappedAngle = Mathf.Round(angle / 45f) * 45f;

        float rad = snappedAngle * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    void UpdateArmSprite(float snappedAngle)
    {
        if (snappedAngle == 0 || snappedAngle == 180)
        {
            armRenderer.sprite = armRight;
            gunRenderer.sprite = currentGun.gunBaseSprite;
            armPivot.localPosition = posRight;
            gunPivot.localPosition = gunOffsetRight;
            gunPivot.localRotation = Quaternion.identity;
        }
        else if (snappedAngle == 45 || snappedAngle == 135)
        {
            armRenderer.sprite = armUpRight;
            gunRenderer.sprite = currentGun.gunDiagonalSprite;
            armPivot.localPosition = posUpRight;
            gunPivot.localPosition = gunOffsetUpRight;
            gunPivot.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (snappedAngle == 90)
        {
            armRenderer.sprite = armUp;
            gunRenderer.sprite = currentGun.gunBaseSprite;
            armPivot.localPosition = posUp;
            gunPivot.localPosition = gunOffsetUp;
            gunPivot.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (snappedAngle == -45 || snappedAngle == -135)
        {
            armRenderer.sprite = armDownRight;
            gunRenderer.sprite = currentGun.gunDiagonalSprite;
            armPivot.localPosition = posDownRight;
            gunPivot.localPosition = gunOffsetDownRight;
            gunPivot.localRotation = Quaternion.identity;
        }
        else if (snappedAngle == -90)
        {
            armRenderer.sprite = armDown;
            gunRenderer.sprite = currentGun.gunBaseSprite;
            armPivot.localPosition = posDown;
            gunPivot.localPosition = gunOffsetDown;
            gunPivot.localRotation = Quaternion.Euler(0, 0, -90);
        }
    }

    void StartClimbing()
    {
        isClimbing = true;
        rb.gravityScale = 0f;
    }

    void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = defaultGravity;
    }

    void UpdateAmmoUI()
    {
        if (currentGun == null || ammoText == null) return;

        ammoText.text = currentGun.currentAmmo + " / " + currentGun.maxAmmo;
        
        if (currentGun.currentAmmo == 0)
            ammoText.color = Color.red;
        else
            ammoText.color = Color.white;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            ladderCount++;

            // Snap target X to ladder center
            ladderX = collision.bounds.center.x;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            ladderCount--;
            if (ladderCount <= 0)
            {
                ladderCount = 0;
                StopClimbing();
            }
        }
    }

    void FixedUpdate()
    {
        // Movement
        if (isClimbing)
        {
            rb.linearVelocity = new Vector2(0f, verticalInput * climbSpeed);

            // Snap X position to ladder center
            transform.position = new Vector3(
                ladderX,
                transform.position.y,
                transform.position.z
            );
        }
        else
        {
            float speed = isRunning ? moveSpeed : moveSpeed * 0.5f;
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
        }

        // Ground Check
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (doubleJumpUsed && isGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            doubleJumpUsed = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (meleePoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(meleePoint.position, punchRange);
    }
}
