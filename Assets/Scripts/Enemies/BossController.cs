using UnityEngine;

public class BossController : MonoBehaviour
{
    public GameObject cart;
    public GameObject driver;
    public Animator cartAnim;

    public int rushesBeforeMelee = 3;
    public float meleeDuration = 15f;

    private int rushCount = 0;
    private float meleeTimer;

    public Transform player;
    public float maxDistanceFromCart = 8f;

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

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (cartAnim == null)
            cartAnim = cart.GetComponent<Animator>();
    }

    void Update()
    {
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
            TriggerDismount();
        }
    }

    void TriggerDismount()
    {
        Vector3 scale = cart.transform.localScale;
        scale.x = 1f; // Ensure driver faces right direction
        cart.transform.localScale = scale;

        // Stop cart movement (but keep it visible!)
        cart.GetComponent<EnemyRusher>().enabled = false;

        if (cartAnim != null)
            cartAnim.SetTrigger("Dismount");
    }

    public void Dismount()
    {
        meleeTimer = meleeDuration;
        rushCount = 0;
        state = State.Melee;

        if (cartAnim != null)
            cartAnim.SetBool("driverOut", true);

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

        if (cartAnim != null)
        {
            cartAnim.SetTrigger("Remount");
            cartAnim.SetBool("driverOut", false);
        }
           
    }

    public void Remount()
    {
        // Reactivate cart AI
        cart.GetComponent<EnemyRusher>().enabled = true;

        if (cartAnim != null)
            

        state = State.Cart;
        IsReturningToCart = false;
    }
}