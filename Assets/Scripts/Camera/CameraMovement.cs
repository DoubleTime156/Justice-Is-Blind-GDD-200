using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    public float speed;

    private Vector3 _position;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _position = transform.position;
    }

    void Update()
    {
        // Get mouse position
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        _position = (player.position + mouseWorldPos)/2.0f;
        _position.z = -10;

        transform.position = Vector3.Lerp(transform.position, _position, speed * Time.deltaTime);
    }
}
