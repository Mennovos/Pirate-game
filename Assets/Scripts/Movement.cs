using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class Movement : MonoBehaviour
{
    private LayerMask grappleLayerMask;

    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float mashCooldown = 3f;
    public float mashAmount;
 
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

    // Mash-challenge (when an enemy grabs the player)
    private bool mashChallengeActive;
    private int mashChallengeRequired;
    private float mashChallengeTimer;
    private int mashChallengeDamage;
    private Coroutine mashChallengeCoroutine;
    private Action<bool> mashChallengeCallback;

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

    private void OnDestroy()
    {
        controls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        // Keep reading move input but only apply it when not paused.
        if (!isPaused)
        {
            movement.x = context.ReadValue<Vector2>().x;
            walking = input.sqrMagnitude > 0.01f;
        }
        else if (context.canceled)
        {
            // ensure movement cleared on cancel even if paused
            movement.x = 0f;
            walking = false;
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
        if (!context.performed) return; // only toggle on performed

        Debug.Log("Pause button pressed");

        // Toggle global TimeManager pause and local flag consistently
        bool newPaused = !TimeManager.Instance.IsPaused;
        TimeManager.Instance.SetPaused(newPaused);
        isPaused = newPaused;

        if (isPaused)
        {
            // Immediately stop motion and disable walking animation so we don't keep moving
            movement = Vector3.zero;
            walking = false;
            anim.SetBool("Walking", false);

            // Clear rigidbody velocity to prevent sliding/bouncing while paused
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            // Unpaused: nothing special to restore; input will resume updating movement.
        }
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

        // Do not apply movement while paused.
        if (!isPaused)
        {
            transform.position += movement * (speed * Time.deltaTime);

            if (movement.magnitude > 0.001f)
            {
                anim.SetBool("Walking", true);
            }
            else 
            {
                anim.SetBool("Walking", false);
            }
        }
        else
        {
            // ensure walking animation off while paused
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
        // If a mash challenge is active, count the press toward it.
        if (context.performed)
        {
            if (mashChallengeActive)
            {
                mashAmount++;
                return;
            }

            // Legacy behavior: keep previous mash behavior for other systems
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
    }

    public float mashClicks()
    {
        return (float)mashAmount;
    }

    /// <summary>
    /// Start a timed mash challenge. Player must press the mash button
    /// <paramref name="requiredMashes"/> times within <paramref name="timeWindow"/> seconds,
    /// otherwise they receive <paramref name="damageOnFail"/> damage.
    /// Movement is paused during the challenge.
    /// The optional onComplete callback receives true on success, false on fail.
    /// </summary>
    public void StartMashingChallenge(int requiredMashes = 4, float timeWindow = 3f, int damageOnFail = 10, Action<bool> onComplete = null)
    {
        // If already active, do nothing
        if (mashChallengeActive) return;

        mashChallengeRequired = requiredMashes;
        mashChallengeTimer = timeWindow;
        mashChallengeDamage = damageOnFail;
        mashAmount = 0f;
        mashChallengeActive = true;
        mashChallengeCallback = onComplete;

        // Pause player input/movement
        isPaused = true;

        // Ensure any existing coroutine is stopped
        if (mashChallengeCoroutine != null) StopCoroutine(mashChallengeCoroutine);
        mashChallengeCoroutine = StartCoroutine(MashChallengeRoutine());
    }

    public void CancelMashingChallenge()
    {
        if (!mashChallengeActive) return;
        mashChallengeActive = false;
        mashAmount = 0f;
        if (mashChallengeCoroutine != null) { StopCoroutine(mashChallengeCoroutine); mashChallengeCoroutine = null; }
        // invoke callback as failure
        mashChallengeCallback?.Invoke(false);
        mashChallengeCallback = null;
        isPaused = false;
    }

    private IEnumerator MashChallengeRoutine()
    {
        float t = 0f;
        while (t < mashChallengeTimer)
        {
            t += Time.deltaTime;
            // Optionally show UI here using mashAmount / mashChallengeRequired
            yield return null;
        }

        // Challenge ended: check result
        bool success = mashAmount >= mashChallengeRequired;
        if (!success)
        {
            // failed -> take damage
            health?.TakeDamage(mashChallengeDamage);
        }

        // notify caller
        mashChallengeCallback?.Invoke(success);
        mashChallengeCallback = null;

        // cleanup
        mashChallengeActive = false;
        mashChallengeCoroutine = null;
        mashAmount = 0f;
        isPaused = false;
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
