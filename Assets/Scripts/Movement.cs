using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float mashAmount = 0f;
    [SerializeField] private float mashCooldown = 3f;


    private Vector2 moveInput;
    private Vector3 movement;

    private Controls Controls;
    private LayerMask GroundLayer;

    private Rigidbody Rb;
    private Animator Anim;

    private bool Grappling;
    private bool Grounded;

    [SerializeField] private bool batHit;


    private void Awake()
    {
        mashAmount = 1;
        GroundLayer = LayerMask.GetMask("Ground");
        Anim = GetComponent<Animator>();

        Controls = new Controls();

        Controls.Player.Enable();
        Controls.Player.Move.performed += OnMove;
        Controls.Player.Move.canceled += OnMove;
        Controls.Player.Jump.performed += OnJump;
        Controls.Player.Mashing.performed += OnMashing;    
        // Controls.Player.Grapple.performed += Grapple;

        Rb = GetComponent<Rigidbody>();
    }
    private void FixedUpdate()
    {
        Grounded = Physics.Raycast(transform.position, Vector3.down, 1.5f, GroundLayer);
    }
    private void Update()
    {
        if (batHit)
        {
            if (mashCooldown < -1)
            {
                mashCooldown = -1;
            }
            else
            {
                mashCooldown -= Time.deltaTime;
            }
        }


        transform.position += movement * (speed * Time.deltaTime);

        if (movement.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(
                Vector3.ProjectOnPlane(movement, Vector3.up), Vector3.up);
        }


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
    public void OnMashing(InputAction.CallbackContext context)
    {
         if (mashCooldown < 0)
         {
           mashAmount++;
           mashCooldown = 3f;
         }
        if (mashAmount == 4)
        {
            batHit = false;
            mashAmount = 0;
        }
    }

    public float mashClicks()
    {
        return (float)mashAmount;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EnemyBat"))
        {
            batHit = true;
        }
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
