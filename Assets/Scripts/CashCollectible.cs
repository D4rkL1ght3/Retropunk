using UnityEngine;

public class CashCollectible : MonoBehaviour
{
    [Header("Cash Settings")]
    [SerializeField] private int cashAmount = 10;

    [Header("Collect Sound Effect")]
    [SerializeField] private AudioClip collectSound;

    private bool collected = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (!other.CompareTag("Player")) return;

        Collect();
    }

    void Collect()
    {
        collected = true;

        if (LevelCashManager.Instance != null)
        {
            LevelCashManager.Instance.CollectCash(cashAmount);
        }
        else
        {
            Debug.LogWarning("No LevelCashManager found in scene!");
        }

        if (collectSound != null)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

        Destroy(gameObject);
    }
}