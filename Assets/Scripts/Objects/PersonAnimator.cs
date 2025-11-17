using UnityEngine;

public class PersonAnimator : MonoBehaviour
{
    public bool isMoving;

    [SerializeField] private Animator _animator;

    private float _angle;

    void Update()
    {
        // Default scale
        transform.localScale = new Vector3(1, 1, 1);

        // Check if moving
        if (!isMoving)
        {
            _animator.SetBool("IsUp", false);
            _animator.SetBool("IsDown", false);
            _animator.SetBool("IsLeftRight", false);
            return;
        }

        // Convert to -180–180
        _angle = Mathf.DeltaAngle(0f, transform.localRotation.eulerAngles.z);

        // Check direction
        bool right = _angle >= -45f && _angle < 45f;
        bool up = _angle >= 45f && _angle < 135f;
        bool left = _angle >= 135f || _angle < -135f;
        bool down = _angle >= -135f && _angle < -45f;

        // Flip left animation
        if (left) transform.localScale = new Vector3(-1, 1, 1);

        // Set Up and Down bools
        _animator.SetBool("IsUp", up);
        _animator.SetBool("IsDown", down);

        // Left or Right takes over Up and Down
        if (left || right)
        {
            _animator.SetBool("IsLeftRight", true);
            _animator.SetBool("IsUp", false);
            _animator.SetBool("IsDown", false);
        }
    }
}
