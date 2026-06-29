using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public class Movement : MonoBehaviour
{
    private LayerMask grappleLayerMask;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    public float mashAmount;
    [SerializeField] private float mashCooldown = 3f;
 
    private Vector3 movement;

    private Controls controls;
    private Health health;

    private LayerMask groundLayer;
    private Rigidbody rb;
    [SerializeField] private Animator anim;

    public Rigidbody Rigidbody => rb;

    private bool grounded;
    private bool walking;
    private bool batHit;
    private bool isPaused;

    [SerializeField] private GameObject menuManger;
    
    [Space]
    [SerializeField] private List<AudioClip> jumpSounds;
    private List<AudioClip> walkSounds;
    [SerializeField, Min(float.Epsilon)] private float walkSoundInterval;
    [SerializeField] private AudioSource audioSource;
    //For mashing ui


    private void Awake()
    {
        groundLayer = LayerMask.GetMask("Ground");
        grappleLayerMask = LayerMask.GetMask("Grappling");
        health = FindAnyObjectByType<Health>();

        controls = new Controls();

        controls.Player.Enable();
            controls.Player.Move.performed += OnMove;
            controls.Player.Move.canceled += OnMove;
            controls.Player.Jump.performed += OnJump;
            controls.Player.Mashing.performed += OnMashing;
            controls.Player.Pause.performed += OnPause;

        rb = GetComponent<Rigidbody>();
    }
    public void OnIspaused()
    {
       isPaused = true;
    }
    public bool IsPaused()
    {
        return (bool)isPaused;
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        if (!isPaused)
        {
            movement.x = context.ReadValue<Vector2>().x;
            walking = input.sqrMagnitude > 0.01f;
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (grounded && !isPaused)
        {
            anim.SetTrigger("Jumping");
            
            if (audioSource && jumpSounds.Count > 0)
            {
                int index = Random.Range(0, jumpSounds.Count);
                AudioClip clip = jumpSounds[index];
        
                audioSource.PlayOneShot(clip);
            }
            
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        Debug.Log("Pause button pressed");
        isPaused = !isPaused;
        TimeManager.Instance.SetPaused(!TimeManager.Instance.IsPaused);
        Time.timeScale = TimeManager.Instance.IsPaused ? 0f : 1f;
    }

    private void FixedUpdate()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 
            1.5f, groundLayer);

        if (grounded)
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out GroundSoundSupplier soundSupplier))
            {
                walkSounds = soundSupplier.WalkSounds;
            }
            else
            {
                walkSounds = new List<AudioClip>();
            }
        }
    }
   
    private void Update()
    {  
        mashCooldown -= Time.deltaTime;

        transform.position += movement * (speed * Time.deltaTime);

        if (movement.magnitude > 0.001f)
        {
            anim.SetBool("Walking", true);
        }
        else 
        {
            anim.SetBool("Walking", false);
        }

        if (!grounded)
        {
            anim.SetBool("Falling", true);
            anim.SetBool("Walking", false); 
        }
        else
        {
            anim.SetBool("Falling", false);
        }
        if(mashAmount == 5)
        {
            isPaused = false;
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


    private IEnumerator WalkSoundCoroutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => grounded && walking);

            if (audioSource && walkSounds.Count > 0)
            {
                int index = Random.Range(0, walkSounds.Count);
                AudioClip clip = walkSounds[index];
        
                audioSource.PlayOneShot(clip);
            }

            yield return new WaitForSeconds(walkSoundInterval);
        }
    }
}
