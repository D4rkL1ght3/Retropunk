using UnityEngine;

public class BossController : MonoBehaviour
{
    public GameObject cart;
    public GameObject driver;

    public int rushesBeforeMelee = 3;
    public float meleeDuration = 15f;

    private int rushCount = 0;
    private float meleeTimer;

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
    }

    void Update()
    {
        if (state == State.Melee && !IsReturningToCart)
        {
            meleeTimer -= Time.deltaTime;

            if (meleeTimer <= 0f)
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
        state = State.Melee;

        driver.transform.position = cart.transform.position;
        driver.SetActive(true);

        // Stop cart movement (but keep it visible!)
        cart.GetComponent<EnemyRusher>().enabled = false;

        meleeTimer = meleeDuration;
        rushCount = 0;
    }

    void StartReturn()
    {
        IsReturningToCart = true;
    }

    public void TriggerRemount()
    {
        driver.SetActive(false);

        // Reactivate cart AI
        cart.GetComponent<EnemyRusher>().enabled = true;

        state = State.Cart;
        IsReturningToCart = false;
    }
}