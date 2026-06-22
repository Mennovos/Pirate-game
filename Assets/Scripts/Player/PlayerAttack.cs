using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField, Min(0f)] private float duration;
    [SerializeField, Min(0f)] private float cooldown;
    [SerializeField, Min(0f)] private float strength;
    
    [Space]
    [SerializeField] private List<AudioClip> attackSounds;
    [SerializeField] private List<AudioClip> hitSounds;
    [SerializeField] private AudioSource audioSource;
    
    private Controls controls;
    
    private Collider coll;
    private Camera cam;

    private Coroutine coroutine;

    private void Awake()
    {
        controls = new Controls();
        coll = GetComponent<Collider>();
        cam = Camera.main;
        
        coll.enabled = false;
        
        controls.Enable();

        controls.Player.Attack.started += OnAttack;
    }

    private void OnDestroy()
    {
        controls.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IEnemy enemy))
        {
            if (audioSource && hitSounds.Count > 0)
            {
                int index = Random.Range(0, hitSounds.Count);
                AudioClip clip = hitSounds[index];
        
                audioSource.PlayOneShot(clip);
            }
            
            enemy.attack(transform.forward * strength);
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        anim.ResetTrigger("Attacking");
        anim.SetTrigger("Attacking");
        coroutine ??= StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        Vector2 mousePos = controls.Player.MausePos.ReadValue<Vector2>();
        
        transform.rotation = Quaternion.LookRotation(
            -(transform.position - cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -cam.transform.position.z))), 
            Vector3.up);
            
        if (audioSource && attackSounds.Count > 0)
        {
            int index = Random.Range(0, attackSounds.Count);
            AudioClip clip = attackSounds[index];
        
            audioSource.PlayOneShot(clip);
        }
        
        coll.enabled = true;
        yield return new WaitForSeconds(duration);
        
        coll.enabled = false;
        yield return new WaitForSeconds(cooldown);
        
        coroutine = null;
    }
}
