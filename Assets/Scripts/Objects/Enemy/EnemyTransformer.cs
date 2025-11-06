using UnityEngine;

public class EnemyTransformer : MonoBehaviour
{
    [SerializeField] private EnemyData data;
    
    private GameObject _lightVision;

    private float _speed;
    private float _rotateSpeed;

    void Awake()
    {
        _speed = data.chaseSpeed;
        _rotateSpeed = data.rotateSpeed;
        _lightVision = transform.Find("LightVision").gameObject;
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void UpdateMovement(Vector3 dir)
    {
        transform.position += _speed * dir;
    }

    public void UpdateDirection(Vector3 dir)
    {
        Vector2 newDir = Vector2.Lerp(_lightVision.transform.up, dir.normalized, _rotateSpeed);
        _lightVision.transform.up = newDir;

    }
}
