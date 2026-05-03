using UnityEngine;

public class BossController : MonoBehaviour
{
    public Transform player;

    [Header("References")]
    public GameObject cart;
    public GameObject driver;
    public Animator animator;
    public Rigidbody2D rb;

    public EnemySpawner[] spawners;
    [SerializeField] private Goal goal;

    [Header("Boss Settings")]
    public int rushesBeforeMelee = 3;
    public float meleeDuration = 15f;
    public float maxDistanceFromCart = 8f;
    public float dismountRange = 4f;

    private int rushCount = 0;
    private float meleeTimer;
    private bool readyToDismount = false;

    public bool IsReturningToCart { get; private set; }

    public Vector2 CartPosition => cart.transform.position;

    private enum State
    {
        Cart,
        Melee
    }

    private State state;

    void Start()
    {
        state = State.Cart;
        driver.SetActive(false);
        GetComponent<Health>().OnDeath += OnBossDeath;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (animator == null)
            animator = cart.GetComponent<Animator>();

        if (rb == null)
            rb = cart.GetComponent<Rigidbody2D>();

        if (spawners.Length == 0)
            spawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
    }

    void Update()
    {
        if (state == State.Cart && readyToDismount)
        {
            float distanceToPlayer = Vector2.Distance(
                cart.transform.position,
                player.position
            );

            if (distanceToPlayer <= dismountRange)
            {
                TriggerDismount();
                readyToDismount = false;
            }
        }

        if (state == State.Melee && !IsReturningToCart)
        {
            meleeTimer -= Time.deltaTime;

            float playerDistanceFromCart = Vector2.Distance(
                player.position,
                cart.transform.position
            );

            if (playerDistanceFromCart > maxDistanceFromCart)
            {
                StartReturn(); // FORCE retreat
                return;
            }

            if (meleeTimer <= 0f && state == State.Melee)
            {
                StartReturn();
            }
        }
    }

    public void OnRushFinished()
    {
        if (state != State.Cart) return;

        rushCount++;

        if (rushCount >= rushesBeforeMelee)
        {
            readyToDismount = true;
        }
    }

    void TriggerDismount()
    {
        Vector3 scale = cart.transform.localScale;
        scale.x = 1f; // Ensure driver faces right direction
        cart.transform.localScale = scale;

        // Stop cart movement (but keep it visible!)
        cart.GetComponent<EnemyRusher>().enabled = false;

        if (animator != null)
            animator.SetTrigger("Dismount");
    }

    public void Dismount()
    {
        meleeTimer = meleeDuration;
        rushCount = 0;
        state = State.Melee;

        if (animator != null)
            animator.SetBool("driverOut", true);

        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        driver.transform.position = cart.transform.position;
        driver.SetActive(true);
    }

    void StartReturn()
    {
        IsReturningToCart = true;
    }

    public void TriggerRemount()
    {
        driver.SetActive(false);

        if (animator != null)
        {
            animator.SetTrigger("Remount");
            animator.SetBool("driverOut", false);
        }
    }

    public void Remount()
    {
        // Reactivate cart AI
        cart.GetComponent<EnemyRusher>().enabled = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
        state = State.Cart;
        IsReturningToCart = false;
    }

    public void StartBossFight()
    {
        foreach (var spawner in spawners)
            spawner.StartSpawning();

        AudioManager.Instance.PlayBossMusic();
    }

    void OnBossDeath()
    {
        foreach (var spawner in spawners)
            spawner.StopSpawning();

        if (goal != null)
            goal.UnlockGoal();
    }

    void OnDrawGizmosSelected()
    {
        if (cart != null)
        {
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(cart.transform.position, maxDistanceFromCart);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cart.transform.position, dismountRange);
        }
    }
}