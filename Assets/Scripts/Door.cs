using UnityEngine;

public class Door : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D entryPoint;
    public bool enterDoor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        entryPoint = GetComponent<BoxCollider2D>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (enterDoor)
                spriteRenderer.sortingOrder = 8;
            else
                spriteRenderer.sortingOrder = 0;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (enterDoor)
                spriteRenderer.sortingOrder = 0;
            else
                spriteRenderer.sortingOrder = 8;
        }
    }
}
