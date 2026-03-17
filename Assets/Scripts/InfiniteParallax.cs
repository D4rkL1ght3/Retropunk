using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    public Transform cameraTransform;
    public float parallaxMultiplierX;
    public float parallaxMultiplierY;

    private float spriteWidth;
    private Vector3 lastCameraPosition;

    void Start()
    {
        spriteWidth = GetComponentInChildren<SpriteRenderer>().bounds.size.x;
        lastCameraPosition = cameraTransform.position;
    }

    void Update()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxMultiplierX, deltaMovement.y * parallaxMultiplierY, 0f);

        lastCameraPosition = cameraTransform.position;

        // Looping logic
        if (cameraTransform.position.x - transform.position.x > spriteWidth)
        {
            transform.position += new Vector3(spriteWidth, 0f, 0f);
        }
        else if (cameraTransform.position.x - transform.position.x < -spriteWidth)
        {
            transform.position -= new Vector3(spriteWidth, 0f, 0f);
        }
    }
}
