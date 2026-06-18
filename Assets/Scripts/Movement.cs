using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;


public class Movement : MonoBehaviour
{
    LayerMask grappleLayerMask;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] public float mashAmount = 0f;
    [SerializeField] private float mashCooldown = 3f;
 
    private Vector3 movement;

    private Controls Controls;
    private Health health;

    private LayerMask GroundLayer;
    private Rigidbody Rb;
    [SerializeField] private Animator Anim;

    private bool Grounded;
    private bool batHit;

    [SerializeField] private GameObject menuManger;

    //for mashing text
    // [SerializeField] private TextMeshProUGUI E;


    private void Awake()
    {
        GroundLayer = LayerMask.GetMask("Ground");
        grappleLayerMask = LayerMask.GetMask("Grappling");
        health = FindAnyObjectByType<Health>();

        Controls = new Controls();

        Controls.Player.Enable();
        if(TimeManager.Instance.IsPaused == false)
        {
            Controls.Player.Move.performed += OnMove;
            Controls.Player.Move.canceled += OnMove;
            Controls.Player.Jump.performed += OnJump;
            Controls.Player.Mashing.performed += OnMashing;
            Controls.Player.Pause.performed += OnPause;
        }

        Rb = GetComponent<Rigidbody>();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        movement.x = context.ReadValue<Vector2>().x;
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

    public void OnPause(InputAction.CallbackContext context)
    {
        Debug.Log("Pause button pressed");
        TimeManager.Instance.SetPaused(!TimeManager.Instance.IsPaused);
        Time.timeScale = TimeManager.Instance.IsPaused ? 0f : 1f;
    }

    private void FixedUpdate()
    {
        Grounded = Physics.Raycast(transform.position, Vector3.down, 1.5f, GroundLayer);

    }
   
    private void Update()
    {  
        mashCooldown -= Time.deltaTime;

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
    public void OnMashing(InputAction.CallbackContext context)
    {
        if (batHit)
            DealDamageIfNotCooldown();
        
        if (mashCooldown < 3)
        {
           mashAmount++;
           mashCooldown = 3f;
        }
        
        if (mashAmount >= 4)
        {
            batHit = false;
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
