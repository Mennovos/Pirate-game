using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;

    private Vector2 moveInput;
    private Vector3 movement;

    private Controls Controls;
    private LayerMask GroundLayer;

    private Rigidbody Rb;
    private Animator Anim;

    private bool Grappling;
    private bool Grounded;

    private void Awake()
    {  
        GroundLayer = LayerMask.GetMask("Ground");
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
       // movement.z = context.ReadValue<Vector2>().y;
        bool Walking = input.sqrMagnitude > 0.01f;
       // Anim.SetBool("Walking", Walking);
    }
    public void OnJump(InputAction.CallbackContext context)
    { 
        if(Grounded)
            Rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        //here you can make the attack animation and logic

    }

    private void Update()
    {
        transform.Translate(movement * speed * Time.deltaTime);

        if (Restraint)
        {
            Controls.Player.Disable();
        }
    }
    private void FixedUpdate()
    {
        Grounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, GroundLayer);
    }



    // future grapple code neglect for now 

    //IEnumerator GrappleCooldown()
    //{
    //    Anim.SetTrigger("Grapple");
    //    yield return new WaitForSeconds(1f);
    //    Grappling = true;
    //    yield return new WaitForSeconds(0.01f);
    //    Grappling = false;
    //}

    //public void Grapple(InputAction.CallbackContext context)
    //{
    //    if (context.performed)
    //    {
    //        StartCoroutine(GrappleCooldown());
    //    }
    //}
}
