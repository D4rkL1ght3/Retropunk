using System.Collections;
using TMPro;
using UnityEngine;
using static Gun;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Default,
        GunOneHanded,
        GunTwoHanded,
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
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    [SerializeField] Animator defaultAnimator;
    [SerializeField] Animator gunAnimator;
    [SerializeField] Animator twoHandedAnimator;
    [SerializeField] Animator meleeAnimator;

    private Animator currentAnimator
    {
        get
        {
            return GetActiveModel().GetComponent<Animator>();
        }
    }

    GameObject GetActiveModel()
    {
        if (defaultModel.activeSelf) return defaultModel;
        if (oneHandedModel.activeSelf) return oneHandedModel;
        if (twoHandedModel.activeSelf) return twoHandedModel;
        if (meleeModel.activeSelf) return meleeModel;

        return defaultModel;
    }

    private float moveInput;
    private bool isGrounded;

    [Header("Player Models")]
    [SerializeField] private GameObject defaultModel;
    [SerializeField] private GameObject oneHandedModel;
    [SerializeField] private GameObject twoHandedModel;
    [SerializeField] private GameObject meleeModel;

    [Header("Sprite Renderers")]
    [SerializeField] SpriteRenderer armRenderer;
    [SerializeField] SpriteRenderer gunRenderer;

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

    [Header("Arm Sprites")]
    [SerializeField] private Sprite armRight;
    [SerializeField] private Sprite armUpRight;
    [SerializeField] private Sprite armDownRight;
    [SerializeField] private Sprite armUp;
    [SerializeField] private Sprite armDown;

    [Header("Two Arm Pivoting")]
    [SerializeField] private Vector2 posRightTwo;
    [SerializeField] private Vector2 posUpRightTwo;
    [SerializeField] private Vector2 posUpTwo;
    [SerializeField] private Vector2 posDownRightTwo;
    [SerializeField] private Vector2 posDownTwo;

    [SerializeField] private Vector2 gunOffsetRightTwo;
    [SerializeField] private Vector2 gunOffsetUpRightTwo;
    [SerializeField] private Vector2 gunOffsetUpTwo;
    [SerializeField] private Vector2 gunOffsetDownRightTwo;
    [SerializeField] private Vector2 gunOffsetDownTwo;

    [Header("Two Handed Sprites")]
    [SerializeField] private Sprite twoArmRight;
    [SerializeField] private Sprite twoArmUpRight;
    [SerializeField] private Sprite twoArmDownRight;
    [SerializeField] private Sprite twoArmUp;
    [SerializeField] private Sprite twoArmDown;

    [Header("Shooting")]
    private Gun currentGun;
    [SerializeField] Gun primaryWeapon;
    [SerializeField] Gun secondaryWeapon;
    [SerializeField] private Transform firePoint;

    Vector2 currentAimDirection;
    float currentSnappedAngle;

    [Header("Reloading")]
    private Coroutine reloadCoroutine;
    private bool isReloading;

    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip reloadSound;

    [Header("Melee")]
    [SerializeField] MeleeWeapon meleeWeapon;
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private Transform meleePoint;
    [SerializeField] private LayerMask enemyLayer;

    public int punchDamage = 4;
    public float punchRange = 0.4f;
    public float punchCooldown = 0.5f;

    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    [Header("Stamina")]
    public float maxStamina = 5f;
    private float currentStamina;
    public float CurrentStamina => currentStamina;

    public float staminaDrainRate = 1f;
    public float staminaRegenRate = 0.8f;

    private bool isRunning;

    private float staminaCooldownTimer;
    public float staminaCooldown = 0.6f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;

        if (primaryWeapon != null)
            primaryWeapon.Initialize();

        if (secondaryWeapon != null)
            secondaryWeapon.Initialize();

        currentStamina = maxStamina;
        UpdateAmmoUI();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        // Get horizontal input
        moveInput = Input.GetAxisRaw("Horizontal");

        // Running
        bool runInput = Input.GetKey(KeyCode.LeftShift);

        if (runInput && currentStamina > 0 && moveInput != 0)
            isRunning = true;
        else
            isRunning = false;

        if (currentState == PlayerState.Melee && moveInput != 0)
        {
            staminaCooldownTimer = staminaCooldown;
        }

        HandleStamina();

        // Flip sprite
        if (currentState == PlayerState.Default || currentState == PlayerState.Melee)
        {
            if (moveInput > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (moveInput < 0)
                transform.localScale = new Vector3(-1, 1, 1);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && !doubleJumpUsed && currentStamina >= 0.25f && currentState != PlayerState.Melee)
        {
            if (currentState != PlayerState.Default)
                doubleJumpUsed = true; // Disable double jump

            if (!isGrounded && !isClimbing)
            {
                // Double Jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                defaultAnimator.SetTrigger("DoubleJump");
                doubleJumpUsed = true;
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            currentStamina -= 0.25f; // Small stamina cost for jumping
            staminaCooldownTimer = staminaCooldown;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }

        // Climbing
        verticalInput = Input.GetAxisRaw("Vertical");

        if (isOnLadder && Mathf.Abs(verticalInput) > 0f && currentState == PlayerState.Default)
            StartClimbing();

        if ((currentState == PlayerState.GunOneHanded || currentState == PlayerState.GunTwoHanded) && isClimbing)
            StopClimbing();

        if (isOnLadder && isGrounded && verticalInput < 0f)
            StopClimbing();

        if (isClimbing && (Mathf.Abs(moveInput) > 0 || Input.GetButtonDown("Jump")))
            StopClimbing();

        // Toggle states
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (currentGun == primaryWeapon)
            {
                EnterDefaultMode();
                return;
            }
            EquipGun(primaryWeapon);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (currentGun == secondaryWeapon)
            {
                EnterDefaultMode();
                return;
            }
            EquipGun(secondaryWeapon);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (currentState == PlayerState.Melee)
            {
                EnterDefaultMode();
                return;
            }
            EnterMeleeMode();
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            EnterDefaultMode();
        }

        // Animations
        currentAnimator.SetFloat("Speed", Mathf.Abs(moveInput));
        defaultAnimator.SetFloat("ClimbSpeed", verticalInput);
        currentAnimator.SetBool("IsJumping", !isGrounded && !isClimbing);
        currentAnimator.SetBool("IsRunning", isRunning);
        defaultAnimator.SetBool("IsClimbing", isClimbing);

        if (currentState == PlayerState.Melee && meleeWeapon != null)
        {
            weaponAnimator.SetFloat("Speed", Mathf.Abs(moveInput));
            weaponAnimator.SetBool("IsRunning", isRunning);
        }

        if (currentState == PlayerState.Default || currentState == PlayerState.Melee)
        {
            if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime && !isAttacking)
            {
                Attack();
                nextAttackTime = Time.time + GetMeleeCooldown();
            }
        }
        else
        {
            if (isAttacking)
                isAttacking = false;
        }

        // Aiming
        if (currentState == PlayerState.GunOneHanded || currentState == PlayerState.GunTwoHanded)
        {
            AimTowardMouse();
        }

        // Shooting
        if ((currentState == PlayerState.GunOneHanded || currentState == PlayerState.GunTwoHanded)
            && currentGun != null && !isReloading)
        {
            if (Input.GetMouseButton(0))
            {
                if (currentGun.CanShoot())
                {
                    currentGun.Shoot(firePoint, currentAimDirection);

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
                reloadCoroutine =  StartCoroutine(Reload());
            }
        }

        if (currentState == PlayerState.Default || currentState == PlayerState.Melee)
        {
            ammoText.gameObject.SetActive(false);
            CancelReload();
        }
        else
        {
            ammoText.gameObject.SetActive(true);
        }

        if (!isReloading && audioSource.loop == true)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }
    }

    void SetModel(GameObject activeModel)
    {
        defaultModel.SetActive(false);
        oneHandedModel.SetActive(false);
        twoHandedModel.SetActive(false);
        meleeModel.SetActive(false);

        activeModel.SetActive(true);
    }

    void EquipGun(Gun gun)
    {
        if (gun == null) return;

        CancelReload(); // Cancel reload when switching guns
        currentGun = gun;

        // Decide state based on weapon type
        switch (gun.gunType)
        {
            case GunType.OneHanded:
                currentState = PlayerState.GunOneHanded;
                SetModel(oneHandedModel);
                break;

            case GunType.TwoHanded:
                currentState = PlayerState.GunTwoHanded;
                SetModel(twoHandedModel);
                break;
        }

        armPivot.gameObject.SetActive(true);
        UpdateAmmoUI();
    }

    void EnterDefaultMode()
    {
        currentState = PlayerState.Default;
        SetModel(defaultModel);
        armPivot.gameObject.SetActive(false);
        currentGun = null;
    }

    void EnterMeleeMode()
    {
        currentState = PlayerState.Melee;
        SetModel(meleeModel);
        armPivot.gameObject.SetActive(false);
        currentGun = null;

        if (meleeWeapon != null)
        {
            weaponAnimator.runtimeAnimatorController = meleeWeapon.animator;
            weaponAnimator.gameObject.SetActive(true);
        }
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
    void Attack()
    {
        isAttacking = true;
        currentAnimator.SetTrigger("Attack");
        currentAnimator.SetBool("IsAttacking", true);

        if (currentState == PlayerState.Melee && meleeWeapon != null)
        {
            weaponAnimator.SetTrigger("Attack");
            weaponAnimator.SetBool("IsAttacking", true);
        }
    }

    public void DealMeleeDamage()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            meleePoint.position,
            GetMeleeRange(),
            enemyLayer
        );

        foreach (Collider2D hit in hits)
        {
            Health health = hit.GetComponent<Health>();

            if (health != null)
            {
                health.TakeDamage(GetMeleeDamage());
            }
        }

        isAttacking = false;
        currentAnimator.SetBool("IsAttacking", false);

        if (currentState == PlayerState.Melee && meleeWeapon != null)
        {
            weaponAnimator.SetBool("IsAttacking", false);
        }
    }

    int GetMeleeDamage()
    {
        if (currentState == PlayerState.Melee && meleeWeapon != null)
            return meleeWeapon.damage;

        return punchDamage;
    }

    float GetMeleeRange()
    {
        if (currentState == PlayerState.Melee && meleeWeapon != null)
            return meleeWeapon.range;

        return punchRange;
    }

    float GetMeleeCooldown()
    {
        if (currentState == PlayerState.Melee && meleeWeapon != null)
            return meleeWeapon.cooldown;

        return punchCooldown;
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
        {
            audioSource.clip = reloadSound;
            audioSource.Play();
            audioSource.loop = true;
        }

        yield return new WaitForSeconds(currentGun.reloadTime);

        currentGun.Reload();
        UpdateAmmoUI();

        isReloading = false;
        reloadCoroutine = null;
        Debug.Log("Reloaded!");
    }

    void CancelReload()
    {
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
        if (currentGun == null) return;

        if (currentState == PlayerState.GunTwoHanded)
        {
            armRenderer.sortingOrder = 6; // Ensure arms are in front of the gun
            gunRenderer.sortingOrder = 5; // Ensure gun is behind the arms

            if (snappedAngle == 0 || snappedAngle == 180)
            {
                armRenderer.sprite = twoArmRight;
                gunRenderer.sprite = currentGun.baseSprite;
                armPivot.localPosition = posRightTwo;
                gunPivot.localPosition = gunOffsetRightTwo;
                gunPivot.localRotation = Quaternion.identity;
            }
            else if (snappedAngle == 45 || snappedAngle == 135)
            {
                armRenderer.sprite = twoArmUpRight;
                gunRenderer.sprite = currentGun.diagonalSprite;
                armPivot.localPosition = posUpRightTwo;
                gunPivot.localPosition = gunOffsetUpRightTwo;
                gunPivot.localRotation = Quaternion.Euler(0, 0, 90);
            }
            else if (snappedAngle == 90)
            {
                armRenderer.sprite = twoArmUp;
                gunRenderer.sprite = currentGun.baseSprite;
                armPivot.localPosition = posUpTwo;
                gunPivot.localPosition = gunOffsetUpTwo;
                gunPivot.localRotation = Quaternion.Euler(0, 0, 90);
            }
            else if (snappedAngle == -45 || snappedAngle == -135)
            {
                armRenderer.sprite = twoArmDownRight;
                gunRenderer.sprite = currentGun.diagonalSprite;
                armPivot.localPosition = posDownRightTwo;
                gunPivot.localPosition = gunOffsetDownRightTwo;
                gunPivot.localRotation = Quaternion.identity;
            }
            else if (snappedAngle == -90)
            {
                armRenderer.sprite = twoArmDown;
                gunRenderer.sprite = currentGun.baseSprite;
                armPivot.localPosition = posDownTwo;
                gunPivot.localPosition = gunOffsetDownTwo;
                gunPivot.localRotation = Quaternion.Euler(0, 0, -90);
            }
            return; // Skip one-handed logic
        }

        armRenderer.sortingOrder = 3;
        gunRenderer.sortingOrder = 2;

        if (snappedAngle == 0 || snappedAngle == 180)
        {
            armRenderer.sprite = armRight;
            gunRenderer.sprite = currentGun.baseSprite;
            armPivot.localPosition = posRight;
            gunPivot.localPosition = gunOffsetRight;
            gunPivot.localRotation = Quaternion.identity;
        }
        else if (snappedAngle == 45 || snappedAngle == 135)
        {
            armRenderer.sprite = armUpRight;
            gunRenderer.sprite = currentGun.diagonalSprite;
            armPivot.localPosition = posUpRight;
            gunPivot.localPosition = gunOffsetUpRight;
            gunPivot.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (snappedAngle == 90)
        {
            armRenderer.sprite = armUp;
            gunRenderer.sprite = currentGun.baseSprite;
            armPivot.localPosition = posUp;
            gunPivot.localPosition = gunOffsetUp;
            gunPivot.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else if (snappedAngle == -45 || snappedAngle == -135)
        {
            armRenderer.sprite = armDownRight;
            gunRenderer.sprite = currentGun.diagonalSprite;
            armPivot.localPosition = posDownRight;
            gunPivot.localPosition = gunOffsetDownRight;
            gunPivot.localRotation = Quaternion.identity;
        }
        else if (snappedAngle == -90)
        {
            armRenderer.sprite = armDown;
            gunRenderer.sprite = currentGun.baseSprite;
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
            float speed;
            if (currentState == PlayerState.Melee)
            {
                speed = isRunning ? moveSpeed * 1.2f : moveSpeed * 1f;
            }
            else
            {
                speed = isRunning ? moveSpeed : moveSpeed * 0.5f;
            }
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
        Gizmos.DrawWireSphere(meleePoint.position, GetMeleeRange());
    }
}
