using UnityEngine;

public class Door : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D entryPoint;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        entryPoint = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            spriteRenderer.sortingOrder = 6;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            spriteRenderer.sortingOrder = 0;
        }
    }
}
