using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float speed = 5f;

    private Vector2 moveInput;
    private Vector3 movement;

   private Controls Controls;

    private Rigidbody Rb;
    private Animator Anim;
    public bool Grappling;
    private bool Restraint;

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        Controls = new Controls();
        Controls.Player.Enable();
        Controls.Player.Move.performed += OnMove;
        Controls.Player.Move.canceled += OnMove;
        Controls.Player.Jump.performed += OnJump;
        // Controls.Player.Grapple.performed += Grapple;

        Rb = GetComponent<Rigidbody>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        movement.x = context.ReadValue<Vector2>().x;
        movement.z = context.ReadValue<Vector2>().y;
        bool Walking = input.sqrMagnitude > 0.01f;
       // Anim.SetBool("Walking", Walking);
    }
    public void OnJump(InputAction.CallbackContext context)
    {
      Rb.AddForce(Vector3.up * 5f, ForceMode.Impulse);
      Anim.SetTrigger("Jump");
    }

    void Update()
    {
        transform.Translate(movement * speed * Time.deltaTime);
        if (cameraTransform != null)
        {
            Vector3 PlayerRotation = transform.eulerAngles;
            PlayerRotation.y = cameraTransform.eulerAngles.y;
            transform.eulerAngles = PlayerRotation;
        }
    }

    //IEnumerator GrappleCooldown()
    //{
    //    Anim.SetTrigger("Grapple");
    //    yield return new WaitForSeconds(1f);
    //    Grappling = true;
    //    yield return new WaitForSeconds(0.01f);
    //    Grappling = false;
    //}
    //IEnumerator AttackCooldown(string Attackname, float Cooldowntime)
    //{
    //    Anim.SetTrigger(Attackname);
    //    Restraint = true;
    //    yield return new WaitForSeconds(Cooldowntime);
    //    Restraint = false;
    //}
    //public void Grapple(InputAction.CallbackContext context)
    //{
    //    if (context.performed)
    //    {
    //        StartCoroutine(GrappleCooldown());
    //    }
    //}
}
