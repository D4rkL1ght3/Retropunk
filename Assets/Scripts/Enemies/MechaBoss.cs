using UnityEngine;

public class MechaBoss : EnemyMelee
{
    [Header("Seeking Missile Barrage")]
    public GameObject missilePrefab;
    public Transform missileLaunchPoint;

    public int missileCount = 6;
    public float missileCooldown = 4f;

    private float nextMissileTime;
    private bool fightStarted = false;

    protected override void Update()
    {
        base.Update();

        if (!fightStarted && currentState == EnemyState.Chase)
        {
            fightStarted = true;
            nextMissileTime = Time.time + missileCooldown;
        }

        if (currentState == EnemyState.Chase)
        {
            TryMissileBarrage();
        }
    }

    void TryMissileBarrage()
    {
        if (Time.time < nextMissileTime)
            return;

        FireMissileBarrage();

        nextMissileTime = Time.time + missileCooldown;
    }

    void FireMissileBarrage()
    {
        for (int i = 0; i < missileCount; i++)
        {
            GameObject missile = Instantiate(
                missilePrefab,
                missileLaunchPoint.position,
                missileLaunchPoint.rotation
            );

            SeekingMissile seekingMissile =
                missile.GetComponent<SeekingMissile>();

            if (seekingMissile != null)
            {
                seekingMissile.player = player;
            }
        }
    }
}