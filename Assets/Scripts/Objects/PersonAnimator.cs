using UnityEngine;

public class PersonAnimator : MonoBehaviour
{

    [SerializeField] private Animator _animator;

    public Vector2 movement;
    public bool onSwing;
    private float _angle;

    void Update()
    {
        _animator.SetBool("IsMoving", !(movement.x == 0 && movement.y == 0));

        _animator.SetFloat("AnimMoveX", movement.x);
        _animator.SetFloat("AnimMoveY", movement.y);


        //if (onSwing) onSwing = false;
        
       // _animator.SetBool("CaneSmash", );
    }
 
}

