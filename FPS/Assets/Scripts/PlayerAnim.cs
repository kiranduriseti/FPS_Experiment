using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    private Animator anim;
    private CharacterController controller;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponentInParent<CharacterController>();
    }

    public void SetCasting(bool casting)
    {
        anim.SetBool("Cast", casting);
    }

    public void SetPlayerDead(bool dead)
    {
        anim.SetTrigger("Dead");
    }

    public void SetAnimSpeed(float target)
    {
        anim.SetFloat("Speed", target);
    }

    public void setTrigger(string trigger)
    {
        anim.SetTrigger(trigger);
    }

    private void Update()
    {
        if (controller == null) return;

        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;
        anim.SetFloat("Speed", horizontalVelocity.magnitude);
    }
}