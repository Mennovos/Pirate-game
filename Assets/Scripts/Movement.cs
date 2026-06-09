using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] public float mashAmount = 0f;
    [SerializeField] private float mashCooldown = 3f;


    private Vector2 moveInput;
    private Vector3 movement;

    private Controls Controls;
    private Health health;

    private LayerMask GroundLayer;
    private Rigidbody Rb;
    [SerializeField] private Animator Anim;

    private bool Grappling;
    private bool Grounded;

    [SerializeField] private bool batHit;
    //for mashing text
    // [SerializeField] private TextMeshProUGUI E;


    private void Awake()
    {
        GroundLayer = LayerMask.GetMask("Ground");
        health = FindAnyObjectByType<Health>();

        Controls = new Controls();

        Controls.Player.Enable();
        Controls.Player.Move.performed += OnMove;
        Controls.Player.Move.canceled += OnMove;
        Controls.Player.Jump.performed += OnJump;
        Controls.Player.Mashing.performed += OnMashing;    

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
          mashCooldown -= Time.deltaTime;
        }


        transform.position += movement * (speed * Time.deltaTime);

        if (movement.magnitude > 0.001f)
        {
            Anim.SetBool("Walking", true);
            transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(movement, Vector3.up), Vector3.up);
        }
        else 
        {
            Anim.SetBool("Walking", false);
        }

        if (!Grounded)
        {
            Anim.SetBool("Falling", true);
            Anim.SetBool("Walking", false); 
        }
        else
        {
            Anim.SetBool("Falling", false);
        }

        //make text work for  mashing thingie


        //if (mashCooldown < 1 && mashCooldown > -1)
        //{
        //    E.text = "Mash the button to escape!";
        //}
        //else
        //{
        //    E.text = "                     E";
        //}


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
        if (Grounded)
        {
            Anim.SetTrigger("Jumping");
            Rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        }
    public void OnMashing(InputAction.CallbackContext context)
    {
        if (batHit)
        DealDamageIfNotCooldown();
        if (mashCooldown < 3)
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
    private void DealDamageIfNotCooldown() {     
        if (mashCooldown < -1)
        {
            health.TakeDamage(10);
        }
        else if (mashCooldown > 1)
        {
            health.TakeDamage(10);
        }
        if (mashCooldown < 1 && mashCooldown > -1)
        {
            health.TakeDamage(0);
        }
    }
}
