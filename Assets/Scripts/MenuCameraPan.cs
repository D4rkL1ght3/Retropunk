using UnityEngine;

public class MenuCameraPan : MonoBehaviour
{
    public Transform cameraTransform;
    public float panSpeed = 1f;

    private float startX;
    private int direction = 1;

    void Start()
    {
        startX = cameraTransform.position.x;
    }

    void Update()
    {
        cameraTransform.position += new Vector3(direction * panSpeed * Time.deltaTime, 0f, 0f);
    }
}