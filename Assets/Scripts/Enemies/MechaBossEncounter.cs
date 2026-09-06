using UnityEngine;
using UnityEngine.Playables;

public class MechaBossEncounter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MechaBoss boss;
    [SerializeField] private PlayableDirector introTimeline;

    private Health bossHealth;
    private Rigidbody2D playerRb;

    private bool encounterStarted = false;

    void Awake()
    {
        if (boss != null)
            bossHealth = boss.GetComponent<Health>();

        if (playerController != null)
            playerRb = playerController.GetComponent<Rigidbody2D>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (encounterStarted)
            return;

        if (!collision.CompareTag("Player"))
            return;

        encounterStarted = true;

        StartIntroCutscene();
    }

    void StartIntroCutscene()
    {
        // Disable player control
        playerController.enabled = false;

        // Stop player movement
        if (playerRb != null)
            playerRb.linearVelocity = Vector2.zero;

        // Disable boss combat and health
        boss.Disable();

        // Play intro Timeline
        introTimeline.Play();

        // Listen for Timeline finishing
        introTimeline.stopped += OnIntroTimelineFinished;
    }

    void OnIntroTimelineFinished(PlayableDirector director)
    {
        // Re-enable player control
        playerController.enabled = true;

        // Re-enable boss combat and health
        boss.enabled = true;
        bossHealth.enabled = true;

        // Make sure the boss starts fighting immediately
        boss.aggroed = true;

        Debug.Log("Mecha Boss intro finished! Fight begins!");
    }
}